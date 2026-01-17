using Microsoft.Extensions.Configuration;

namespace EventPro.Business.RabbitMQMessaging.Interface
{
    #region Interface Summary
    /// <summary>
    /// Interface for Bulk Message Queue Producer Service.
    /// This interface defines the contract for publishing bulk webhook messages
    /// to a RabbitMQ queue for asynchronous processing.
    ///
    /// Purpose:
    /// - Handles Twilio webhook messages for bulk SMS/WhatsApp campaigns
    /// - Used when sending messages to multiple recipients at once
    /// - Supports controlled message processing with pause/resume capabilities
    ///
    /// Queue Name: TwilioBulkMessagingWebHookMessages (configured in appsettings)
    ///
    /// Note: This queue is used in conjunction with IWebHookBulkMessagingQueueConsumerService
    /// which provides flow control to limit concurrent bulk operations.
    /// </summary>
    #endregion
    public interface IWebHookBulkMessagingQueueProducerService
    {
        #region Methods

        /// <summary>
        /// Sends a message asynchronously to the RabbitMQ bulk messaging queue.
        /// The message is serialized to JSON and published for later processing.
        /// </summary>
        /// <param name="message">
        /// The message object to be sent. Typically a MessageMetadataRequest containing:
        /// - MessageStatus: Status updates from Twilio bulk campaigns
        /// - Body: Content for incoming messages
        /// - Other webhook metadata from Twilio
        /// </param>
        /// <returns>A Task representing the asynchronous publish operation</returns>
        public Task SendingMessageAsync(object message);

        #endregion
    }
}
