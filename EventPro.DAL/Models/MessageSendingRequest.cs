using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EventPro.DAL.Models
{
    public class MessageSendingRequest
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public MessageSendingType MessageType { get; set; }
        public int EventId { get; set; }

        public List<int> GuestIds { get; set; } = new();
        [JsonIgnore]
        public Events Event { get; set; }
        [JsonIgnore]
        public List<Guest> Guests { get; set; }

        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int RetryCount { get; set; } = 0;

        public int MaxRetries { get; set; } = 3;
    }
}
