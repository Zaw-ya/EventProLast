using EventPro.DAL.Models;
using System.Threading.Tasks;

namespace EventPro.Business.RabbitMQMessaging.Interface
{
    public interface IMessageSendingSingleQueueProducerService
    {
        Task PublishMessageAsync(MessageSendingRequest request);
    }
}
