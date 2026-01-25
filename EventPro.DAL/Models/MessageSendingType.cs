namespace EventPro.DAL.Models
{
    public enum MessageSendingType
    {
        Confirmation = 1,
        Card = 2,
        Reminder = 3,
        Congratulation = 4,
        EventLocation = 5,
        Decline = 6,
        Duplicate = 7,
        GateKeeper = 8
    }
}
