using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IEventLocationMessageTemplates
    {
        Task SendArabicEventLocation(List<Guest> guests, Events events);
        Task SendEnglishEventLocation(List<Guest> guests, Events events);
    }
}
