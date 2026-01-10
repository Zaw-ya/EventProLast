using System.ComponentModel.DataAnnotations;

namespace EventPro.DAL.Models
{
    public class EventLocation
    {
        [Key]
        public int Id { get; set; }
        public string City { get; set; }
        public string Governorate { get; set; }
        public string Country { get; set; }

        public string GetLocationFormatted()
        {
            return $"{City},{Governorate}|{Country}";
        }
    }
}
