using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventPro.DAL.Models
{
    public class ConfirmationMessageResponsesKeyword
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string KeywordKey { get; set; }   

        [Required]
        [MaxLength(20)]
        public string LanguageCode { get; set; } 

        [Required]
        [MaxLength(200)]
        public string KeywordValue { get; set; } 

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }


        [Required]
        public int CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public Users CreatedByUser { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public Users UpdatedByUser { get; set; }
    }
}
