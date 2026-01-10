using Google.Cloud.Firestore;

namespace EventPro.DAL.Models
{
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
