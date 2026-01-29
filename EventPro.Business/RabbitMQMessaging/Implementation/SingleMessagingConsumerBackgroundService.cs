using Microsoft.Extensions.Hosting;
using EventPro.Business.RabbitMQMessaging.Interface;
using Microsoft.Extensions.Logging;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    #region Class Summary
    /// <summary>
    /// Background service that hosts the single messaging queue consumer.
    /// This service runs continuously as a hosted service in the ASP.NET Core application,
    /// ensuring that webhook messages are processed even when no HTTP requests are active.
    ///
    /// Architecture Overview:
    /// - Inherits from BackgroundService (Microsoft.Extensions.Hosting)
    /// - Registered as a hosted service in Startup.cs using AddHostedService
    /// - Starts automatically when the application starts
    /// - Runs for the lifetime of the application
    ///
    /// Purpose:
    /// - Provides a long-running process to consume RabbitMQ messages
    /// - Ensures single messaging webhooks are processed asynchronously
    /// - Decouples message consumption from the web request lifecycle
    ///
    /// Message Flow:
    /// 1. Application starts
    /// 2. This background service is created and started automatically
    /// 3. ExecuteAsync is called, which invokes ConsumeMessage()
    /// 4. ConsumeMessage() sets up the RabbitMQ consumer and runs indefinitely
    /// 5. Messages are processed as they arrive in the queue
    ///
    /// Registration (in Startup.cs):
    /// services.AddHostedService&lt;SingleMessagingConsumerBackgroundService&gt;();
    /// </summary>
    #endregion
    public class SingleMessagingConsumerBackgroundService : BackgroundService
    {
        #region Private Fields

        /// <summary>
        /// The consumer service that handles actual message consumption from RabbitMQ.
        /// Injected via dependency injection.
        /// </summary>
        private readonly IWebHookSingleMessagingQueueConsumerService _WebHookQueueConsumerService;
        private readonly ILogger<SingleMessagingConsumerBackgroundService> _logger;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the background service with the consumer service dependency.
        /// </summary>
        /// <param name="webHookQueueConsumerService">
        /// The single messaging queue consumer service that will process messages.
        /// This should be registered as a Singleton in the DI container.
        /// </param>
        public SingleMessagingConsumerBackgroundService(IWebHookSingleMessagingQueueConsumerService webHookQueueConsumerService)
        {
            _WebHookQueueConsumerService = webHookQueueConsumerService;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Executes the background task - starts consuming messages from the queue.
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
        /// Note: Consider implementing stoppingToken handling for graceful shutdown.
        /// </remarks>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "BulkMessagingConsumerBackgroundService started. Initializing RabbitMQ bulk consumer with flow control.");

            try
            {
                // Start consuming messages from the bulk messaging queue
                // Consumer runs indefinitely and handles pause/resume internally
                _WebHookQueueConsumerService.ConsumeMessage();

                _logger.LogInformation(
                    "RabbitMQ Bulk Messaging consumer successfully started and listening for messages.");

                // Keep the background service alive until application shutdown
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "BulkMessagingConsumerBackgroundService is stopping due to application shutdown.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Fatal error while starting Bulk Messaging RabbitMQ consumer.");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "BulkMessagingConsumerBackgroundService is stopping. Application shutdown requested.");

            await base.StopAsync(cancellationToken);
        }

        #endregion
    }
}
