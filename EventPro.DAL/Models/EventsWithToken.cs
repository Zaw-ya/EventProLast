using System;
using System.Collections.Generic;

namespace EventPro.DAL.Models
{
    public class EventsWithToken
    {
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public string EventVenue { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? AttendanceTime { get; set; }
        public List<string> Tokens { get; set; }
    }
}
