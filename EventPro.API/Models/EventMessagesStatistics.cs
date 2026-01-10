namespace EventPro.API.Models
{
    public class EventMessagesStatistics
    {

        public ConfirmationMessagesStatistics ConfirmationMessages { get; set; }

        public CardMessagesStatistics CardMessages { get; set; }

        public EventLocationMessagesStatistics EventLocationMessages { get; set; }

        public ReminderMessagesStatistics ReminderMessages { get; set; }

        public CongratulationMessagesStatistics CongratulationMessages { get; set; }

    }
}
