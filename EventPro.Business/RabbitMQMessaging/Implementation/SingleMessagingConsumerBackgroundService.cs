using Microsoft.Extensions.Hosting;
using EventPro.Business.RabbitMQMessaging.Interface;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    public class SingleMessagingConsumerBackgroundService : BackgroundService
    {
        private readonly IWebHookSingleMessagingQueueConsumerService _WebHookQueueConsumerService;

        public SingleMessagingConsumerBackgroundService(IWebHookSingleMessagingQueueConsumerService webHookQueueConsumerService)
        {
            _WebHookQueueConsumerService = webHookQueueConsumerService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _WebHookQueueConsumerService.ConsumeMessage();
        }
    }
}
