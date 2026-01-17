using Microsoft.Extensions.Configuration;

namespace EventPro.Business.RabbitMQMessaging.Interface
{
    #region Interface Summary
    /// <summary>
    /// Interface for Single Message Queue Producer Service.
    /// This interface defines the contract for publishing individual webhook messages
    /// to a RabbitMQ queue for asynchronous processing.
    ///
    /// Purpose:
    /// - Handles Twilio webhook messages for single/individual SMS/WhatsApp communications
    /// - Decouples message reception from processing to improve system responsiveness
    /// - Enables asynchronous processing of webhook callbacks from Twilio
    ///
    /// Queue Name: TwilioSingleMessagingWebHookMessages (configured in appsettings)
    /// </summary>
    #endregion
    public interface IWebHookSingleMessagingQueueProducerService
    {
        #region Methods

        /// <summary>
        /// Sends a message asynchronously to the RabbitMQ queue.
        /// The message is serialized to JSON and published to the single messaging webhook queue.
        /// </summary>
        /// <param name="message">
        /// The message object to be sent. Typically a MessageMetadataRequest containing:
        /// - MessageStatus: Status updates from Twilio (delivered, failed, etc.)
        /// - Body: Content for incoming messages
        /// - Other webhook metadata from Twilio
        /// </param>
        /// <returns>A Task representing the asynchronous publish operation</returns>
        public Task SendingMessageAsync(object message);

        #endregion
    }
}
