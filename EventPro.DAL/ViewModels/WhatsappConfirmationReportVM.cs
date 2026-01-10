using EventPro.DAL.Models;

namespace EventPro.DAL.ViewModels
{
    public class WhatsappConfirmationReportVM
    {
        public WhatsappConfirmationReportVM(vw_ConfirmationReport data)
        {
            EventCode = string.Concat("E0000", data.Id);
            LinkedTo = data.LinkedEvent.ToString() ?? "--";
            EventTitle = data.SystemEventTitle;
            EventId = data.Id;
            Sent = data.TextSent.ToString() ?? "0";
            CountYes = data.YesResponse.ToString() ?? "0";
            ConfirmedGuests = data.TotalConfirmedGuests ?? 0;
            CountNo = data.NoResponse.ToString() ?? "0";
            Waiting = data.WaitingResponse.ToString() ?? "0";
        }

        public int EventId { get; set; }
        public string EventCode { get; set; }
        public string LinkedTo { get; set; }
        public string EventTitle { get; set; }
        public string Sent { get; set; }
        public string CountYes { get; set; }
        public int ConfirmedGuests { get; set; }
        public string CountNo { get; set; }
        public string Waiting { get; set; }
    }
}
