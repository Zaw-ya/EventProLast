using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IDuplicateMessageTemplates
    {
        Task SendArabicDuplicateAnswer(List<Guest> guests, Events events);
        Task SendEnglishDuplicateAnswer(List<Guest> guests, Events events);
    }
}
