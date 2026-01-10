using Microsoft.Extensions.Configuration;

namespace EventPro.Business.RabbitMQMessaging.Interface
{
    public interface IWebHookSingleMessagingQueueProducerService
    {
        public Task SendingMessageAsync(object message);
    }
}
