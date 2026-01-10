using Microsoft.Extensions.Hosting;
using EventPro.Business.RabbitMQMessaging.Interface;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    public class BulkMessagingConsumerBackgroundService : BackgroundService
    {
        private readonly IWebHookBulkMessagingQueueConsumerService _WebHookQueueConsumerServices;

        public BulkMessagingConsumerBackgroundService(IWebHookBulkMessagingQueueConsumerService webHookQueueConsumerServices)
        {
            _WebHookQueueConsumerServices = webHookQueueConsumerServices;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _WebHookQueueConsumerServices.ConsumeMessage();
        }
    }
}
