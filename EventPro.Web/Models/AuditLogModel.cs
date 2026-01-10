using System;

namespace EventPro.Web.Models
{
    public class AuditLogModel
    {
        public long Id { get; set; }
        public string Action { get; set; }
        public int? RelatedId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UserName { get; set; }
        public int? EventId { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public long? linkedTo { get; set; }
        public string? EventTitle { get; set; }
        public string? Desc { get; set; }
    }
}