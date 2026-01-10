using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPro.DAL.Models
{
    [Table("ReportDeletedEventsByGk")]
    public class ReportDeletedEventsByGk
    {
        [Key]
        public int Id { get; set; }

        public int EventId { get; set; }

        public int GatekeeperId { get; set; }

        public DateTime UnassignedOn { get; set; }

        public int UnassignedById { get; set; }

        public string UnassignedByName { get; set; }
    }
}
