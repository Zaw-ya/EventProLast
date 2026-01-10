using EventPro.DAL.Models;
using System.Collections.Generic;

namespace EventPro.DAL.ViewModels
{
    public class AvaliableGateKeepersVM
    {
        public AvaliableGateKeepersVM(Users gateKeeper, List<string> gateKeeperScheduled)
        {
            Id = gateKeeper.UserId;
            Name = string.Concat(gateKeeper.FirstName, " ", gateKeeper.LastName);
            AssignedEventsOnSameDay = GetAssignedEventsOnSameDay(gateKeeperScheduled);
            CountAssignedEventsOnSameDay = AssignedEventsOnSameDay.Count;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<AssignedEventsOnSameDay> AssignedEventsOnSameDay { get; set; } = new List<AssignedEventsOnSameDay>();
        public int CountAssignedEventsOnSameDay { get; set; }

        public List<AssignedEventsOnSameDay> GetAssignedEventsOnSameDay(List<string> gateKeeperScheduled)
        {
            List<AssignedEventsOnSameDay> result = new List<AssignedEventsOnSameDay>(); ;
            foreach (var gateKeeper in gateKeeperScheduled)
            {
                result.Add(new ViewModels.AssignedEventsOnSameDay
                {
                    EventTitle = gateKeeper
                });
            }
            return result;
        }

    }

}
