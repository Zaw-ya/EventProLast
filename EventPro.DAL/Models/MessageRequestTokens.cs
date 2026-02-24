using System.Collections.Generic;

namespace EventPro.DAL.Models
{
    /// <summary>
    /// FCM multicast notification request for sending to multiple device tokens.
    /// Used by <see cref="Common.FirbaseAPI.NotifyTokensAsync"/> for batch notifications.
    /// Typically used by the scheduled notification service to remind gatekeepers.
    /// </summary>
    public class MessageRequestTokens
    {
        /// <summary>The event ID this notification is about.</summary>
        public int EventID { get; set; }

        /// <summary>Notification title displayed on each device.</summary>
        public string Title { get; set; }

        /// <summary>Notification body text.</summary>
        public string Body { get; set; }

        /// <summary>List of FCM device tokens to send to (max 500 per FCM call).</summary>
        public List<string> Tokens { get; set; }
    }
}