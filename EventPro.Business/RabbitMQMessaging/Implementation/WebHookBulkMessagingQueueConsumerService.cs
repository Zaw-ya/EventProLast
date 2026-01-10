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


namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    public class WebHookBulkMessagingQueueConsumerService : IWebHookBulkMessagingQueueConsumerService
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
        public WebHookBulkMessagingQueueConsumerService(IConnectionFactory connectionFactory,
            IConfiguration configuration, IMemoryCacheStoreService memoryCacheStoreService,
            IServiceScopeFactory serviceScopeFactory, UrlProtector urlProtector,
            DistributedLockHelper distributedLockHelper,
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
            QueueName = _configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower();
          // _memoryCacheStoreService.save(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower(), 0);
        }

        public void Pause()
        {
          var counter = _memoryCacheStoreService.Retrieve(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower());
            counter++;
            _memoryCacheStoreService.save(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower(), counter);
            // BulkSendingCounter++;
        }

        public void Resume()
        {
            var counter = _memoryCacheStoreService.Retrieve(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower());
            counter--;
            _memoryCacheStoreService.save(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower(), counter);
        //    BulkSendingCounter--;
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
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    MessageMetadataRequest messageRequest = JsonConvert
                   .DeserializeObject<MessageMetadataRequest>(message);

                    using (var scope = _ServiceScopeFactory.CreateScope())
                    {
                        var twilioWebHookService = scope.ServiceProvider.GetRequiredService<ITwilioWebhookService>();
                        var consumer = scope.ServiceProvider.GetRequiredService<IWebHookBulkMessagingQueueConsumerService>();

                        if (!consumer.IsValidConsumingBulkMessages())
                        {
                            await Task.Delay(1000);
                            channel.BasicNack(ea.DeliveryTag, false, true);
                            return;
                        }


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

            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            return;
        }

        public bool IsValidSendingBulkMessages()
        {
            var NumberOfOpertorToSendBulkOnSameTime = db.AppSettings
                .AsNoTracking()
                .Select(e => e.NumberOfOpertorToSendBulkOnSameTime)
                .FirstOrDefault();
            var counter = _memoryCacheStoreService.Retrieve(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower());

            if (counter >= NumberOfOpertorToSendBulkOnSameTime)
                return false;
            return true;
        }

        public void ForceStart()
        {
        _memoryCacheStoreService.save(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower(),0);
         //   BulkSendingCounter = 0;
        }

        public void ForceStop()
        {
            _memoryCacheStoreService.save(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower(), 1000000000);
           // BulkSendingCounter = 1000000000;
        }

        public bool IsValidConsumingBulkMessages()
        {
            var counter = _memoryCacheStoreService.Retrieve(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower());
            if (counter > 0)
                return false;
            return true;
        }
    }
}
