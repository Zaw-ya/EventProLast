using System;
using System.Collections.Generic;

namespace EventPro.DAL.Models
{
    public class GKWhatsRemiderMsgModel
    {

        public int EventID { get; set; }
        public List<GkDetailsWhatsRemiderMsg> GkDetails { get; set; }
        public string Title { get; set; }
        public string EventTitle { get; set; }
        public string EventVenue { get; set; }
        public DateTime? AttendanceTime { get; set; }
    }
    public class GkDetailsWhatsRemiderMsg
    {
        public string GKPhoneNumber { get; set; }
        public string GKFName { get; set; }
    }
}
