using EventPro.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventPro.Business.RabbitMQMessaging.Interface
{
    public interface IMessageSendingBulkQueueProducerService
    {
        Task PublishBatchAsync(List<MessageSendingRequest> requests);
    }
}
