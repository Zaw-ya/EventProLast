using System;

namespace EventPro.DAL.Dto
{
    public class MessageLog
    {
        public string Body { get; set; }
        public string ToPhoneNumber { get; set; }
        public DateTime? DateSent { get; set; }
        public bool HasMedia { get; set; }
        public string MediaUrl { get; set; }
        public bool EventProMessage { get; set; }
        public string status { get; set; }
        public string MediaExtention { get; set; }

    }
}
