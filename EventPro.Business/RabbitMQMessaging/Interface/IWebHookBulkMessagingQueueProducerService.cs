using Microsoft.Extensions.Configuration;

namespace EventPro.Business.RabbitMQMessaging.Interface
{
    public interface IWebHookBulkMessagingQueueProducerService
    {
        public Task SendingMessageAsync(object message);
    }
}
