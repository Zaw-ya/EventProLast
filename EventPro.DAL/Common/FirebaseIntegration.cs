using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventPro.DAL.Common
{
    public class FirebaseIntegration
    {
        readonly FirestoreDb firestoreDb;
        const string collectionName = "Events";
        IConfiguration configuration;
        HttpClient client;

        public FirebaseIntegration(FirestoreDb firestoreDb, IConfiguration config, HttpClient client)
        {
            this.firestoreDb = firestoreDb;
            configuration = config;
            this.client = client;
        }

        public async Task<EventDocument> AddEventTestAsync(Events _event)
        {
            var collection = firestoreDb.Collection(collectionName);
            var document = ConvertModelDocument.ConvertEventModelToDocument(_event);
            await collection.AddAsync(document);
            return document;
        }

        public async Task<string> SendPushNotification(Events myEvent, string cityTopic)
        {
            cityTopic = cityTopic.ToLower();
            /*
            FirebaseApp app = null;
            try
            {
                app = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(configuration.GetSection("FireBaseJSON").Value)
                }, "EventPro");
            }
            catch
            {
                app = FirebaseApp.GetInstance("EventPro");
            }
            var fcm = FirebaseMessaging.GetMessaging(app);
            Message message = new Message()
            {
                Notification = new Notification
                {
                    Title = "New Event",
                    Body = "Event created for city " + cityTopic,
                },
                Data = new Dictionary<string, string>()
                 {
                    { "ffrom", myEvent.EventFrom.ToString() },
                    { "to", myEvent.EventTo.ToString() },
                    { "title", myEvent.EventTitle },
                    { "venue", myEvent.EventVenue },
                    { "parentTitle", myEvent.ParentTitle },
                    { "description", myEvent.EventDescription },
                 },

                Topic = cityTopic
            };

            return await fcm.SendAsync(message);
            */
            var authKey = "AAAAAxAeYpQ:APA91bFQI7e3hkWDChFrNJZeo9umTdHdxYDgjpGs0K5P_MCVA3K0VuyWCqb7" +
                "BYNpDVVw5bB0iAg68UKxCXq3PQLE3MoNVbe-pJepZcKZc1nm1xWOBo-qP2YkjJp5ED9qumSSx2J0bMu5";
            var api = new AAM.Helpers.Common.FirebaseAPI(client, authKey);
            return await api.SendPushNotificationTopics(cityTopic, myEvent.EventTitle,
                $"New event is created! \nstarting in {myEvent.EventVenue} at {myEvent.AttendanceTime}");
        }
    }
}
