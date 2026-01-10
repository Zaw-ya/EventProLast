using System;

namespace EventPro.DAL.Models
{
    public class ClientEventsModel
    {
        public int Id { get; set; }
        public string EventTitle { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public string EventVenue { get; set; }
    }
}
