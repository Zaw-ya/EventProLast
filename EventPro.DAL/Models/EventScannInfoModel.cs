
namespace EventPro.DAL.Models
{
    public class EventScannInfoModel
    {
        public int? GuestId { get; set; }
        public string? GuestName { get; set; }
        public int? NoOfMembers { get; set; }
        public int Scanned { get; set; }
        public string? Response { get; set; }
        public string? WhatsappStatus { get; set; }
    }
}
