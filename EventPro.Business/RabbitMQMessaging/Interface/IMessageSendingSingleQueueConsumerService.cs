namespace EventPro.Business.RabbitMQMessaging.Interface
{
    public interface IMessageSendingSingleQueueConsumerService
    {
        void ConsumeMessage();
    }
}
