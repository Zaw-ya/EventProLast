using System;

namespace EventPro.Web.Models
{
    public class TotalGuestModel
    {
        public int Id { get; set; }
        public long? linkedTo { get; set; }
        public string EventTitle { get; set; }
        public string SystemEventTitle { get; set; }
        public string Icon { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public string EventVenue { get; set; }
        public string Location { get; set; }
        public DateTime? LocationFrom { get; set; }
        public int? TotalGuests { get; set; }
    }
}
