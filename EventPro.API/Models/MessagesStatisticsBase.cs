namespace EventPro.API.Models
{
    public class MessagesStatisticsBase
    {
        public int ReadNumber { get; set; }
        public int DeliverdNumber { get; set; }
        public int SentNumber { get; set; }
        public int FailedNumber { get; set; }
        public int NotSentNumber { get; set; }
    }
}
