using System;

namespace EventPro.Web.Models
{
    public class EventByGatekeeperModel
    {
        public int Id { get; set; }
        public long? linkedTo { get; set; }
        public string EventTitle { get; set; }
        public string SystemEventTitle { get; set; }

        public string Icon { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public string EventVenue { get; set; }
        public int? CreatedFor { get; set; }
        public string Location { get; set; }
        public string GatekeeperIds { get; set; }
        public string GatekeeperNames { get; set; }
    }
}