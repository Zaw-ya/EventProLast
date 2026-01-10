namespace EventPro.Business.RabbitMQMessaging.Interface
{
    public interface IWebHookSingleMessagingQueueConsumerService
    {
        public void ConsumeMessage();
    }
}
