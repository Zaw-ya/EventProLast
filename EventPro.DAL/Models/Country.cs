using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventPro.DAL.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CountryName { get; set; }
        public virtual ICollection<City> Cities { get; set; }
    }
}
