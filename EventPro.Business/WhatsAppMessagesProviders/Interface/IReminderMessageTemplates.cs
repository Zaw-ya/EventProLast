using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IReminderMessageTemplates
    {
        Task SendReminderCustom(List<Guest> guests, Events events);
        Task SendRTemp1(List<Guest> guests, Events events);
        Task SendRTemp1WithCalenderICS(List<Guest> guests, Events events);
        Task SendRTemp2or3(List<Guest> guests, Events events);
        Task SendRTemp2or3WithCalenderICS(List<Guest> guests, Events events);
        Task SendReminderWithTempId(List<Guest> guests, Events events);
        Task SendReminderCustomWithHeaderImage(List<Guest> guests, Events events);
        Task SendRTemp1WithHeaderImage(List<Guest> guests, Events events);
        Task SendRTemp1WithHeaderImageWithCalenderICS(List<Guest> guests, Events events);
        Task SendRTemp2or3WithHeaderImage(List<Guest> guests, Events events);
        Task SendRTemp2or3WithHeaderImageWithCalenderIcs(List<Guest> guests, Events events);
        Task SendReminderWithTempIdWithHeaderImage(List<Guest> guests, Events events);
        Task SendMarketingInterestedMsg(List<Guest> guests, Events events);
        Task SendMarketingInterestedMsgWithHeaderImage(List<Guest> guests, Events events);
        Task SendMarketingNotInterestedMsg(List<Guest> guests, Events events);
        Task SendMarketingNotInterestedMsgWithHeaderImage(List<Guest> guests, Events events);

        //Template with variables
        Task SendCustomTemplateWithVariables(List<Guest> guests, Events events);
    }
}
