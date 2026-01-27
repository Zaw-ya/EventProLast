using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace EventPro.DAL.Models
{
    public class TwilioProfileSettings
    {
        
        [Key]
        public int Id { get; set; }

        // Account main settings
        [Required]
        public string Name { get; set; }
        [Required]
        public string AuthToken { get; set; }
        [Required]
        public string AccountSid { get; set; }
        [Required]
        public string MessagingServiceSid { get; set; }
        public string? WhatsAppNumberSaudi1 { get; set; }
        public string? WhatsAppNumberSaudi2 { get; set; }
        public string? WhatsAppNumberKuwait1 { get; set; }
        public string? WhatsAppNumberKuwait2 { get; set; }
        public string? WhatsAppNumberBahrain1 { get; set; }
        public string? WhatsAppNumberBahrain2 { get; set; }
        public string? WhatsAppNumberEgypt1 { get; set; }
        public string? WhatsAppNumberEgypt2 { get; set; }

        //messaging templates 

        //confirmation Templates 

        //Female
        public string? ConfirmArabicFemaleWithoutGuestName { get; set; }
        public string? ConfirmArabicFemaleWithHeaderTextAndWithoutGuestName { get; set; }
        public string? ConfirmArabicFemaleWithHeaderImageAndWithoutGuestName { get; set; }
        public string? ConfirmArabicFemaleWithHeaderVideoAndWithoutGuestName { get; set; }
        public string? ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithoutGuestName { get; set; }

        public string? ConfirmArabicFemaleWithGuestName { get; set; }
        public string? ConfirmArabicFemaleWithHeaderTextAndWithGuestName { get; set; }
        public string? ConfirmArabicFemaleWithHeaderImageAndWithGuestName { get; set; }
        public string? ConfirmArabicFemaleWithHeaderVideoAndWithGuestName { get; set; }
        public string? ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithGuestName { get; set; }

        //Female with link
        public string? ConfirmArabicFemaleWithoutGuestNameWithLink { get; set; }
        public string? ConfirmArabicFemaleWithHeaderTextAndWithoutGuestNameWithLink { get; set; }
        public string? ConfirmArabicFemaleWithHeaderImageAndWithoutGuestNameWithLink { get; set; }
        public string? ConfirmArabicFemaleWithHeaderVideoAndWithoutGuestNameWithLink { get; set; }
        public string? ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink { get; set; }

        public string? ConfirmArabicFemaleWithGuestNameWithLink { get; set; }
        public string? ConfirmArabicFemaleWithHeaderTextAndWithGuestNameWithLink { get; set; }
        public string? ConfirmArabicFemaleWithHeaderImageAndWithGuestNameWithLink { get; set; }
        public string? ConfirmArabicFemaleWithHeaderVideoAndWithGuestNameWithLink { get; set; }
        public string? ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithGuestNameWithLink { get; set; }



        //Male
        public string? ConfirmArabicMaleWithoutGuestName { get; set; }
        public string? ConfirmArabicMaleWithHeaderTextAndWithoutGuestName { get; set; }
        public string? ConfirmArabicMaleWithHeaderImageAndWithoutGuestName { get; set; }
        public string? ConfirmArabicMaleWithHeaderVideoAndWithoutGuestName { get; set; }
        public string? ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithoutGuestName { get; set; }

        public string? ConfirmArabicMaleWithGuestName { get; set; }
        public string? ConfirmArabicMaleWithHeaderTextAndWithGuestName { get; set; }
        public string? ConfirmArabicMaleWithHeaderImageAndWithGuestName { get; set; }
        public string? ConfirmArabicMaleWithHeaderVideoAndWithGuestName { get; set; }
        public string? ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithGuestName { get; set; }

        //Male with link
        public string? ConfirmArabicMaleWithoutGuestNameWithLink { get; set; }
        public string? ConfirmArabicMaleWithHeaderTextAndWithoutGuestNameWithLink { get; set; }
        public string? ConfirmArabicMaleWithHeaderImageAndWithoutGuestNameWithLink { get; set; }
        public string? ConfirmArabicMaleWithHeaderVideoAndWithoutGuestNameWithLink { get; set; }
        public string? ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink { get; set; }

        public string? ConfirmArabicMaleWithGuestNameWithLink { get; set; }
        public string? ConfirmArabicMaleWithHeaderTextAndWithGuestNameWithLink { get; set; }
        public string? ConfirmArabicMaleWithHeaderImageAndWithGuestNameWithLink { get; set; }
        public string? ConfirmArabicMaleWithHeaderVideoAndWithGuestNameWithLink { get; set; }
        public string? ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithGuestNameWithLink { get; set; }


        //English
        public string? ConfirmEnglishWithoutGuestName { get; set; }
        public string? ConfirmEnglishWithHeaderTextAndWithoutGuestName { get; set; }
        public string? ConfirmEnglishWithHeaderImageAndWithoutGuestName { get; set; }
        public string? ConfirmEnglishWithHeaderVideoAndWithoutGuestName { get; set; }
        public string? ConfirmEnglishWithHeaderImageAndHeaderTextAndWithoutGuestName { get; set; }

        public string? ConfirmEnglishWithGuestName { get; set; }
        public string? ConfirmEnglishWithHeaderTextAndWithGuestName { get; set; }
        public string? ConfirmEnglishWithHeaderImageAndWithGuestName { get; set; }
        public string? ConfirmEnglishWithHeaderVideoAndWithGuestName { get; set; }
        public string? ConfirmEnglishWithHeaderImageAndHeaderTextAndWithGuestName { get; set; }

        //English with link
        public string? ConfirmEnglishWithoutGuestNameWithLink { get; set; }
        public string? ConfirmEnglishWithHeaderTextAndWithoutGuestNameWithLink { get; set; }
        public string? ConfirmEnglishWithHeaderImageAndWithoutGuestNameWithLink { get; set; }
        public string? ConfirmEnglishWithHeaderVideoAndWithoutGuestNameWithLink { get; set; }
        public string? ConfirmEnglishWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink { get; set; }

        public string? ConfirmEnglishWithGuestNameWithLink { get; set; }
        public string? ConfirmEnglishWithHeaderTextAndWithGuestNameWithLink { get; set; }
        public string? ConfirmEnglishWithHeaderImageAndWithGuestNameWithLink { get; set; }
        public string? ConfirmEnglishWithHeaderVideoAndWithGuestNameWithLink { get; set; }
        public string? ConfirmEnglishWithHeaderImageAndHeaderTextAndWithGuestNameWithLink { get; set; }


        //Card
        public string? ArabicCardWithoutGuestName { get; set; }
        public string? ArabicCardWithGuestName { get; set; }
        public string? EnglihsCardWithoutGuestName { get; set; }
        public string? EnglishCardWithGuestName { get; set; }


        //Thanks 
        public string? CustomThanksWithoutGuestName { get; set; }
        public string? CustomThanksWithGuestName { get; set; }
        public string? ThanksTemp1 { get; set; }
        public string? ThanksTemp2 { get; set; }
        public string? ThanksTemp3 { get; set; }
        public string? ThanksTemp4 { get; set; }
        public string? ThanksTemp5 { get; set; }
        public string? ThanksTemp6 { get; set; }
        public string? ThanksTemp7 { get; set; }
        public string? ThanksTemp8 { get; set; }
        public string? ThanksTemp9 { get; set; }
        public string? ThanksTemp10 { get; set; }
        public string? CustomThanksWithoutGuestNameWithHeaderImage { get; set; }
        public string? CustomThanksWithGuestNameWithHeaderImage { get; set; }
        public string? ThanksTemp1WithHeaderImage { get; set; }
        public string? ThanksTemp2WithHeaderImage { get; set; }
        public string? ThanksTemp3WithHeaderImage { get; set; }
        public string? ThanksTemp4WithHeaderImage { get; set; }
        public string? ThanksTemp5WithHeaderImage { get; set; }
        public string? ThanksTemp6WithHeaderImage { get; set; }
        public string? ThanksTemp7WithHeaderImage { get; set; }
        public string? ThanksTemp8WithHeaderImage { get; set; }
        public string? ThanksTemp9WithHeaderImage { get; set; }
        public string? ThanksTemp10WithHeaderImage { get; set; }

        public string? ArabicCongratulationMessageToEventOwner { get; set; }
        public string? EnglishCongratulationMessageToEventOwner { get; set; }


        //Duplication
        public string? ArabicDuplicateAnswer { get; set; }
        public string? EnglishDuplicateAnswer { get; set; }

        
        //Event Location
        public string? ArabicEventLocation { get; set; }
        public string? EnglishEventLocation { get; set; }


        //Decline
        public string? ArabicDecline { get; set; }


        //Reminder
        public string? CustomReminderWithoutGuestName { get; set; }
        public string? CustomReminderWithGuestName { get; set; }
        public string? ReminderTemp1 { get; set; }
        public string? ReminderTemp1WithCalenderIcs { get; set; }
        public string? ReminderTemp2 { get; set; }
        public string? ReminderTemp2WithCalenderIcs { get; set; }
        public string? ReminderTemp3 { get; set; }
        public string? ReminderTemp3WithCalenderIcs { get; set; }
        public string? CustomReminderWithoutGuestNameWithHeaderImage { get; set; }
        public string? CustomReminderWithGuestNameWithHeaderImage { get; set; }
        public string? ReminderTemp1WithHeaderImage { get; set; }
        public string? ReminderTemp1WithHeaderImageWithCalenderIcs { get; set; }
        public string? ReminderTemp2WithHeaderImage { get; set; }
        public string? ReminderTemp2WithHeaderImageWithCalenderIcs { get; set; }
        public string? ReminderTemp3WithHeaderImage { get; set; }
        public string? ReminderTemp3WithHeaderImageWithCalenderIcs { get; set; }
        public string? MarketingInterestMsg { get; set; }
        public string? MarketingInterestMsgWithImage { get; set; }
        public string? MarketingNotInterestMsg { get; set; }
        public string? MarketingNotInterestMsgWithImage { get; set; }

    }
}
