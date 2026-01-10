using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IMessagesSendingFactory
    {
        Task SendConfirmationMessagesAsync(List<Guest> guests, Events events);
        IConfirmationMessageTemplates GetConfirmationMessageTemplates();
        Task SendCardMessagesAsync(List<Guest> guests, Events events);
        ICardMessageTemplates GetCardMessageTemplates();
        Task SendEventLocationAsync(List<Guest> guests, Events events);
        IEventLocationMessageTemplates GetEventLocationTemplates();
        Task SendDuplicateAnswerAsync(List<Guest> guests, Events events );
        IDuplicateMessageTemplates GetDuplicateAnswerTemplates();
        Task SendReminderMessageAsync(List<Guest> guests, Events events);
        IReminderMessageTemplates GetReminderMessageTemplates();
        Task SendCongratulationMessageAsync(List<Guest> guests, Events events);
        ICongratulationsMessageTemplates GetCongratulatioinMessageTemplates();
        Task SendDeclineMessageAsync(List<Guest> guests, Events events);
        IDeclineMessageTemplates GetDeclineMessageTemplates();
    }
}
