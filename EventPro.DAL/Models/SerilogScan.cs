using System;
using System.ComponentModel.DataAnnotations;

namespace EventPro.DAL.Models
{
    public class SerilogScan
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
