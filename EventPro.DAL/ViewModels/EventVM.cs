using EventPro.DAL.Models;
using System;

namespace EventPro.DAL.ViewModels
{

    public class EventVM
    {
        public EventVM(VwEvents events)
        {
            Id = events.Id.ToString();
            Linked_To = getLinkedEvent(events.LinkedEvent);
            Title = getEventTitle(events.Icon, events.SystemEventTitle, events.Id.ToString());
            Start_Date = events.EventFrom.ToShortDateString();
            End_Date = events.EventTo.ToShortDateString();
            Venue = events.EventVenue?.ToString();
            Created_On = events.CreatedOn.ToLocalTime().ToShortDateString();
            Created_By = events.FirstName + " " + events.LastName;
            Status = GetEventStatus(events.EventTo, events.EventFrom);
            Deleted_On = events.DeletedOn?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "";
            Deleted_By = events.DeletedBy_FirstName + " " + events.DeletedBy_LastName;
        }

        public string Id { get; set; }
        public string Linked_To { get; set; }
        public string Title { get; set; }
        public string Start_Date { get; set; }
        public string End_Date { get; set; }
        public string Venue { get; set; }
        public string Created_On { get; set; }
        public string Created_By { get; set; }
        public string Deleted_On { get; set; }
        public string Deleted_By { get; set; }
        public string Status { get; set; }

        public string getLinkedEvent(long? linkedEvent)
        {
            if (linkedEvent == null)
            {
                return "---";

            }
            return linkedEvent.ToString();
        }

        public string getEventTitle(string? icon, string Title, string id)
        {
            if (icon != null)
            {

                return $"<a href=\"/admin/viewevent?id={id}\"> {Title} </a>";

            }
            return $"<a href=\"/admin/viewevent?id={id}\"> {Title} </a>";
        }

        public string GetEventStatus(DateTime? events_EventTo, DateTime? events_EventFrom)
        {
            if (events_EventTo < DateTime.Now)
            {
                return "past";
            }
            else if (events_EventTo >= DateTime.Now && events_EventFrom <= DateTime.Now)
            {
                return "in-progress";
            }
            else if (events_EventTo > DateTime.Now)
            {
                return "upcoming";
            }

            return "in-progress";

        }
    }


}
