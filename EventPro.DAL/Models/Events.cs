using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class Events
    {
        public Events()
        {
            CardInfo = new HashSet<CardInfo>();
            EventGatekeeperMapping = new HashSet<EventGatekeeperMapping>();
            Guest = new HashSet<Guest>();
            Invoices = new HashSet<Invoices>();
        }

        public string? EventLocation { get; set; }
        public string? CustomConfirmationTemplateWithVariables { get; set; }
        public string? CustomCardTemplateWithVariables { get; set; }
        public string? CustomReminderTemplateWithVariables { get; set; }
        public string? CustomCongratulationTemplateWithVariables { get; set; }
        public string? ConfirmationButtonsType { get; set; }
        public int? EventLocationId { get; set; }
        public DateTime? AttendanceTime { get; set; }
        public bool? ShowOnCalender { get; set; }
        public bool? WhatsappConfirmation { get; set; }
        public bool? WhatsappPush { get; set; }
        public bool? ShowFailedSendingEventLocationLink { get; set; }
        public bool? ShowFailedSendingCongratulationLink { get; set; }
        public string? FailedSendingConfiramtionMessagesLinksLanguage { get; set; }
        public string? SendingConfiramtionMessagesLinksLanguage { get; set; }

        public int Id { get; set; }
        public int? EventCode { get; set; }
        public string EventTitle { get; set; }
        public string SystemEventTitle { get; set; }

        public bool? IsDeleted { get; set; } = false;
        public DateTime? DeletedOn { get; set; }
        public int? DeletedBy { get; set; }

        public int? Type { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public string EventVenue { get; set; }
        public string? MessageHeaderImage { get; set; }
        public string? MessageHeaderText { get; set; }
        public string GmapCode { get; set; }
        public string Glocation { get; set; }
        public string Icon { get; set; } // Event Icon URL
        [Required]
        public string EventDescription { get; set; }
        public int? CreatedBy { get; set; }
        public int? CreatedFor { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool? IsArchived { get; set; }
        public string Status { get; set; }
        public string ParentTitle { get; set; }
        public string ParentTitleGender { get; set; } = "Female";
        public string MessageLanguage { get; set; } = "Arabic";
        public long? LinkedEvent { get; set; }
        public bool SendInvitation { get; set; } = true;
        public int? CityId { get; set; }
        public TimeSpan? LeaveTime { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string WhatsappProviderName { get; set; }
        public string? CustomInvitationMessageTemplateName { get; set; }
        public string? CustomCardInvitationTemplateName { get; set; }
        public string? CardInvitationTemplateType { get; set; }
        public string? ConguratulationsMsgSentOnNumber { get; set; }
        public string? ConguratulationsMsgType { get; set; }
        public string? ConguratulationsMsgTemplateName { get; set; }
        public string SendingType { get; set; }
        public string ReminderMessageTempName { get; set; }
        public string? ReminderMessage { get; set; }
        public string? ThanksMessage { get; set; }
        public string? ReminderTempId { get; set; }
        public string? ThanksTempId { get; set; }
        public string? DeclineTempId { get; set; }
        public string? FailedGuestsMessag { get; set; }
        public string? FailedGuestsCardText { get; set; }
        public string? LinkGuestsCardText { get; set; }
        public string? FailedGuestsLocationEmbedSrc { get; set; }
        public string? LinkGuestsLocationEmbedSrc { get; set; }
        public string? FailedGuestsReminderMessage { get; set; }
        public string? FailedGuestsCongratulationMsg { get; set; }
        public string? ReminderMsgHeaderImg { get; set; }
        public string? CongratulationMsgHeaderImg { get; set; }
        public int ChoosenNumberWithinCountry { get; set; }
        public string choosenSendingWhatsappProfile { get; set; }
        public string choosenSendingCountryNumber { get; set; }
        public string? ResponseInterestedOfMarketingMsg { get; set; }
        public string? ResponseInterestedOfMarketingMsgHeaderImage { get; set; }
        public string? ResponseNotInterestedOfMarketingMsg { get; set; }
        public string? ResponseNotInterestedOfMarketingMsgHeaderImage { get; set; }


        public virtual City City { get; set; }
        public virtual Users CreatedByNavigation { get; set; }
        public virtual Users CreatedForNavigation { get; set; }
        public virtual Users ModifiedByNavigation { get; set; }
        public virtual EventCategory TypeNavigation { get; set; }
        public virtual ICollection<CardInfo> CardInfo { get; set; }
        public virtual ICollection<EventGatekeeperMapping> EventGatekeeperMapping { get; set; }
        public virtual ICollection<Guest> Guest { get; set; }
        public virtual ICollection<Invoices> Invoices { get; set; }
        public virtual ICollection<GKEventHistory> GKEventHistories { get; set; }
        public virtual ICollection<EventOperator> EventOperators { get; set; }
    }
}
