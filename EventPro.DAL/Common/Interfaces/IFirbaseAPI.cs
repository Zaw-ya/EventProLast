using EventPro.DAL.Models;
using System.Threading.Tasks;

namespace EventPro.DAL.Common.Interfaces
{
    /// <summary>
    /// Interface for Firebase Cloud Messaging (FCM) operations.
    /// Implemented by <see cref="FirbaseAPI"/> and registered in DI as Scoped.
    /// </summary>
    public interface IFirbaseAPI
    {
        /// <summary>
        /// Sends a push notification to a single device token or a topic.
        /// </summary>
        Task<bool> NotifyTopicOrTokenAsync(MessageRequest request);

        /// <summary>
        /// Sends a push notification to multiple device tokens (multicast).
        /// </summary>
        Task<bool> NotifyTokensAsync(MessageRequestTokens request);
    }
}
