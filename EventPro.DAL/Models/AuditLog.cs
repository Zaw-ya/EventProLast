using EventPro.DAL.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace EventPro.DAL.Models
{
    public class AuditLog
    {
        [Key]
        public long Id { get; set; }
        public ActionEnum? Action { get; set; } = ActionEnum.AddEvent;
        public int? RelatedId { get; set; }
        public DateTime? CreatedOn { get; set; } = DateTime.Now;
        public string? Notes { get; set; }

        public int? UserId { get; set; }
        public int? EventId { get; set; }

        public virtual Users User { get; set; }
        public virtual Events Event { get; set; }
    }
}
