namespace EventPro.Business.RabbitMQMessaging.Interface
{
    #region Interface Summary
    /// <summary>
    /// Interface for Bulk Message Queue Consumer Service.
    /// This interface defines the contract for consuming bulk webhook messages
    /// from a RabbitMQ queue with advanced flow control capabilities.
    ///
    /// Purpose:
    /// - Consumes Twilio webhook messages from the bulk messaging queue
    /// - Provides pause/resume functionality to control message processing rate
    /// - Supports concurrent bulk operation limits based on system configuration
    /// - Enables force start/stop for administrative control
    ///
    /// Queue Name: TwilioBulkMessagingWebHookMessages (configured in appsettings)
    /// Prefetch Count: 5 messages (lower than single messaging for better control)
    ///
    /// Flow Control:
    /// - Uses a counter stored in Redis/Memory Cache to track active bulk operations
    /// - Messages are requeued (NACKed) when the system is paused
    /// - Validates against NumberOfOpertorToSendBulkOnSameTime setting
    /// </summary>
    #endregion
    public interface IWebHookBulkMessagingQueueConsumerService
    {
        #region Message Consumption Methods

        /// <summary>
        /// Starts consuming messages from the RabbitMQ bulk messaging queue.
        /// Sets up the consumer with event handlers and begins listening for messages.
        ///
        /// Processing Logic:
        /// - Checks if consumption is valid (not paused) before processing
        /// - If paused: Delays 1 second and requeues the message (BasicNack with requeue=true)
        /// - If valid: Processes status updates or incoming messages accordingly
        /// </summary>
        public void ConsumeMessage();

        #endregion

        #region Flow Control Methods

        /// <summary>
        /// Resumes message consumption by decrementing the pause counter.
        /// Call this after a bulk sending operation completes to allow more operations.
        ///
        /// Effect: Decrements the counter in cache, potentially allowing consumption to resume
        /// </summary>
        public void Resume();

        /// <summary>
        /// Pauses message consumption by incrementing the pause counter.
        /// Call this before starting a bulk sending operation to reserve a slot.
        ///
        /// Effect: Increments the counter in cache, potentially blocking consumption
        /// </summary>
        public void Pause();

        #endregion

        #region Validation Methods

        /// <summary>
        /// Checks if a new bulk sending operation can be started.
        /// Validates against the configured NumberOfOpertorToSendBulkOnSameTime limit.
        /// </summary>
        /// <returns>
        /// True if the current counter is less than the configured limit,
        /// False if the limit has been reached
        /// </returns>
        public bool IsValidSendingBulkMessages();

        /// <summary>
        /// Checks if the consumer is allowed to process messages.
        /// Used internally by ConsumeMessage to determine if messages should be processed or requeued.
        /// </summary>
        /// <returns>
        /// True if counter is 0 (no active bulk operations blocking consumption),
        /// False if counter is greater than 0 (consumption is paused)
        /// </returns>
        public bool IsValidConsumingBulkMessages();

        #endregion

        #region Administrative Control Methods

        /// <summary>
        /// Forces the consumer to start processing by resetting the counter to 0.
        /// Use this for administrative purposes to clear any stuck state.
        ///
        /// Warning: This overrides normal flow control and should be used carefully
        /// </summary>
        public void ForceStart();

        /// <summary>
        /// Forces the consumer to stop processing by setting the counter to a very high value (1 billion).
        /// Use this for administrative purposes to immediately halt all bulk processing.
        ///
        /// Warning: This will prevent all bulk message consumption until ForceStart is called
        /// </summary>
        public void ForceStop();

        #endregion
    }
}
