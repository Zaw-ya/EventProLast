using EventPro.DAL.Models;
using EventPro.DAL.Common;

namespace EventPro.DAL.ViewModels
{
    public class UserVM
    {
        public UserVM(Users user)
        {
            Id = user.UserId.ToString();
            FullName = user.FirstName + " " + user.LastName;
            Role = GetRoleName(user.Role);
            UserName = user.UserName;
            Email = user.Email;
            Status = user.IsActive;
            CreatedOn = user.CreatedOn?.ToLocalTime().ToShortDateString();
            Approved = user.Approved;
        }

        private string GetRoleName(int? roleId)
        {
            if (roleId == RoleIds.Administrator) return "Administrator";
            if (roleId == RoleIds.Client) return "Client";
            if (roleId == RoleIds.GateKeeper) return "GateKeeper";
            if (roleId == RoleIds.Operator) return "Operator";
            if (roleId == RoleIds.Agent) return "Agent";
            if (roleId == RoleIds.Supervisor) return "Supervisor";
            if (roleId == RoleIds.Accounting) return "Accounting";

            return "null";
        }

        public string Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string CreatedOn { get; set; }
        public bool? Status { get; set; }
        public bool? Approved { get; set; }
        public bool? IsActive { get; set; }




    }
}



