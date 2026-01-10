using Microsoft.Extensions.Configuration;
using EventPro.Business.RabbitMQMessaging.Interface;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    public class WebHookBulkMessagingQueueProducerService : IWebHookBulkMessagingQueueProducerService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly IConfiguration _Configuration;
        private readonly String QueueName;

        public WebHookBulkMessagingQueueProducerService(IConnectionFactory connectionFactory, IConfiguration configuration)
        {
            _connectionFactory = connectionFactory;
            connection = _connectionFactory.CreateConnection();
            channel = connection.CreateModel();
            _Configuration = configuration;
            QueueName = _Configuration.GetSection("RabbitMqQueues")["TwilioBulkMessagingWebHookMessages"].ToLower();
            channel = connection.CreateModel();

            channel.QueueDeclare(queue: QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }
        public async Task SendingMessageAsync(object message)
        {
            var jsonString = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonString);

            await Task.Run(() =>
            {
                lock (channel)
                {
                    channel.BasicPublish(exchange: string.Empty,
                            routingKey: QueueName,
                            body: body,
                            basicProperties: null);
                }

            });

            return;

        }
    }
}
