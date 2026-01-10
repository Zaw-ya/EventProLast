using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class VwGuestReportWa
    {
        public int GuestId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public string FirstName { get; set; }
        public string PrimaryContactNo { get; set; }
        public DateTime? WaresponseTime { get; set; }
        public string Response { get; set; }
        public string MessageId { get; set; }
        public bool? TextDelivered { get; set; }
        public bool? TextSent { get; set; }
        public bool? TextRead { get; set; }
        public bool? TextFailed { get; set; }
        public bool? ImgDelivered { get; set; }
        public bool? ImgSent { get; set; }
        public bool? ImgRead { get; set; }
        public bool? ImgFailed { get; set; }
        public long? LinkedEvent { get; set; }
    }
}
