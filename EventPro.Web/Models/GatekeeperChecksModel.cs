using System;

namespace EventPro.Web.Models
{
    public class GatekeeperChecksModel
    {
        public int Id { get; set; }
        public long? linkedTo { get; set; }
        public string EventTitle { get; set; }
        public string Icon { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
        public string EventVenue { get; set; }
        public string Location { get; set; }
        public int GatekeeperId { get; set; }
        public string GatekeeperName { get; set; }
        public string CheckType { get; set; }
        public string LogDate { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
    }
}
