using System.ComponentModel.DataAnnotations;

namespace EventPro.DAL.Models
{
    public class City
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CityName { get; set; }
        public int CountryId { get; set; }
        public virtual Country Country { get; set; }
    }
}
