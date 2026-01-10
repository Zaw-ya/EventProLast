using EventPro.DAL.Models;

namespace EventPro.DAL.ViewModels
{
    public class AssignedGateKeepersVM
    {
        public AssignedGateKeepersVM(VwEventGatekeeper gateKeeper)
        {
            Id = gateKeeper.UserId;
            Name = string.Concat(gateKeeper.FirstName, " ", gateKeeper.LastName);
            UserName = gateKeeper.UserName;
            IbnNo = gateKeeper.Ibnnumber ?? "";
            AccountNo = gateKeeper.BankAccountNo ?? "";
            TaskId = gateKeeper.TaskId ?? 0;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string IbnNo { get; set; }
        public string AccountNo { get; set; }
        public int TaskId { get; set; }

    }

}
