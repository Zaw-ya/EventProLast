using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class VwGateKeeperScheduled
    {
        public int TaskId { get; set; }
        public int? EventId { get; set; }
        public int? GatekeeperId { get; set; }
        public DateTime? AsssignedOn { get; set; }
        public int? AssignedBy { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public string EventTitle { get; set; }
        public string EventVenue { get; set; }
    }
}
