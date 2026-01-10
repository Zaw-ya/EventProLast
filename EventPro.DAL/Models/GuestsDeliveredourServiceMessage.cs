using System.ComponentModel.DataAnnotations;

namespace EventPro.DAL.Models
{
    public class GuestsDeliveredourServiceMessage
    {
        [Key]
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string AddedDate { get; set; }
        public string ProviderName { get; set; }
        public string ProviderNumber { get; set; }
    }
}
