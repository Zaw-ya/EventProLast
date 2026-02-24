using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EventPro.Business.RabbitMQMessaging.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    public class MessageSendingBulkQueueConsumerService : IMessageSendingBulkQueueConsumerService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWebHookBulkMessagingQueueConsumerService _webHookBulkMessagingQueueConsumerService;
        private readonly string QueueName = "MessageSendingBulk";

        public MessageSendingBulkQueueConsumerService(
            IConnectionFactory connectionFactory,
            IServiceScopeFactory serviceScopeFactory,
            IWebHookBulkMessagingQueueConsumerService webHookBulkMessagingQueueConsumerService)
        {
            _connectionFactory = connectionFactory;
            _serviceScopeFactory = serviceScopeFactory;
            _webHookBulkMessagingQueueConsumerService = webHookBulkMessagingQueueConsumerService;
        }

        public void ConsumeMessage()
        {
            var connection = _connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: QueueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 5, global: false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    _webHookBulkMessagingQueueConsumerService.Pause();
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    var request = JsonConvert.DeserializeObject<MessageSendingRequest>(message);

                    if (request == null)
                    {
                        Log.Warning("Received null MessageSendingRequest from bulk queue");
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        return;
                    }

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<EventProContext>>();
                        var whatsappProvider = scope.ServiceProvider.GetRequiredService<IWhatsappSendingProviderService>();

                        using (var db = dbFactory.CreateDbContext())
                        {
                            var eventData = await db.Events
                                .AsNoTracking()
                                .FirstOrDefaultAsync(e => e.Id == request.EventId);

                            if (eventData == null)
                            {
                                Log.Warning($"Event {request.EventId} not found for bulk MessageSendingRequest");
                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                return;
                            }

                            var guests = await db.Guest
                                .Where(g => request.GuestIds.Contains(g.GuestId))
                                .ToListAsync();

                            if (!guests.Any())
                            {
                                Log.Warning($"No guests found for GuestIds: {string.Join(",", request.GuestIds)}");
                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                return;
                            }

                            var sendingProvider = await whatsappProvider.SelectConfiguredSendingProviderAsync(eventData);

                            switch (request.MessageType)
                            {
                                case MessageSendingType.Card:
                                    await sendingProvider.SendCardMessagesAsync(guests, eventData);
                                    break;
                                case MessageSendingType.EventLocation:
                                    await sendingProvider.SendEventLocationAsync(guests, eventData);
                                    break;
                                case MessageSendingType.Reminder:
                                    await sendingProvider.SendReminderMessageAsync(guests, eventData);
                                    break;
                                case MessageSendingType.Confirmation:
                                    await sendingProvider.SendConfirmationMessagesAsync(guests, eventData);
                                    break;
                                case MessageSendingType.Congratulation:
                                    await sendingProvider.SendCongratulationMessageAsync(guests, eventData);
                                    break;
                                case MessageSendingType.Decline:
                                    await sendingProvider.SendDeclineMessageAsync(guests, eventData);
                                    break;
                                case MessageSendingType.Duplicate:
                                    await sendingProvider.SendDuplicateAnswerAsync(guests, eventData);
                                    break;
                                default:
                                    Log.Warning($"Unknown MessageSendingType: {request.MessageType}");
                                    break;
                            }

                            Log.Information($"Processed bulk {request.MessageType} message for Event {request.EventId}, Guests: {string.Join(",", request.GuestIds)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error processing bulk MessageSendingRequest from RabbitMQ");
                }
                finally
                {
                    _webHookBulkMessagingQueueConsumerService.Resume();
                }

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        }
    }
}
