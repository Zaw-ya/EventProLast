using EventPro.DAL.Models;
using System.Threading.Tasks;

namespace EventPro.Services.TwilioService.Interface
{
    public interface ITwilioService
    {
        Task<Guest> SendArabicbasic(Guest guest, Events events);
        Task<Guest> SendArabicbasicHeaderText(Guest guest, Events events);
        Task<Guest> SendArabicbasicHeaderImage(Guest guest, Events events);
        Task<Guest> SendArabicbasicHeaderTextImage(Guest guest, Events events);
        Task<Guest> SendEnglishbasic(Guest guest, Events events);
        Task<Guest> SendbasicHeaderTextEnglish(Guest guest, Events events);
        Task<Guest> SendbasicHeaderImageEnglish(Guest guest, Events events);
        Task<Guest> SendbasicHeaderTextImageEnglish(Guest guest, Events events);
        Task<Guest> SendArabicFemaleDefault(Guest guest, Events events);
        Task<Guest> SendArabicFemaleWithHeaderImage(Guest guest, Events events);
        Task<Guest> SendArabicFemaleWithHeaderImageAndHeaderText(Guest guest, Events events);
        Task<Guest> SendArabicFemaleWithHeaderText(Guest guest, Events events);
        Task<Guest> SendArabicMaleDefault(Guest guest, Events events);
        Task<Guest> SendArabicMaleWithHeaderImage(Guest guest, Events events);
        Task<Guest> SendArabicMaleWithHeaderImageAndHeaderText(Guest guest, Events events);
        Task<Guest> SendArabicMaleWithHeaderText(Guest guest, Events events);
        Task<Guest> SendEnglishDefault(Guest guest, Events events);
        Task<Guest> SendEnglishWithHeaderImage(Guest guest, Events events);
        Task<Guest> SendEnglishWithHeaderText(Guest guest, Events events);
        Task<Guest> SendEnglishWithHeaderImageAndHeaderText(Guest guest, Events events);
        Task<string> SendArabicDuplicateAnswer(Guest guest, Events events);
        Task<string> SendEnglishDuplicateAnswer(Guest guest, Events events);
        Task<Guest> SendArabicEventLocation(Guest guest, Events events);
        Task<Guest> SendEnglishEventLocation(Guest guest, Events events);
        Task<Guest> SendCustomBasic(Guest guest, Events events);
        Task<Guest> SendCustomBasicHeaderText(Guest guest, Events events);
        Task<Guest> SendCustomBasicHeaderImage(Guest guest, Events events);
        Task<Guest> SendCustomBasicHeaderTextImage(Guest guest, Events events);
        Task<Guest> SendCustomWithName(Guest guest, Events events);
        Task<Guest> SendCustomWithNameHeaderText(Guest guest, Events events);
        Task<Guest> SendCustomWithNameHeaderImage(Guest guest, Events events);
        Task<Guest> SendCustomWithNameHeaderTextImage(Guest guest, Events events);
        Task<Guest> SendThanksCustom(Guest guest, Events events);
        Task<Guest> SendTemp1(Guest guest, Events events);
        Task<Guest> SendTemp2(Guest guest, Events events);
        Task<Guest> SendTemp3(Guest guest, Events events);
        Task<Guest> SendTemp4(Guest guest, Events events);
        Task<Guest> SendTemp5(Guest guest, Events events);
        Task<Guest> SendTemp6(Guest guest, Events events);
        Task<Guest> SendTemp7(Guest guest, Events events);
        Task<Guest> SendTemp8(Guest guest, Events events);
        Task<Guest> SendTemp9(Guest guest, Events events);
        Task<Guest> SendTemp10(Guest guest, Events events);
        Task<Guest> SendCongratulationMessageToOwner(Guest guest, string message);
        Task<Guest> SendCongratulationMessageToOwnerEnglish(Guest guest, string message);
        Task<string> SendEventProArabicService(string phoneNumber);
        Task<string> SendEventProArabicServiceKuwait(string phoneNumber);
        Task<string> SendEventProEnglishService(string phoneNumber);
        Task<Guest> SendArabicCard(Guest guest, Events events);
        Task<Guest> SendArabicCardwithname(Guest guest, Events events);
        Task<Guest> SendEnglishCard(Guest guest, Events events);
        Task<Guest> SendEnglishCardwithname(Guest guest, Events events);
        Task<Guest> SendReminderCustom(Guest guest, Events events);
        Task<Guest> SendRTemp1(Guest guest, Events events);
        Task<Guest> SendRTemp2or3(Guest guest, Events events);
        Task<Guest> SendReminderWithTempId(Guest guest, Events events);
        Task<Guest> SendThanksById(Guest guest, Events events);
        Task<Guest> SendCardByIDBasic(Guest guest, Events events);
        Task<Guest> SendCardByIDWithGusetName(Guest guest, Events events);
        Task<Guest> SendDeclineTemp(Guest guest, Events events);
        Task<string> GetMessageStatusAndUpdateGuestAsync(Guest guest);
        Task<string> GetMessagesAndUpdateEventGuestsAsync(Events events);
        Task<bool> ValidatePhoneNumberAsync(Guest guest);
        Task<Guest> SendDeclineTempFixedTemp(Guest guest, Events events);
        Task<string> SendEventProArabicServiceBahrain(string phoneNumber);
        Task CheckSingleAccountAsync(TwilioProfileSettings acc);
        Task<BalanceResponse?> GetBalanceAsync(string accountSid, string authToken);
    }
 
}







