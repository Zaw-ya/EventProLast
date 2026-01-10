using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class VwUsers
    {
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PrimaryContactNo { get; set; }
        public string SecondaryContantNo { get; set; }
        public string ModeOfCommunication { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public int? LoginAttempt { get; set; }
        public string TemporaryPass { get; set; }
        public bool? IsActive { get; set; }
        public bool? Approved { get; set; } = false;
        public DateTime? LockedOn { get; set; }
        public string PreferedTimeZone { get; set; }
        public int? Role { get; set; }
        public string BankAccountNo { get; set; }
        public string Ibnnumber { get; set; }
        public string BankName { get; set; }
    }
}
