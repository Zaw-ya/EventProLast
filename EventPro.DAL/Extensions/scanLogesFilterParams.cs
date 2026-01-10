using System;

namespace EventPro.DAL.Extensions
{
    public class scanLogesFilterParams
    {
        public DateTime? scanFrom { get; set; }
        public DateTime? scanTo { get; set; }
        public string? scanCode { get; set; }
        public int? GateKeeperId { get; set; }
        public int? EventId { get; set; }
    }
}
