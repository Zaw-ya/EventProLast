using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class VwEvents
    {
        public int Id { get; set; }
        public int? EventCode { get; set; }
        public string EventTitle { get; set; }
        public string SystemEventTitle { get; set; }
        public int? Type { get; set; }
        public DateTime EventFrom { get; set; }
        public DateTime EventTo { get; set; }
        public string EventVenue { get; set; }
        public string GmapCode { get; set; }
        public string Glocation { get; set; }
        public string EventDescription { get; set; }
        public int? CreatedBy { get; set; }
        public int? CreatedFor { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool? IsArchived { get; set; }
        public string Status { get; set; }
        public string IconUrl { get; set; }
        public string Icon { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Category { get; set; }
        public long? LinkedEvent { get; set; }

        public bool? IsDeleted { get; set; }

        public int? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }

        public string DeletedBy_FirstName { get; set; }
        public string DeletedBy_LastName { get; set; }



    }
}
