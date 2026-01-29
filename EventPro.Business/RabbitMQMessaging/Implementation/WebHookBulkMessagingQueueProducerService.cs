using Microsoft.Extensions.Configuration;
using EventPro.Business.RabbitMQMessaging.Interface;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    #region Class Summary
    /// <summary>
    /// Producer service for publishing bulk webhook messages to RabbitMQ.
    /// This service is responsible for sending bulk/campaign Twilio webhook messages
    /// to a message queue for controlled asynchronous processing.
    ///
    /// Architecture Overview:
    /// - Implements the Producer pattern in the Producer-Consumer messaging architecture
    /// - Works in conjunction with WebHookBulkMessagingQueueConsumerService for flow control
    /// - Creates a persistent connection and channel to RabbitMQ on initialization
    /// - Uses thread-safe publishing with lock mechanism for concurrent access
    ///
    /// Queue Configuration:
    /// - Queue Name: TwilioBulkMessagingWebHookMessages (from appsettings)
    /// - Durable: false (queue won't survive broker restart)
    /// - Exclusive: false (queue can be accessed by multiple connections)
    /// - AutoDelete: false (queue won't be deleted when consumers disconnect)
    ///
    /// Usage:
    /// - Registered as Singleton in DI container
    /// - Called by webhook controllers when receiving Twilio callbacks for bulk campaigns
    /// - Messages are serialized to JSON before publishing
    ///
    /// Note: This producer is used for high-volume messaging scenarios where
    /// rate limiting and flow control are required on the consumer side.
    /// </summary>
    #endregion
    public class WebHookBulkMessagingQueueProducerService : IWebHookBulkMessagingQueueProducerService
    {
        #region Private Fields

        /// <summary>
        /// Factory for creating RabbitMQ connections. Injected via DI.
        /// </summary>
        private readonly IConnectionFactory _connectionFactory;

        /// <summary>
        /// Persistent connection to RabbitMQ server.
        /// Created once during initialization and reused for all publish operations.
        /// </summary>
        private readonly IConnection connection;

        /// <summary>
        /// RabbitMQ channel (virtual connection) used for publishing messages.
        /// Shared across all publish calls with thread-safe locking.
        /// </summary>
        private readonly IModel channel;

        /// <summary>
        /// Application configuration for accessing RabbitMQ queue settings.
        /// </summary>
        private readonly IConfiguration _Configuration;

        /// <summary>
        /// The name of the queue to publish messages to.
        /// Retrieved from configuration: RabbitMqQueues:TwilioBulkMessagingWebHookMessages
        /// </summary>
        private readonly String QueueName;

        private readonly ILogger<WebHookBulkMessagingQueueProducerService>? _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the bulk producer service with RabbitMQ connection and queue declaration.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating RabbitMQ connections</param>
        /// <param name="configuration">Application configuration containing queue settings</param>
        /// <remarks>
        /// On initialization:
        /// 1. Creates a new connection to RabbitMQ server
        /// 2. Creates a channel for message operations
        /// 3. Retrieves queue name from configuration
        /// 4. Declares the queue (creates if not exists)
        ///
        /// Note: The channel is created twice in the original code (line 21 and 24),
        /// the second creation is used for actual operations.
        /// </remarks>
        public WebHookBulkMessagingQueueProducerService(IConnectionFactory connectionFactory, IConfiguration configuration, ILogger<WebHookBulkMessagingQueueProducerService>? logger)
        {
            _connectionFactory = connectionFactory;
            connection = _connectionFactory.CreateConnection();
            channel = connection.CreateModel();
            _Configuration = configuration;
            QueueName = _Configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower();
            channel = connection.CreateModel();

            // Declare the queue - creates it if it doesn't exist
            // This ensures the queue is available before any publish attempts
            channel.QueueDeclare(queue: QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _logger = logger;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Publishes a message asynchronously to the bulk messaging webhook queue.
        /// </summary>
        /// <param name="message">
        /// The message object to publish. Will be serialized to JSON.
        /// Expected types: MessageMetadataRequest or similar webhook payload objects
        /// </param>
        /// <returns>A completed Task when the message has been published</returns>
        /// <remarks>
        /// Thread Safety:
        /// - Uses lock on channel to ensure thread-safe publishing
        /// - Multiple concurrent calls will be serialized through the lock
        ///
        /// Message Flow:
        /// 1. Serialize message object to JSON string
        /// 2. Convert JSON to UTF-8 byte array
        /// 3. Acquire lock on channel
        /// 4. Publish to queue using BasicPublish
        /// 5. Release lock
        ///
        /// Consumer Processing:
        /// - Messages in this queue are subject to flow control
        /// - Consumer may pause/resume based on bulk operation limits
        /// - Messages may be requeued if consumer is paused
        /// </remarks>
        public async Task SendingMessageAsync(object message)
        {
            // Serialize the message object to JSON format
            var jsonString = JsonSerializer.Serialize(message);

            // Convert the JSON string to a byte array for RabbitMQ
            var body = Encoding.UTF8.GetBytes(jsonString);

            // Publish asynchronously with thread-safe channel access
            try
            {
                await Task.Run(() =>
                {
                    // Lock ensures only one thread publishes at a time
                    // This prevents channel corruption from concurrent access
                    lock (channel)
                    {
                        _logger?.LogDebug("Publishing to queue {QueueName} - size {Bytes} bytes", QueueName, body.Length);

                        // Publish to the default exchange (empty string)
                        // Routing key matches queue name for direct delivery
                        channel.BasicPublish(exchange: string.Empty,
                                routingKey: QueueName,
                                body: body,
                                basicProperties: null);
                        _logger?.LogInformation("Successfully published to {QueueName}", QueueName);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to publish message to {QueueName}", QueueName);
                throw;
            }

            return;

        }

        #endregion
    }
}
