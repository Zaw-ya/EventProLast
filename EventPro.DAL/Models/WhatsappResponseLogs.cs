using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class WhatsappResponseLogs
    {
        public int WaId { get; set; }
        public string Wakey { get; set; }
        public string Type { get; set; }
        public string Response { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? Ticks { get; set; }
        public string RecepientNo { get; set; }
    }
}
