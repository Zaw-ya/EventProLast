// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class VwMiagregationReport
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public int? YesResponse { get; set; }
        public int? NoResponse { get; set; }
        public int? WaitingResponse { get; set; }
        public int? TotalFamily { get; set; }
        public int? TotalGuest { get; set; }
        public int? Allowed { get; set; }
        public int? Declined { get; set; }
        public int? TextDelivered { get; set; }
        public int? TextSent { get; set; }
        public int? TextRead { get; set; }
        public int? TextFailed { get; set; }
        public int? ImgDelivered { get; set; }
        public int? ImgSent { get; set; }
        public int? ImgRead { get; set; }
        public int? ImgFailed { get; set; }
        public int? WhatsappNotExists { get; set; }
        public long? LinkedEvent { get; set; }
    }
}
