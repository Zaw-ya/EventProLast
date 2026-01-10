using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventPro.Business.MemoryCacheStore.Implementaiion;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.RabbitMQMessaging.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Implementation;
using EventPro.Business.WhatsAppMessagesWebhook.Implementation;
using EventPro.Business.WhatsAppMessagesWebhook.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Configuration;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    public class WebHookSingleMessagingQueueConsumerService : IWebHookSingleMessagingQueueConsumerService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IWebHookService _twilioWebHookService;
        private static int BulkSendingCounter;
        private readonly EventProContext db;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheStoreService _memoryCacheStoreService;
        private readonly IServiceScopeFactory _ServiceScopeFactory;
        private readonly String QueueName;
        private readonly UrlProtector _urlProtector;
        private readonly DistributedLockHelper _distributedLockHelper;
        public WebHookSingleMessagingQueueConsumerService(IConnectionFactory connectionFactory,
            IConfiguration configuration, IMemoryCacheStoreService memoryCacheStoreService,
            IServiceScopeFactory serviceScopeFactory, UrlProtector urlProtector,DistributedLockHelper distributedLockHelper,
            IDbContextFactory<EventProContext> dbFactory)
        {
            _urlProtector = urlProtector;
            _connectionFactory = connectionFactory;
            _configuration = configuration;
            _memoryCacheStoreService = memoryCacheStoreService;
            _distributedLockHelper = distributedLockHelper;
            _twilioWebHookService = new TwilioWebhookService(configuration,
                new WhatsappSendingProvidersService(_configuration, _memoryCacheStoreService, _urlProtector), _distributedLockHelper, dbFactory);
            _ServiceScopeFactory = serviceScopeFactory;
            db = new EventProContext(_configuration);
            QueueName = _configuration.GetSection("RabbitMqQueues")["TwilioSingleMessagingWebHookMessages"].ToLower();
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
            channel.BasicQos(prefetchSize: 0, prefetchCount: 7, global: false);
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    MessageMetadataRequest messageRequest = JsonConvert
                   .DeserializeObject<MessageMetadataRequest>(message);

                    using (var scope = _ServiceScopeFactory.CreateScope())
                    {
                        var twilioWebHookService = scope.ServiceProvider.GetRequiredService<ITwilioWebhookService>();

                        if (!string.IsNullOrEmpty(messageRequest.MessageStatus) &&
                             string.IsNullOrEmpty(messageRequest.Body))
                        {
                            await twilioWebHookService.ProcessStatusAsync(messageRequest);
                        }
                        else
                        {
                            await twilioWebHookService.ProcessIncomingMessageAsync(messageRequest);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"this is rabbit mq ex {ex}");
                }
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

            };

            channel.BasicConsume(queue: QueueName , autoAck: false, consumer: consumer);
            return;
        }
    }
}
