using EventPro.Business.RabbitMQMessaging.Interface;
using EventPro.DAL.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EventPro.Business.RabbitMQMessaging.Implementation
{
    public class MessageSendingSingleQueueProducerService : IMessageSendingSingleQueueProducerService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string QueueName = "MessageSendingSingle";

        public MessageSendingSingleQueueProducerService(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            connection = _connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(queue: QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        public async Task PublishMessageAsync(MessageSendingRequest request)
        {
            var jsonString = JsonSerializer.Serialize(request);
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
        }
    }
}
