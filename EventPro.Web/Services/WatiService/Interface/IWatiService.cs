using EventPro.DAL.Models;
using System.Threading.Tasks;

namespace EventPro.Services.WatiService.Interface
{
    public interface IWatiService
    {
        public Task<string> SendArabicFemaleInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendArabicMaleInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendEnglishInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendEnglishInvitaionTemplateWithHeaderText(Guest guest, Events events);
        public Task<string> SendEnglishInvitaionTemplateWithHeaderImage(Guest guest, Events events);
        public Task<string> SendEnglishInvitaionTemplateWihtHeaderTextAndHeaderImage(Guest guest, Events events);
        public Task<string> SendWorkInvitationTemplate(Guest guest, Events events);
        public Task<string> SendWorkInvitationTemplateWithHeaderText(Guest guest, Events events);
        public Task<string> SendWorkInvitationTemplateWithHeaderImage(Guest guest, Events events);
        public Task<string> SendWorkInvitationTemplateWithHeaderTextAndHeaderImage(Guest guest, Events events);
        public Task<string> SendCustomInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendCustomInvitaionWithClientNameTemplate(Guest guest, Events events);
        public Task<string> SendCardInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendEnglishCardInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendEventLocationTemplate(Guest guest, Events events);
        public Task<string> SendEnglishEventLocationTemplate(Guest guest, Events events);
        public Task<string> SendUserAccountDetailsTemplate(Users user);
        public Task<string> SendEventProServiceTemplate(string phoneNumber);
        public Task<string> SendGateKeeperCheckInTemplate(Guest guest, Events events);
        public Task<string> SendGateKeeperCheckInImageTemplate(Guest guest, Events events);
        public Task<string> SendGateKeeperCheckOutTemplate(Guest guest, Events events);
        public Task<string> SendDuplicateAnswerMessageTemplate(Guest guest, Events events);
        public Task<string> SendEnglishDuplicateAnswerMessageTemplate(Guest guest, Events events);
        public Task<string> SendArabicFemaleInvitaionTemplateWithHeaderText(Guest guest, Events events);
        public Task<string> SendArabicMaleInvitaionTemplateWithHeaderText(Guest guest, Events events);
        public Task<string> SendArabicFemaleInvitaionTemplateWithHeaderImage(Guest guest, Events events);
        public Task<string> SendArabicMaleInvitaionTemplateWithHeaderImage(Guest guest, Events events);
        public Task<string> SendArabicFemaleInvitaionTemplateWithHeaderImageAndHeaderText(Guest guest, Events events);
        public Task<string> SendArabicMaleInvitaionTemplateWithHeaderImageAndHeaderText(Guest guest, Events events);
        public Task<string> SendSaudiFoodSaveInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendEICHHOLTZInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendQRInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendCustomCardWithClientNameInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendEnglishCustomCardWithClientNameInvitaionTemplate(Guest guest, Events events);
        public Task<string> SendCongratulationMessageTemplate(Guest guest, Events events);
        public Task<string> SendCongratulationMessageToPrideTemplate(Guest guest, string message);
        public Task<string> SendCustomReminderMessageTemplate(Guest guest, Events events);
        public Task<string> SendCustomReminderMessageWithGuesttNameTemplate(Guest guest, Events events);
        public Task<string> SendReminderMessageTemplate1(Guest guest, Events events);
        public Task<string> SendReminderMessageTemplate2(Guest guest, Events events);
        public Task<string> SendReminderMessageTemplate3(Guest guest, Events events);
        public Task<string> SendGateKeeperReminderMessage(GKWhatsRemiderMsgModel model);
        public Task<string> SendGateKeeperTodayReminderMessage(GKWhatsRemiderMsgModel model);

    }
}
