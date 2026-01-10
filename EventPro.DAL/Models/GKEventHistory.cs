using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventPro.DAL.Models
{
    public class GKEventHistory
    {
        [Key]
        public int Id { get; set; }

        public int GK_Id { get; set; }

        public int Event_Id { get; set; }
        public string ImagePath { get; set; }
        public DateTime LogDT { get; set; }
        public string CheckType { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        [ForeignKey("GK_Id")]
        public virtual Users Gatekeeper { get; set; }
        [ForeignKey("Event_Id")]
        public virtual Events Event { get; set; }
    }
}
