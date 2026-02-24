using EventPro.Business.MemoryCacheStore.Implementaiion;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.RabbitMQMessaging.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Implementation;
using EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio;
using EventPro.Business.WhatsAppMessagesWebhook.Implementation;
using EventPro.Business.WhatsAppMessagesWebhook.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Org.BouncyCastle.Ocsp;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Serilog;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    #region Class Summary
    /// <summary>
    /// Consumer service for processing single webhook messages from RabbitMQ.
    /// This service listens to the single messaging queue and processes Twilio webhook callbacks
    /// for individual SMS/WhatsApp message status updates and incoming messages.
    ///
    /// Architecture Overview:
    /// - Implements the Consumer pattern in the Producer-Consumer messaging architecture
    /// - Runs continuously as a background service via SingleMessagingConsumerBackgroundService
    /// - Uses event-driven consumption with EventingBasicConsumer
    ///
    /// Queue Configuration:
    /// - Queue Name: TwilioSingleMessagingWebHookMessages (from appsettings)
    /// - Prefetch Count: 7 (processes up to 7 messages concurrently)
    /// - Auto-Ack: false (manual acknowledgment after processing)
    ///
    /// Message Processing:
    /// - Status Messages: Updates message delivery status in database
    /// - Incoming Messages: Processes messages received from customers
    ///
    /// Dependencies:
    /// - TwilioWebhookService: Handles the actual message processing logic
    /// - WhatsappSendingProvidersService: Manages WhatsApp provider integrations
    /// - DistributedLockHelper: Ensures distributed synchronization for concurrent operations
    /// </summary>
    #endregion
    public class WebHookSingleMessagingQueueConsumerService : IWebHookSingleMessagingQueueConsumerService
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
        /// Counter for tracking bulk sending operations (legacy - not actively used in this service).
        /// </summary>
        private static int BulkSendingCounter;

        /// <summary>
        /// Database context for direct database operations.
        /// </summary>
        private readonly EventProContext db;

        /// <summary>
        /// Application configuration for accessing settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Memory cache service for distributed caching (Redis-backed).
        /// </summary>
        private readonly IMemoryCacheStoreService _memoryCacheStoreService;

        /// <summary>
        /// Factory for creating DI scopes within message handlers.
        /// Required because message handlers run outside the normal request scope.
        /// </summary>
        private readonly IServiceScopeFactory _ServiceScopeFactory;

        /// <summary>
        /// The name of the queue to consume messages from.
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

        private readonly ILogger<TwilioCardTemplates> _logger;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the consumer service with all required dependencies.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating RabbitMQ connections</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="memoryCacheStoreService">Distributed cache service</param>
        /// <param name="serviceScopeFactory">Factory for creating DI scopes</param>
        /// <param name="urlProtector">URL protection service</param>
        /// <param name="distributedLockHelper">Distributed locking helper</param>
        /// <param name="dbFactory">Database context factory</param>
        /// <remarks>
        /// Creates an instance of TwilioWebhookService for processing messages.
        /// This service is created directly rather than from DI to ensure proper lifecycle.
        /// </remarks>
        public WebHookSingleMessagingQueueConsumerService(IConnectionFactory connectionFactory,
            IConfiguration configuration, IMemoryCacheStoreService memoryCacheStoreService,
            IServiceScopeFactory serviceScopeFactory, UrlProtector urlProtector, DistributedLockHelper distributedLockHelper,
            IDbContextFactory<EventProContext> dbFactory, ILogger<TwilioCardTemplates> logger)
        {
            _logger = logger;
            _urlProtector = urlProtector;
            _connectionFactory = connectionFactory;
            _configuration = configuration;
            _memoryCacheStoreService = memoryCacheStoreService;
            _distributedLockHelper = distributedLockHelper;

            // Create the webhook service with required dependencies
            // This handles the actual processing of Twilio webhooks
            _twilioWebHookService = new TwilioWebhookService(configuration,
                new WhatsappSendingProvidersService(_configuration, _memoryCacheStoreService, _urlProtector, _logger), _distributedLockHelper, dbFactory , _logger);

            _ServiceScopeFactory = serviceScopeFactory;
            db = new EventProContext(_configuration);
            QueueName = _configuration.GetSection("RabbitMqQueues")["TwilioSingleMessagingWebHookMessages"].ToLower();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts consuming messages from the single messaging webhook queue.
        /// This method sets up the RabbitMQ consumer and begins processing messages.
        /// </summary>
        /// <remarks>
        /// Queue Setup:
        /// 1. Creates a new connection and channel to RabbitMQ
        /// 2. Declares the queue (idempotent - creates if not exists)
        /// 3. Sets QoS with prefetch count of 7 for parallel processing
        /// 4. Creates an EventingBasicConsumer for async message handling
        ///
        /// Message Processing Flow:
        /// 1. Receive message from queue
        /// 2. Deserialize JSON to MessageMetadataRequest
        /// 3. Create a new DI scope for the handler
        /// 4. Determine message type:
        ///    - If has MessageStatus and no Body: Process as status update
        ///    - If has Body: Process as incoming customer message
        /// 5. Acknowledge the message (BasicAck)
        ///
        /// Error Handling:
        /// - Exceptions are logged via Serilog
        /// - Messages are still acknowledged to prevent infinite reprocessing
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
            // prefetchCount: 7 - Consumer will receive up to 7 unacknowledged messages at a time
            // This allows for parallel processing while preventing memory overload
            channel.BasicQos(prefetchSize: 0, prefetchCount: 7, global: false);

            // Create an event-based consumer for async message handling
            var consumer = new EventingBasicConsumer(channel);

            #endregion

            #region Message Handler Setup

            // Set up the message received event handler
            consumer.Received += async (model, ea) =>
            {
                var deliveryTag = ea.DeliveryTag;
                var queue = QueueName;
                try
                {
                    #region Message Deserialization

                    // Extract the message body from the delivery event
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);

                    _logger.LogDebug("[{Queue}] Received message (deliveryTag={Tag}, size={Bytes})", queue, deliveryTag, body.Length);

                    // Deserialize the JSON message to MessageMetadataRequest object
                    // This contains Twilio webhook data: status, body, from, to, etc.
                    MessageMetadataRequest messageRequest = JsonConvert
                   .DeserializeObject<MessageMetadataRequest>(message);

                    if (messageRequest == null)
                    {
                        _logger.LogWarning("[{Queue}] Deserialization failed → deliveryTag={Tag}", queue, deliveryTag);
                        channel.BasicAck(deliveryTag, false);
                        return;
                    }

                    _logger.LogInformation("[{Queue}] Processing MessageSid={Sid} | Status={Status} | HasBody={HasBody}",
                    queue, messageRequest.MessageSid, messageRequest.MessageStatus, !string.IsNullOrEmpty(messageRequest.Body));

                    #endregion

                    #region Message Processing

                    // Create a new DI scope for this message handler
                    // This ensures proper disposal of scoped services
                    using (var scope = _ServiceScopeFactory.CreateScope())
                    {
                        // Get the webhook service from the scoped container
                        var twilioWebHookService = scope.ServiceProvider.GetRequiredService<ITwilioWebhookService>();

                        // Determine message type and process accordingly
                        if (!string.IsNullOrEmpty(messageRequest.MessageStatus) &&
                             string.IsNullOrEmpty(messageRequest.Body))
                        {
                            // Status Update Message:
                            // Has MessageStatus (delivered, read, failed, etc.) but no Body
                            // This is a delivery status callback from Twilio
                            _logger.LogInformation("[{Queue}] Processing DELIVERY STATUS → {Status} for MessageSid={Sid}",
                            queue, messageRequest.MessageStatus, messageRequest.MessageSid);
                            
                            await twilioWebHookService.ProcessStatusAsync(messageRequest);
                        }
                        else
                        {
                            // Incoming Message:
                            // Has a Body - this is a message from a customer
                            // Process it as an incoming WhatsApp/SMS message
                            _logger.LogInformation("[{Queue}] Processing INCOMING MESSAGE from {From} to {To} → Body length={Len}",
                            queue, messageRequest.From, messageRequest.To, messageRequest.Body?.Length ?? 0);
                            
                            await twilioWebHookService.ProcessIncomingMessageAsync(messageRequest);
                        }
                        _logger.LogInformation("[{Queue}] Successfully processed message → MessageSid={Sid}", queue, messageRequest.MessageSid);
                        channel.BasicAck(deliveryTag, false);
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    // Log any errors that occur during processing
                    // The message will still be acknowledged to prevent infinite loops
                    _logger.LogError(ex, "[{Queue}] Failed to process message → deliveryTag={Tag}", queue, deliveryTag);
                    channel.BasicNack(deliveryTag, false, true);
                }
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
    }
}
