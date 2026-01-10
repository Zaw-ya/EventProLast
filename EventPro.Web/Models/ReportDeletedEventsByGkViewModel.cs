using System;
using System.Collections.Generic;

namespace EventPro.Web.Models
{
    public class ReportDeletedEventsByGkViewModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public long? LinkedTo { get; set; }
        public string SystemEventTitle { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public string EventVenue { get; set; }
        public string Location { get; set; }
        public DateTime DeletedOn { get; set; }
        public int DeletedById { get; set; }
        public string DeletedByName { get; set; }
        public List<string> AssignedNames { get; set; }

    }
}
