namespace EventPro.API.Models
{
    public class EventConfirmationMessagesStatistics
    {
        public int TotalGuestsNumber { get; set; }
        public int AcceptedGuestsNumber { get; set; }
        public int DeclienedGuestsNumber { get; set; }
        public int NoAnswerGuestsNumber { get; set; }
        public int FailedGuestsNumber { get; set; }
        public int NotSentGuestsNumber { get; set; }
        public int AttendedGuestsNumber { get; set; }

    }
}
