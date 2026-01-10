using System;

namespace EventPro.DAL.Models
{
    public class MobileLog
    {
        public long Id { get; set; }
        public long GKId { get; set; }
        public string DeviceInfo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long EventId { get; set; }
        public string QrCode { get; set; }
        public string ApiResponse { get; set; }
    }
}
