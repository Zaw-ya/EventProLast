using System;

namespace EventPro.DAL.Models
{
    public class EventScanHistory
    {
        public DateTime? ScannedOn { get; set; }
        public string ResponseCode { get; set; }
        public string Response { get; set; }
        public string GuestFullName { get; set; }
        public int? NoOfMembers { get; set; }
    }
}
