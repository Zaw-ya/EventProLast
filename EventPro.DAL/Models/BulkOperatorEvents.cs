using System;
using System.ComponentModel.DataAnnotations;

namespace EventPro.DAL.Models
{
    public class BulkOperatorEvents
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "From Operator is required")]
        public int? OperatorAssignedFromId { get; set; }
        [Required(ErrorMessage = "To Operator is required")]
        public int? OperatorAssignedToId { get; set; }
        public int AssignedById { get; set; }
        public DateTime AssignedOn { get; set; }
        public Users OperatorAssignedFrom { get; set; }
        public Users OperatorAssignedTo { get; set; }
        public Users AssignedBy { get; set; }

    }
}
