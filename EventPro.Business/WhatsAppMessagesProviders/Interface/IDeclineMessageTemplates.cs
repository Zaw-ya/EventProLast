using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IDeclineMessageTemplates
    {
        Task SendDeclineTemplate(List<Guest> guests, Events events);
        Task SendCustomDeclineTemplate(List<Guest> guests, Events events);
    }
}
