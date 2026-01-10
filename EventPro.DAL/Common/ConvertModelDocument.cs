using EventPro.DAL.Models;
using System;

namespace EventPro.DAL.Common
{
    public static class ConvertModelDocument
    {
        public static Events ConvertEventDocumentToModel(EventDocument document)
        {
            return new Events
            {
                Id = int.Parse(document.Id),
                EventCode = int.Parse(document.EventCode),
                EventTitle = document.EventTitle,
                EventDescription = document.EventDescription,
                ParentTitle = document.ParentTitle,
                EventVenue = document.EventVenue,
                EventFrom = DateTime.Parse(document.EventFrom),
                EventTo = DateTime.Parse(document.EventTo)
            };
        }

        public static EventDocument ConvertEventModelToDocument(Events _event)
        {
            return new EventDocument
            {
                Id = _event.Id.ToString(),
                EventCode = _event.EventCode.ToString(),
                EventTitle = _event.EventTitle,
                EventDescription = _event.EventDescription,
                ParentTitle = _event.ParentTitle,
                EventVenue = _event.EventVenue,
                EventFrom = _event.EventFrom.ToString(),
                EventTo = _event.EventTo.ToString()
            };
        }
    }
}
