namespace EventPro.DAL.Models
{
    public class ScanSummary
    {
        public int Id { get; set; }
        public long? LinkedEvent { get; set; }
        public string EventTitle { get; set; }
        public string SystemEventTitle { get; set; }

        public int? TotalGuests { get; set; }
        public int Allowed { get; set; }
        public int Declined { get; set; }
    }
}
