using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class VwScanHistory
    {
        public int ScanId { get; set; }
        public int? ScanBy { get; set; }
        public DateTime? ScannedOn { get; set; }
        public string ScannedCode { get; set; }
        public int? GuestId { get; set; }
        public string ResponseCode { get; set; }
        public string Response { get; set; }
        public string FirstName { get; set; }
        public int? NoOfMembers { get; set; }
        public string EventTitle { get; set; }
        public string EventVenue { get; set; }
        public DateTime? EventFrom { get; set; }
        public string EventDescription { get; set; }
    }
}
