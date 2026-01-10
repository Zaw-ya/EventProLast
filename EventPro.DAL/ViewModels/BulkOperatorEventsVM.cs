using EventPro.DAL.Models;

namespace EventPro.DAL.ViewModels
{
    public class BulkOperatorEventsVM
    {
        public BulkOperatorEventsVM(BulkOperatorEvents model)
        {
            Id = model.Id;
            AssignedFromFullName = model.OperatorAssignedFrom.FirstName + " " + model.OperatorAssignedFrom.LastName;
            AssignedToFullName = model.OperatorAssignedTo.FirstName + " " + model.OperatorAssignedTo.LastName;
            AssignedBy = model.AssignedBy.FirstName + " " + model.AssignedBy.LastName;
            AssignedFromUserName = model.OperatorAssignedFrom.UserName;
            AssignedToUserName = model.OperatorAssignedTo.UserName;
            AssignedOn = model.AssignedOn.ToLocalTime().ToString();
        }

        public int Id { get; set; }
        public string AssignedFromFullName { get; set; }
        public string AssignedFromUserName { get; set; }
        public string AssignedToFullName { get; set; }
        public string AssignedToUserName { get; set; }
        public string AssignedOn { get; set; }
        public string AssignedBy { get; set; }
        public int ViewNumber { get; set; }

    }
}
