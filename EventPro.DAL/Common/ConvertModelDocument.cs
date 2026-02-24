using EventPro.DAL.Models;
using System;

namespace EventPro.DAL.Common
{
    /// <summary>
    /// Converts between EF Core domain models (Events) and Firestore documents (EventDocument).
    /// Used by <see cref="FirebaseIntegration"/> when syncing events to Cloud Firestore.
    /// </summary>
    public static class ConvertModelDocument
    {
        #region Firestore Document -> Domain Model

        /// <summary>
        /// Converts a Firestore <see cref="EventDocument"/> to an EF Core <see cref="Events"/> entity.
        /// </summary>
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

        #endregion

        #region Domain Model -> Firestore Document

        /// <summary>
        /// Converts an EF Core <see cref="Events"/> entity to a Firestore <see cref="EventDocument"/>.
        /// Stored in the Firestore "Events" collection.
        /// </summary>
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

        #endregion
    }
}
