using Google.Cloud.Firestore;

namespace EventPro.DAL.Models
{
    /// <summary>
    /// Firestore document model for the "Events" collection.
    /// Maps to Cloud Firestore using Google.Cloud.Firestore attributes.
    /// Converted to/from EF Core Events entity via <see cref="Common.ConvertModelDocument"/>.
    ///
    /// Firestore Console: https://console.firebase.google.com/u/0/project/myinvite-uat/firestore
    /// </summary>
    [FirestoreData]
    public class EventDocument
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string EventCode { get; set; }

        [FirestoreProperty]
        public string EventTitle { get; set; }

        [FirestoreProperty]
        public string EventFrom { get; set; }

        [FirestoreProperty]
        public string EventTo { get; set; }

        [FirestoreProperty]
        public string EventVenue { get; set; }

        [FirestoreProperty]
        public string EventDescription { get; set; }

        [FirestoreProperty]
        public string ParentTitle { get; set; }
    }
}
