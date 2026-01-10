using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface ICongratulationsMessageTemplates
    {
        Task SendThanksCustom(List<Guest> guests, Events events);
        Task SendTemp1(List<Guest> guests, Events events);
        Task SendTemp2(List<Guest> guests, Events events);
        Task SendTemp3(List<Guest> guests, Events events);
        Task SendTemp4(List<Guest> guests, Events events);
        Task SendTemp5(List<Guest> guests, Events events);
        Task SendTemp6(List<Guest> guests, Events events);
        Task SendTemp7(List<Guest> guests, Events events);
        Task SendTemp8(List<Guest> guests, Events events);
        Task SendTemp9(List<Guest> guests, Events events);
        Task SendTemp10(List<Guest> guests, Events events);
        Task SendCongratulationMessageToOwner(List<Guest> guests, Events events, string message);
        Task SendCongratulationMessageToOwnerEnglish(List<Guest> guests, Events events, string message);
        Task SendThanksById(List<Guest> guests, Events events);
        Task SendThanksCustomWithHeaderImage(List<Guest> guests, Events events);
        Task SendTemp1WithHeaderImage(List<Guest> guests, Events events);
        Task SendTemp2WithHeaderImage(List<Guest> guests, Events events);
        Task SendTemp3WithHeaderImage(List<Guest> guests, Events events);
        Task SendTemp4WithHeaderImage(List<Guest> guests, Events events);
        Task SendTemp5WithHeaderImage(List<Guest> guests, Events events);
        Task SendTemp6WithHeaderImage(List<Guest> guests, Events events);
        Task SendTemp7WithHeaderImage(List<Guest> guests, Events events);
        Task SendTemp8WithHeaderImage(List<Guest> guests, Events events);
        Task SendTemp9WithHeaderImage(List<Guest> guests, Events events);
        Task SendTemp10WithHeaderImage(List<Guest> guests, Events events);
        Task SendThanksByIdWithHeaderImage(List<Guest> guests, Events events);

        //Template with variables
        Task SendCustomTemplateWithVariables(List<Guest> guests, Events events);
    }
}
