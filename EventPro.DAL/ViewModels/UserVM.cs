using EventPro.DAL.Models;

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
            if (roleId == 1) return "Administrator";
            if (roleId == 2) return "Client";
            if (roleId == 3) return "GateKeeper";
            if (roleId == 4) return "Operator";
            if (roleId == 5) return "Agent";
            if (roleId == 6) return "Supervisor";
            if (roleId == 7) return "Accounting";

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



