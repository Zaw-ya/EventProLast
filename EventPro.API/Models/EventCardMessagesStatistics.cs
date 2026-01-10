namespace EventPro.API.Models
{
    public class EventCardMessagesStatistics
    {
        public int TotalGuestsNumber { get; set; }
        public int DeliveredGuestsNumber { get; set; }
        public int FailedGuestsNumber { get; set; }
        public int NotSentGuestsNumber { get; set; }
        public int AttendedGuestsNumber { get; set; }

    }
}
