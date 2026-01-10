using EventPro.DAL.Models;

namespace EventPro.DAL.ViewModels
{

    public class ScanLogVM
    {
        public ScanLogVM(VwScanLogs scanLog)
        {
            EventId = scanLog.EventId.ToString();
            EventTitle = scanLog.SystemEventTitle?.ToString() ?? "null";
            EventLinkedTo = scanLog.LinkedEvent.ToString() ?? "--";
            GuestName = scanLog.GuestName?.ToString() ?? "Not Found";
            Validity = scanLog.Nos.ToString();
            Code = scanLog.ResponseCode.ToString();
            Message = scanLog.Response.ToString();
            GateKeeperId = scanLog.ScanBy.ToString();
            GateKeeperName = scanLog.ScannedBy.ToString();
            ScannedOn = scanLog.ScannedOn.Value.ToLocalTime().ToString();
        }

        public string EventId { get; set; }
        public string EventTitle { get; set; }
        public string EventLinkedTo { get; set; }
        public string GuestName { get; set; }
        public string Validity { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string GateKeeperId { get; set; }
        public string GateKeeperName { get; set; }
        public string ScannedOn { get; set; }

    }


}
