using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class VwGuestList
    {
        public int GuestId { get; set; }
        public string FirstName { get; set; }
        public string PrimarycontactNo { get; set; }
        public string EventTitle { get; set; }
        public DateTime? EventFrom { get; set; }
        public DateTime? EventTo { get; set; }
    }
}
