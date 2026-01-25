using Microsoft.Extensions.Hosting;
using EventPro.Business.RabbitMQMessaging.Interface;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    public class MessageSendingBulkConsumerBackgroundService : BackgroundService
    {
        private readonly IMessageSendingBulkQueueConsumerService _consumerService;

        public MessageSendingBulkConsumerBackgroundService(IMessageSendingBulkQueueConsumerService consumerService)
        {
            _consumerService = consumerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("MessageSendingBulkConsumerBackgroundService is starting.");

            _consumerService.ConsumeMessage();

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("MessageSendingBulkConsumerBackgroundService is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
