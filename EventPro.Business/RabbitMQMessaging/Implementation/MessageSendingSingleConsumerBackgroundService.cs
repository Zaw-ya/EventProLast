using Microsoft.Extensions.Hosting;
using EventPro.Business.RabbitMQMessaging.Interface;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    public class MessageSendingSingleConsumerBackgroundService : BackgroundService
    {
        private readonly IMessageSendingSingleQueueConsumerService _consumerService;

        public MessageSendingSingleConsumerBackgroundService(IMessageSendingSingleQueueConsumerService consumerService)
        {
            _consumerService = consumerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("MessageSendingSingleConsumerBackgroundService is starting.");

            _consumerService.ConsumeMessage();

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("MessageSendingSingleConsumerBackgroundService is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
