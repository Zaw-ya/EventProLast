using System;

namespace EventPro.API.Models
{
    public class CalenderFreeEvents
    {
        public int Id { get; set; }
        public string EventTitle { get; set; }
        public string EventVenue { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public string ParentTitle { get; set; }
        public string EventLocation { get; set; }
    }
}
