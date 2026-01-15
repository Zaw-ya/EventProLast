using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventPro.Business.MemoryCacheStore.Implementaiion;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.RabbitMQMessaging.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Implementation;
using EventPro.Business.WhatsAppMessagesWebhook.Implementation;
using EventPro.Business.WhatsAppMessagesWebhook.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;


namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    #region Class Summary
    /// <summary>
    /// Consumer service for processing bulk webhook messages from RabbitMQ with flow control.
    /// This service listens to the bulk messaging queue and processes Twilio webhook callbacks
    /// for bulk/campaign SMS/WhatsApp messages with pause/resume capabilities.
    ///
    /// Architecture Overview:
    /// - Implements the Consumer pattern with advanced flow control capabilities
    /// - Uses a counter stored in Redis/Memory Cache to manage concurrent bulk operations
    /// - Supports pause/resume to control message processing rate
    /// - Runs continuously as a background service via BulkMessagingConsumerBackgroundService
    ///
    /// Queue Configuration:
    /// - Queue Name: TwilioBulkMessagingWebHookMessages (from appsettings)
    /// - Prefetch Count: 5 (lower than single messaging for better control)
    /// - Auto-Ack: false (manual acknowledgment after processing)
    ///
    /// Flow Control Mechanism:
    /// - Counter tracks active bulk operations in distributed cache
    /// - When counter > 0: Consumer pauses, messages are requeued (NACKed with requeue=true)
    /// - When counter = 0: Consumer processes messages normally
    /// - Validates against NumberOfOpertorToSendBulkOnSameTime from AppSettings
    ///
    /// Message Processing:
    /// - Status Messages: Updates message delivery status in database
    /// - Incoming Messages: Processes messages received from customers
    ///
    /// Dependencies:
    /// - TwilioWebhookService: Handles the actual message processing logic
    /// - WhatsappSendingProvidersService: Manages WhatsApp provider integrations
    /// - DistributedLockHelper: Ensures distributed synchronization
    /// - MemoryCacheStoreService: Stores the pause counter for flow control
    /// </summary>
    #endregion
    public class WebHookBulkMessagingQueueConsumerService : IWebHookBulkMessagingQueueConsumerService
    {
        #region Private Fields

        /// <summary>
        /// Factory for creating RabbitMQ connections.
        /// </summary>
        private readonly IConnectionFactory _connectionFactory;

        /// <summary>
        /// Service for processing Twilio webhook messages.
        /// Handles both status updates and incoming messages.
        /// </summary>
        private readonly IWebHookService _twilioWebHookService;

        /// <summary>
        /// Counter for tracking bulk sending operations (legacy static field - replaced by cache).
        /// </summary>
        private static int BulkSendingCounter;

        /// <summary>
        /// Database context for accessing AppSettings and other data.
        /// </summary>
        private readonly EventProContext db;

        /// <summary>
        /// Application configuration for accessing settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Memory cache service (Redis-backed) for storing the pause counter.
        /// This enables distributed flow control across multiple application instances.
        /// </summary>
        private readonly IMemoryCacheStoreService _memoryCacheStoreService;

        /// <summary>
        /// Factory for creating DI scopes within message handlers.
        /// </summary>
        private readonly IServiceScopeFactory _ServiceScopeFactory;

        /// <summary>
        /// The name of the queue to consume messages from.
        /// Also used as the cache key for the pause counter.
        /// </summary>
        private readonly String QueueName;

        /// <summary>
        /// Service for protecting/encrypting URLs in messages.
        /// </summary>
        private readonly UrlProtector _urlProtector;

        /// <summary>
        /// Helper for distributed locking across multiple instances.
        /// </summary>
        private readonly DistributedLockHelper _distributedLockHelper;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the bulk consumer service with all required dependencies.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating RabbitMQ connections</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="memoryCacheStoreService">Distributed cache service for flow control</param>
        /// <param name="serviceScopeFactory">Factory for creating DI scopes</param>
        /// <param name="urlProtector">URL protection service</param>
        /// <param name="distributedLockHelper">Distributed locking helper</param>
        /// <param name="dbFactory">Database context factory</param>
        /// <remarks>
        /// Creates an instance of TwilioWebhookService for processing messages.
        /// The queue name is also used as the cache key for the pause counter.
        /// </remarks>
        public WebHookBulkMessagingQueueConsumerService(IConnectionFactory connectionFactory,
            IConfiguration configuration, IMemoryCacheStoreService memoryCacheStoreService,
            IServiceScopeFactory serviceScopeFactory, UrlProtector urlProtector,
            DistributedLockHelper distributedLockHelper,
            IDbContextFactory<EventProContext> dbFactory)
        {
            _urlProtector = urlProtector;
            _connectionFactory = connectionFactory;
            _configuration = configuration;
            _memoryCacheStoreService = memoryCacheStoreService;
            _distributedLockHelper = distributedLockHelper;

            // Create the webhook service with required dependencies
            _twilioWebHookService = new TwilioWebhookService(configuration,
                new WhatsappSendingProvidersService(_configuration, _memoryCacheStoreService, _urlProtector), _distributedLockHelper, dbFactory);

            _ServiceScopeFactory = serviceScopeFactory;
            db = new EventProContext(_configuration);
            QueueName = _configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower();

            // Note: Counter initialization is commented out in original code
            // _memoryCacheStoreService.save(QueueName, 0);
        }

        #endregion

        #region Flow Control Methods

        /// <summary>
        /// Pauses message consumption by incrementing the pause counter in cache.
        /// Call this method before starting a new bulk sending operation.
        /// </summary>
        /// <remarks>
        /// How it works:
        /// 1. Retrieves current counter value from distributed cache
        /// 2. Increments the counter by 1
        /// 3. Saves the new value back to cache
        ///
        /// Effect:
        /// - When counter > 0, ConsumeMessage will requeue messages instead of processing
        /// - Multiple bulk operations can increment the counter independently
        /// - Each bulk operation should call Resume() when complete
        /// </remarks>
        public void Pause()
        {
            // Retrieve current counter from distributed cache
            var counter = _memoryCacheStoreService.Retrieve(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower());

            // Increment counter to pause consumption
            counter++;

            // Save updated counter back to cache
            _memoryCacheStoreService.save(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower(), counter);

            // Legacy: BulkSendingCounter++;
        }

        /// <summary>
        /// Resumes message consumption by decrementing the pause counter in cache.
        /// Call this method after a bulk sending operation completes.
        /// </summary>
        /// <remarks>
        /// How it works:
        /// 1. Retrieves current counter value from distributed cache
        /// 2. Decrements the counter by 1
        /// 3. Saves the new value back to cache
        ///
        /// Effect:
        /// - When counter reaches 0, ConsumeMessage will process messages normally
        /// - Each Pause() call should have a corresponding Resume() call
        /// </remarks>
        public void Resume()
        {
            // Retrieve current counter from distributed cache
            var counter = _memoryCacheStoreService.Retrieve(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower());

            // Decrement counter to allow consumption
            counter--;

            // Save updated counter back to cache
            _memoryCacheStoreService.save(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower(), counter);

            // Legacy: BulkSendingCounter--;
        }

        #endregion

        #region Message Consumption Methods

        /// <summary>
        /// Starts consuming messages from the bulk messaging webhook queue with flow control.
        /// This method sets up the RabbitMQ consumer and begins processing messages.
        /// </summary>
        /// <remarks>
        /// Queue Setup:
        /// 1. Creates a new connection and channel to RabbitMQ
        /// 2. Declares the queue (idempotent - creates if not exists)
        /// 3. Sets QoS with prefetch count of 5 for controlled parallel processing
        /// 4. Creates an EventingBasicConsumer for async message handling
        ///
        /// Message Processing Flow:
        /// 1. Receive message from queue
        /// 2. Deserialize JSON to MessageMetadataRequest
        /// 3. Create a new DI scope for the handler
        /// 4. Check if consumption is valid (counter = 0):
        ///    - If NOT valid: Delay 1 second, NACK with requeue, return
        ///    - If valid: Continue to process
        /// 5. Determine message type:
        ///    - If has MessageStatus and no Body: Process as status update
        ///    - If has Body: Process as incoming customer message
        /// 6. Acknowledge the message (BasicAck)
        ///
        /// Error Handling:
        /// - Exceptions are logged via Serilog
        /// - Messages are still acknowledged to prevent infinite loops
        /// </remarks>
        public void ConsumeMessage()
        {
            #region RabbitMQ Connection Setup

            // Create a new connection and channel for this consumer
            var connection = _connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            // Declare the queue - ensures it exists before consuming
            channel.QueueDeclare(queue: QueueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            // Set Quality of Service (QoS) settings
            // prefetchCount: 5 - Lower than single messaging (7) for better flow control
            // This limits concurrent processing during bulk operations
            channel.BasicQos(prefetchSize: 0, prefetchCount: 5, global: false);

            // Create an event-based consumer for async message handling
            var consumer = new EventingBasicConsumer(channel);

            #endregion

            #region Message Handler Setup

            // Set up the message received event handler
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    #region Message Deserialization

                    // Extract the message body from the delivery event
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);

                    // Deserialize the JSON message to MessageMetadataRequest object
                    MessageMetadataRequest messageRequest = JsonConvert
                   .DeserializeObject<MessageMetadataRequest>(message);

                    #endregion

                    #region Message Processing with Flow Control

                    // Create a new DI scope for this message handler
                    using (var scope = _ServiceScopeFactory.CreateScope())
                    {
                        // Get required services from the scoped container
                        var twilioWebHookService = scope.ServiceProvider.GetRequiredService<ITwilioWebhookService>();
                        var consumer = scope.ServiceProvider.GetRequiredService<IWebHookBulkMessagingQueueConsumerService>();

                        #region Flow Control Check

                        // Check if message consumption is currently valid (not paused)
                        if (!consumer.IsValidConsumingBulkMessages())
                        {
                            // Consumer is paused - bulk operation in progress
                            // Delay briefly to prevent tight loop
                            await Task.Delay(1000);

                            // NACK the message with requeue=true
                            // This puts the message back in the queue for later processing
                            channel.BasicNack(ea.DeliveryTag, false, true);
                            return;
                        }

                        #endregion

                        #region Process Message

                        // Determine message type and process accordingly
                        if (!string.IsNullOrEmpty(messageRequest!.MessageStatus) &&
                             string.IsNullOrEmpty(messageRequest.Body))
                        {
                            // Status Update Message:
                            // Has MessageStatus (delivered, read, failed, etc.) but no Body
                            // This is a delivery status callback from Twilio
                            await twilioWebHookService.ProcessStatusAsync(messageRequest);
                        }
                        else
                        {
                            // Incoming Message:
                            // Has a Body - this is a message from a customer
                            // Process it as an incoming WhatsApp/SMS message
                            await twilioWebHookService.ProcessIncomingMessageAsync(messageRequest);
                        }

                        #endregion
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    // Log any errors that occur during processing
                    Log.Error($"this is rabbit mq ex {ex}");
                }

                #region Message Acknowledgment

                // Acknowledge the message as processed
                // This removes it from the queue permanently
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                #endregion

            };

            #endregion

            #region Start Consuming

            // Start consuming messages from the queue
            // autoAck: false - we manually acknowledge after processing
            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            #endregion

            return;
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates if a new bulk sending operation can be started.
        /// Checks the current counter against the configured maximum concurrent operations.
        /// </summary>
        /// <returns>
        /// True if counter is less than NumberOfOpertorToSendBulkOnSameTime,
        /// False if the limit has been reached
        /// </returns>
        /// <remarks>
        /// This method queries the database for the configured limit (NumberOfOpertorToSendBulkOnSameTime)
        /// and compares it with the current pause counter from cache.
        ///
        /// Use this method before calling Pause() to check if a new bulk operation is allowed.
        /// </remarks>
        public bool IsValidSendingBulkMessages()
        {
            // Get the configured maximum concurrent bulk operations from database
            var NumberOfOpertorToSendBulkOnSameTime = db.AppSettings
                .AsNoTracking()
                .Select(e => e.NumberOfOpertorToSendBulkOnSameTime)
                .FirstOrDefault();

            // Get current counter from distributed cache
            var counter = _memoryCacheStoreService.Retrieve(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower());

            // Check if we've reached the limit
            if (counter >= NumberOfOpertorToSendBulkOnSameTime)
                return false;

            return true;
        }

        /// <summary>
        /// Validates if the consumer is allowed to process messages.
        /// Used internally by ConsumeMessage to determine if messages should be processed or requeued.
        /// </summary>
        /// <returns>
        /// True if counter equals 0 (no active bulk operations blocking consumption),
        /// False if counter is greater than 0 (consumption is paused)
        /// </returns>
        /// <remarks>
        /// When this returns false, messages are NACKed with requeue=true,
        /// allowing them to be processed once bulk operations complete.
        /// </remarks>
        public bool IsValidConsumingBulkMessages()
        {
            // Get current counter from distributed cache
            var counter = _memoryCacheStoreService.Retrieve(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower());

            // If counter > 0, consumption should be paused
            if (counter > 0)
                return false;

            return true;
        }

        #endregion

        #region Administrative Control Methods

        /// <summary>
        /// Forces the consumer to start processing by resetting the pause counter to 0.
        /// This is an administrative function to clear any stuck state.
        /// </summary>
        /// <remarks>
        /// WARNING: Use with caution!
        /// - This bypasses normal flow control
        /// - May cause issues if bulk operations are still in progress
        /// - Use when the system is stuck and needs manual intervention
        ///
        /// Effect: Sets counter to 0, allowing immediate message consumption
        /// </remarks>
        public void ForceStart()
        {
            // Reset the counter to 0, allowing consumption
            _memoryCacheStoreService.save(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower(),0);

            // Legacy: BulkSendingCounter = 0;
        }

        /// <summary>
        /// Forces the consumer to stop processing by setting the pause counter to a very high value.
        /// This is an administrative function to immediately halt all bulk processing.
        /// </summary>
        /// <remarks>
        /// WARNING: Use with caution!
        /// - This will prevent ALL bulk message consumption
        /// - Messages will be continuously requeued until ForceStart is called
        /// - Use for emergency situations or maintenance windows
        ///
        /// Effect: Sets counter to 1,000,000,000, effectively blocking all consumption
        /// </remarks>
        public void ForceStop()
        {
            // Set counter to extremely high value to block all consumption
            _memoryCacheStoreService.save(_configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower(), 1000000000);

            // Legacy: BulkSendingCounter = 1000000000;
        }

        #endregion
    }
}
