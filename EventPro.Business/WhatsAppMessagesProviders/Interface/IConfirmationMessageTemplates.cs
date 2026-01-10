using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IConfirmationMessageTemplates
    {
        //send Arabic Basic without guest Name
        Task SendArabicbasic(List<Guest> guests, Events events);
        Task SendArabicbasicHeaderText(List<Guest> guests, Events events);
        Task SendArabicbasicHeaderImage(List<Guest> guests, Events events);
        Task SendArabicbasicHeaderTextImage(List<Guest> guests, Events events);

        //send English Basic without guest Name
        Task SendEnglishbasic(List<Guest> guests, Events events);
        Task SendbasicHeaderTextEnglish(List<Guest> guests, Events events);
        Task SendbasicHeaderImageEnglish(List<Guest> guests, Events events);
        Task SendbasicHeaderTextImageEnglish(List<Guest> guests, Events events);

        //send Arabic Default with guest Name (Female)
        Task SendArabicFemaleDefault(List<Guest> guests, Events events);
        Task SendArabicFemaleWithHeaderImage(List<Guest> guests, Events events);
        Task SendArabicFemaleWithHeaderImageAndHeaderText(List<Guest> guests, Events events);
        Task SendArabicFemaleWithHeaderText(List<Guest> guests, Events events);

        //send Arabic Default with guest Name (Male)
        Task SendArabicMaleDefault(List<Guest> guests, Events events);
        Task SendArabicMaleWithHeaderImage(List<Guest> guests, Events events);
        Task SendArabicMaleWithHeaderImageAndHeaderText(List<Guest> guests, Events events);
        Task SendArabicMaleWithHeaderText(List<Guest> guests, Events events);

        //send English Default with guest Name 
        Task SendEnglishDefault(List<Guest> guests, Events events);
        Task SendEnglishWithHeaderImage(List<Guest> guests, Events events);
        Task SendEnglishWithHeaderText(List<Guest> guests, Events events);
        Task SendEnglishWithHeaderImageAndHeaderText(List<Guest> guests, Events events);

        //send Custom Basic without guest Name 
        Task SendCustomBasic(List<Guest> guests, Events events);
        Task SendCustomBasicHeaderText(List<Guest> guests, Events events);
        Task SendCustomBasicHeaderImage(List<Guest> guests, Events events);
        Task SendCustomBasicHeaderTextImage(List<Guest> guests, Events events);

        //send Custom template with guest Name 
        Task SendCustomWithName(List<Guest> guests, Events events);
        Task SendCustomWithNameHeaderText(List<Guest> guests, Events events);
        Task SendCustomWithNameHeaderImage(List<Guest> guests, Events events);
        Task SendCustomWithNameHeaderTextImage(List<Guest> guests, Events events);


        //Template with variables
        Task SendCustomTemplateWithVariables(List<Guest> guests, Events events);


    }
}
