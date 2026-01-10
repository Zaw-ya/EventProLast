using System.ComponentModel;

namespace EventPro.DAL.Enum
{
    public enum ActionEnum

    {
        [Description("Add Event")]
        AddEvent = 1,
        [Description("Update Event")]
        UpdateEvent = 2,
        [Description("Delete Event")]
        DeleteEvent = 3,

        [Description("Add Guest")]
        AddGuest = 4,
        [Description("Update Guest")]
        UpdateGuest = 5,
        [Description("Delete Guest")]
        DeleteGuest = 6,
        [Description("Upload Guest")]
        UploadGuest = 7,
        [Description("Update QR Code")]
        UpdateQRCode = 8,
        [Description("Update Card Design")]
        UpdateCardDesign = 9,
        [Description("Refresh Cards")]
        RefreshCards = 10,
        [Description("Download Cards")]
        DownloadCards = 11,
        [Description("Send Confirmation")]
        SendConfirmation = 12,
        [Description("Send Cards")]
        SendCards = 13,
        [Description("Send Event Location")]
        SendEventLocation = 14,
        [Description("Send Thanks")]
        SendThanks = 15,
        [Description("Send Reminder To All")]
        SendReminderToAll = 16,
        [Description("Send Reminder To Only Accepted")]
        SendReminderToOnlyAccepted = 17,
        [Description("Send Reminder To Only No Answer")]
        SendReminderToOnlyNoAnswer = 18,
        [Description("Send Reminder To Only Received")]
        SendReminderToOnlyReceived = 19,
        [Description("Restore Event")]
        RestoreEvent = 20,
        [Description("CreateUser")]
        CreateUser = 21,
        [Description("Update User")]
        UpdateUser = 22,
        [Description("Activate Or Deactivate User")]
        ActivateOrDeactivateUser = 23,
        [Description("Delete All Guests")]
        DeleteAllGuests = 24,
        [Description("Delete All Guests Cards")]
        DeleteAllGuestsCards = 25,
        [Description("Reset All Guests Status")]
        ResetAllGuestsStatus = 26,
        [Description("Allow Send Confirmation Again")]
        AllowSendConfirmationAgain = 27,
        [Description("Allow Send Cards Again")]
        AllowSendCardsAgain = 28,
        [Description("Allow Send Event Location Again")]
        AllowSendEventLocationAgain = 29,
        [Description("Allow Send Reminders Again")]
        AllowSendRemindersAgain = 30,
        [Description("Allow Send Congratulations Again")]
        AllowSendCongratulationsAgain = 31,
    }

}