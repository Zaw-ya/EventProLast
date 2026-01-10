using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface ICardMessageTemplates
    {
        Task SendArabicCard(List<Guest> guests, Events events);
        Task SendArabicCardwithname(List<Guest> guests, Events events);

        Task SendEnglishCard(List<Guest> guests, Events events);
        Task SendEnglishCardwithname(List<Guest> guests, Events events);

        //custom card templates
        Task SendCardByIDBasic(List<Guest> guests, Events events);
        Task SendCardByIDWithGusetName(List<Guest> guests, Events events);

        //Template with variables
        Task SendCustomTemplateWithVariables(List<Guest> guests, Events events);
    }
}
