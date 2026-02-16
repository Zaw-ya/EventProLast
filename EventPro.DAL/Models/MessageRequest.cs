namespace EventPro.DAL.Models
{
    /// <summary>
    /// FCM notification request for single-token or topic-based messaging.
    /// If Tokens is null/empty, the message is sent to the Topic (city-based).
    /// Used by <see cref="Common.FirbaseAPI.NotifyTopicOrTokenAsync"/>.
    /// </summary>
    public class MessageRequest
    {
        /// <summary>Notification title displayed on the device.</summary>
        public string Title { get; set; }

        /// <summary>Notification body text.</summary>
        public string Body { get; set; }

        /// <summary>Single device FCM token. If set, message is sent to this device only.</summary>
        public string Tokens { get; set; }

        /// <summary>Topic name (e.g., city ID). Used when Tokens is empty.</summary>
        public string Topic { get; set; }
    }
}
