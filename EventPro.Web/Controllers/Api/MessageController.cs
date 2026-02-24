using Microsoft.AspNetCore.Mvc;
using EventPro.DAL.Common;
using EventPro.DAL.Models;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers.Api
{
    /// <summary>
    /// REST API controller for sending Firebase Cloud Messaging (FCM) push notifications.
    /// Provides endpoints for external systems to trigger notifications via topic, single token, or multicast.
    ///
    /// Endpoints:
    ///   GET  /api/Message/SendToTopicOrToken  - Send to a single token or topic
    ///   POST /api/Message/NotifyTokens        - Multicast to multiple device tokens
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        #region Firebase FCM Endpoints

        /// <summary>
        /// Sends a push notification to a single device token or a topic (e.g., city-based).
        /// </summary>
        /// <param name="request">Notification payload with title, body, and target (token or topic).</param>
        /// <returns>True if sent successfully.</returns>
        [HttpGet("SendToTopicOrToken")]
        public async Task<bool> SendToTopicOrTokenAsync([FromBody] MessageRequest request)
        {
            FirbaseAPI _FirbaseAPI = new FirbaseAPI();
            return await _FirbaseAPI.NotifyTopicOrTokenAsync(request);
        }

        /// <summary>
        /// Sends a push notification to multiple device tokens simultaneously (multicast).
        /// </summary>
        /// <param name="request">Notification payload with title, body, and list of device tokens.</param>
        /// <returns>True if all messages were delivered successfully.</returns>
        [HttpPost("NotifyTokens")]
        public async Task<bool> NotifyTokensAsync([FromBody] MessageRequestTokens request)
        {
            FirbaseAPI _FirbaseAPI = new FirbaseAPI();
            return await _FirbaseAPI.NotifyTokensAsync(request);
        }

        #endregion
    }
}
