namespace EventPro.Business.RabbitMQMessaging.Interface
{
    #region Interface Summary
    /// <summary>
    /// Interface for Single Message Queue Consumer Service.
    /// This interface defines the contract for consuming individual webhook messages
    /// from a RabbitMQ queue and processing them.
    ///
    /// Purpose:
    /// - Consumes Twilio webhook messages from the single messaging queue
    /// - Processes message status updates (delivered, failed, read, etc.)
    /// - Handles incoming WhatsApp/SMS messages from customers
    ///
    /// Queue Name: TwilioSingleMessagingWebHookMessages (configured in appsettings)
    /// Prefetch Count: 7 messages (processes up to 7 messages concurrently)
    /// </summary>
    #endregion
    public interface IWebHookSingleMessagingQueueConsumerService
    {
        #region Methods

        /// <summary>
        /// Starts consuming messages from the RabbitMQ queue.
        /// This method sets up the consumer with event handlers and begins
        /// listening for incoming messages. It runs continuously until the
        /// application is stopped.
        ///
        /// Processing Logic:
        /// - If message has MessageStatus and no Body: Process as status update
        /// - If message has Body: Process as incoming message from customer
        /// </summary>
        public void ConsumeMessage();

        #endregion
    }
}
