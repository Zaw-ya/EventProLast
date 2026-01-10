using Microsoft.EntityFrameworkCore;
using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    [Keyless]
    public partial class VwScanLogs
    {
        public int ScanId { get; set; }
        public int? ScanBy { get; set; }
        public int EventId { get; set; }
        public DateTime? ScannedOn { get; set; }
        public string ScannedCode { get; set; }
        public int? GuestId { get; set; }
        public string ResponseCode { get; set; }
        public string Response { get; set; }
        public string ScannedBy { get; set; }
        public string GuestName { get; set; }
        public int? Nos { get; set; }
        public string EventTitle { get; set; }
        public string SystemEventTitle { get; set; }
        public string EventCode { get; set; }
        public int? CreatedFor { get; set; }
        public long? LinkedEvent { get; set; }
    }
}
