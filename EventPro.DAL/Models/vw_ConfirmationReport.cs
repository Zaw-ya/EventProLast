using Microsoft.EntityFrameworkCore;

namespace EventPro.DAL.Models
{
    [Keyless]
    public class vw_ConfirmationReport
    {
        public int Id { get; set; }
        public string EventTitle { get; set; }
        public string SystemEventTitle { get; set; }
        public long? LinkedEvent { get; set; }
        public int? YesResponse { get; set; }
        public int? NoResponse { get; set; }
        public int? WaitingResponse { get; set; }
        public int? TotalConfirmedGuests { get; set; }
        public int? TextSent { get; set; }
        public bool? IsDeleted { get; set; }

    }
}
