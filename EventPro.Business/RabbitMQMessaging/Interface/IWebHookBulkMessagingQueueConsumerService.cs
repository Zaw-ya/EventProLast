namespace EventPro.Business.RabbitMQMessaging.Interface
{
    public interface IWebHookBulkMessagingQueueConsumerService
    {
        public void ConsumeMessage();
        public void Resume();
        public void Pause();
        public bool IsValidSendingBulkMessages();
        public bool IsValidConsumingBulkMessages();
        public void ForceStart();
        public void ForceStop();
    }
}
