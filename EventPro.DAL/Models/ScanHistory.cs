using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class ScanHistory
    {
        public int ScanId { get; set; }
        public int? ScanBy { get; set; }
        public DateTime? ScannedOn { get; set; }
        public string ScannedCode { get; set; }
        public int? GuestId { get; set; }
        public string ResponseCode { get; set; }
        public string Response { get; set; }

        public virtual Guest Guest { get; set; }
        public virtual Users ScanByNavigation { get; set; }
    }
}
