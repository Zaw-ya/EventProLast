using System;

namespace EventPro.DAL.Models
{
    public class EventsStatsByGK
    {
        public string EventTitle { get; set; }
        public string SystemEventTitle { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public string EventVenue { get; set; }
        public int Id { get; set; }
        public int? Scanned { get; set; }
        public int? TotalAllocated { get; set; }
        public string GmapCode { get; set; }
        public string Eventlocation { get; set; }
        public string EventCode { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string LeaveTime { get; set; }
        public string AttendanceTime { get; set; }
    }
}
