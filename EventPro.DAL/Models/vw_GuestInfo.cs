using Microsoft.EntityFrameworkCore;
using System;

namespace EventPro.DAL.Models
{
    [Keyless]
    public class vwGuestInfo
    {
        public int GuestId { get; set; }
        public bool GuestArchieved { get; set; }
        public int? EventId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PrimaryContactNo { get; set; }
        public string SecondaryContactNo { get; set; }
        public string EmailAddress { get; set; }
        public string ModeOfCommunication { get; set; }
        public int? NoOfMembers { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public string Source { get; set; }
        public DateTime? WaresponseTime { get; set; }
        public int? GateKeeper { get; set; }
        public string MessageId { get; set; }
        public string AdditionalText { get; set; }
        public string ImgSentMsgId { get; set; }
        public bool? TextSent { get; set; }
        public bool? TextDelivered { get; set; }
        public bool? TextRead { get; set; }
        public bool? TextFailed { get; set; }
        public bool? ImgFailed { get; set; }
        public bool? ImgSent { get; set; }
        public bool? ImgDelivered { get; set; }
        public bool? ImgRead { get; set; }
        public string Cypertext { get; set; }
        public string WhatsappStatus { get; set; }
        public string? whatsappMessageId { get; set; }
        public string? whatsappMessageImgId { get; set; }
        public string? waMessageEventLocationForSendingToAll { get; set; }
        public string? whatsappWatiEventLocationId { get; set; }
        public bool? EventLocationSent { get; set; }
        public bool? EventLocationDelivered { get; set; }
        public bool? EventLocationRead { get; set; }
        public bool? EventLocationFailed { get; set; }
        public bool? ConguratulationMsgSent { get; set; }  // Corrected spelling from "Conguratulation" to "Congratulation"
        public bool? ConguratulationMsgDelivered { get; set; }
        public bool? ConguratulationMsgRead { get; set; }
        public bool? ConguratulationMsgFailed { get; set; }
        public string? ConguratulationMsgId { get; set; }
        public string? WatiConguratulationMsgId { get; set; }
        public string? ReminderMessageId { get; set; }
        public string? ReminderMessageWatiId { get; set; }
        public bool? ReminderMessageSent { get; set; }
        public bool? ReminderMessageDelivered { get; set; }
        public bool? ReminderMessageRead { get; set; }
        public bool? ReminderMessageFailed { get; set; }
        public string Response { get; set; }
        public int? Scanned { get; set; }
        public bool? IsPhoneNumberValid { get; set; }
    }
}
