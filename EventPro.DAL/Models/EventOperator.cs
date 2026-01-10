using System;

namespace EventPro.DAL.Models
{
    public class EventOperator
    {
        public int OperatorId { get; set; }
        public Users Operator { get; set; }

        public int EventId { get; set; }
        public Events Event { get; set; }

        public DateTime? EventStart { get; set; }
        public DateTime? EventEnd { get; set; }

        public int? BulkOperatroEventsId { get; set; }
    }
}
