using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class EventCategory
    {
        public EventCategory()
        {
            Events = new HashSet<Events>();
        }

        public int EventId { get; set; }
        public string Category { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public bool? Status { get; set; }
        public string Icon { get; set; }

        public virtual Users CreatedByNavigation { get; set; }
        public virtual ICollection<Events> Events { get; set; }
    }
}
