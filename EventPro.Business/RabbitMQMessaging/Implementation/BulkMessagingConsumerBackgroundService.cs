using Microsoft.Extensions.Hosting;
using EventPro.Business.RabbitMQMessaging.Interface;
using Microsoft.Extensions.Logging;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    #region Class Summary
    /// <summary>
    /// Background service that hosts the bulk messaging queue consumer.
    /// This service runs continuously as a hosted service in the ASP.NET Core application,
    /// ensuring that bulk webhook messages are processed with flow control capabilities.
    ///
    /// Architecture Overview:
    /// - Inherits from BackgroundService (Microsoft.Extensions.Hosting)
    /// - Registered as a hosted service in Startup.cs using AddHostedService
    /// - Starts automatically when the application starts
    /// - Runs for the lifetime of the application
    ///
    /// Purpose:
    /// - Provides a long-running process to consume bulk RabbitMQ messages
    /// - Ensures bulk messaging webhooks are processed with rate limiting
    /// - Supports pause/resume functionality via the consumer service
    /// - Decouples message consumption from the web request lifecycle
    ///
    /// Message Flow:
    /// 1. Application starts
    /// 2. This background service is created and started automatically
    /// 3. ExecuteAsync is called, which invokes ConsumeMessage()
    /// 4. ConsumeMessage() sets up the RabbitMQ consumer with flow control
    /// 5. Messages are processed as they arrive, respecting pause/resume state
    ///
    /// Flow Control:
    /// - The consumer service (IWebHookBulkMessagingQueueConsumerService) manages flow control
    /// - When paused, messages are NACKed and requeued
    /// - When resumed, messages are processed normally
    /// - Administrators can use ForceStart/ForceStop for emergency control
    ///
    /// Registration (in Startup.cs):
    /// services.AddHostedService&lt;BulkMessagingConsumerBackgroundService&gt;();
    /// </summary>
    #endregion
    public class BulkMessagingConsumerBackgroundService : BackgroundService
    {
        #region Private Fields

        /// <summary>
        /// The consumer service that handles actual message consumption from RabbitMQ.
        /// This service includes flow control capabilities (pause/resume/force start/stop).
        /// Injected via dependency injection.
        /// </summary>
        private readonly IWebHookBulkMessagingQueueConsumerService _WebHookQueueConsumerServices;
        private readonly ILogger<BulkMessagingConsumerBackgroundService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the background service with the bulk consumer service dependency.
        /// </summary>
        /// <param name="webHookQueueConsumerServices">
        /// The bulk messaging queue consumer service that will process messages.
        /// This should be registered as a Singleton in the DI container to ensure
        /// flow control state is shared across the application.
        /// </param>
        public BulkMessagingConsumerBackgroundService(IWebHookBulkMessagingQueueConsumerService webHookQueueConsumerServices)
        {
            _WebHookQueueConsumerServices = webHookQueueConsumerServices;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Executes the background task - starts consuming bulk messages from the queue.
        /// This method is called automatically when the application starts.
        /// </summary>
        /// <param name="stoppingToken">
        /// Cancellation token that signals when the application is shutting down.
        /// Note: The current implementation doesn't use this token as ConsumeMessage()
        /// runs indefinitely via RabbitMQ's event-based consumer.
        /// </param>
        /// <returns>A Task representing the background operation</returns>
        /// <remarks>
        /// The ConsumeMessage() method sets up an event-based consumer that runs
        /// continuously, listening for incoming messages from the RabbitMQ queue.
        /// The method returns after setting up the consumer, but the consumer
        /// continues to run on a separate thread managed by RabbitMQ client.
        ///
        /// Flow Control Behavior:
        /// - When bulk operations are in progress (counter > 0), messages are requeued
        /// - When no bulk operations are active (counter = 0), messages are processed
        /// - This allows webhooks to be held during bulk sending to prevent overload
        ///
        /// Note: Consider implementing stoppingToken handling for graceful shutdown.
        /// Gharabawy : Implmented this handling to log shutdown event (1/29)
        /// </remarks>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
               "SingleMessagingConsumerBackgroundService started. Initializing RabbitMQ consumer.");

            // Start consuming messages from the bulk messaging queue
            // This sets up the RabbitMQ consumer with flow control which runs indefinitely
            try
            {
                // Start consuming messages from the single messaging queue
                _WebHookQueueConsumerServices.ConsumeMessage();

                _logger.LogInformation(
                    "RabbitMQ Single Messaging consumer successfully started and listening for messages.");

                // Keep the background service alive until application shutdown
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "SingleMessagingConsumerBackgroundService is stopping due to application shutdown.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Fatal error while starting Single Messaging RabbitMQ consumer.");
                throw;
            }
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "SingleMessagingConsumerBackgroundService is stopping. Application shutdown requested.");

            await base.StopAsync(cancellationToken);
        }

        #endregion
    }
}
