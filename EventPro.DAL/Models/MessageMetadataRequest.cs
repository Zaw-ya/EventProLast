using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPro.DAL.Models
{
    public class MessageMetadataRequest
    {
        public string ChannelPrefix { get; set; }
        public string MessagingServiceSid { get; set; }
        public string ApiVersion { get; set; }
        public string MessageStatus { get; set; }
        public string SmsSid { get; set; }
        public string SmsStatus { get; set; }
        public string ChannelInstallSid { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string MessageSid { get; set; }
        public string AccountSid { get; set; }
        public string ChannelToAddress { get; set; }
        public string OriginalRepliedMessageSid { get; set; }
        public string ButtonPayload { get; set; }
        public string ButtonText { get; set; }
        public string OriginalRepliedMessageSender { get; set; }
        public string SmsMessageSid { get; set; }
        public string NumMedia { get; set; }
        public string ProfileName { get; set; }
        public string WaId { get; set; }
        public string MessageType { get; set; }
        public string Body { get; set; }
        public string NumSegments { get; set; }
        public string ReferralNumMedia { get; set; }
    }
}
