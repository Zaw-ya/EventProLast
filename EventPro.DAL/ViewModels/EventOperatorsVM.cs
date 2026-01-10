using EventPro.DAL.Models;

namespace EventPro.DAL.ViewModels
{
    public class EventOperatorsVM
    {
        public EventOperatorsVM(Users Operator)
        {
            Id = Operator.UserId;
            Name = string.Concat(Operator.FirstName, " ", Operator.LastName);
            UserName = Operator.UserName;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }


    }

}
