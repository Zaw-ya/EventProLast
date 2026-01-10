using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EventPro.DAL.Models
{
    public partial class Users
    {
        public Users()
        {
            EventCategory = new HashSet<EventCategory>();
            EventGatekeeperMappingAssignedByNavigation = new HashSet<EventGatekeeperMapping>();
            EventGatekeeperMappingGatekeeper = new HashSet<EventGatekeeperMapping>();
            EventsCreatedByNavigation = new HashSet<Events>();
            EventsCreatedForNavigation = new HashSet<Events>();
            EventsModifiedByNavigation = new HashSet<Events>();
            GuestCreatedByNavigation = new HashSet<Guest>();
            GuestGateKeeperNavigation = new HashSet<Guest>();
            InverseCreatedByNavigation = new HashSet<Users>();
            InverseModifiedByNavigation = new HashSet<Users>();
            ScanHistory = new HashSet<ScanHistory>();
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        [MaxLength(20)]
        public string FirstName { get; set; }
        [MaxLength(20)]
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
        public string DeviceId { get; set; }
        public int? CityId { get; set; }
        public bool SendNotificationsOrEmails { get; set; } = false;

        [ForeignKey("CityId")]
        public virtual City City { get; set; }
        public virtual Users CreatedByNavigation { get; set; }
        public virtual Users ModifiedByNavigation { get; set; }
        public virtual Roles RoleNavigation { get; set; }
        public virtual ICollection<EventCategory> EventCategory { get; set; }
        public virtual ICollection<EventGatekeeperMapping> EventGatekeeperMappingAssignedByNavigation { get; set; }
        public virtual ICollection<EventGatekeeperMapping> EventGatekeeperMappingGatekeeper { get; set; }
        public virtual ICollection<Events> EventsCreatedByNavigation { get; set; }
        public virtual ICollection<Events> EventsCreatedForNavigation { get; set; }
        public virtual ICollection<Events> EventsModifiedByNavigation { get; set; }
        public virtual ICollection<Guest> GuestCreatedByNavigation { get; set; }
        public virtual ICollection<Guest> GuestGateKeeperNavigation { get; set; }
        public virtual ICollection<Users> InverseCreatedByNavigation { get; set; }
        public virtual ICollection<Users> InverseModifiedByNavigation { get; set; }
        public virtual ICollection<ScanHistory> ScanHistory { get; set; }
        public virtual ICollection<GKEventHistory> GKEventHistories { get; set; }
        public virtual ICollection<EventOperator> EventOperators { get; set; }
    }
}
