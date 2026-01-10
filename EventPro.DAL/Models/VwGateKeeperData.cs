using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class VwGateKeeperData
    {
        public long? RowId { get; set; }
        public string EventTitle { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public string EventVenue { get; set; }
        public string GmapCode { get; set; }
        public string FullName { get; set; }
        public int Id { get; set; }
        public int? GatekeeperId { get; set; }
        public int? Scanned { get; set; }
        public int? TotalAllocated { get; set; }
        public int? UniqueFamily { get; set; }
    }
}
