using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventPro.DAL.Common
{
    /// <summary>
    /// Firebase Integration service for Cloud Firestore and FCM push notifications.
    /// Manages CRUD operations on the Firestore "Events" collection and sends
    /// push notifications via the legacy HTTP-based FCM API.
    ///
    /// Firestore project ID is read from appsettings ("FireBaseProjId"):
    ///   - Production: myinvite-8fce8
    ///   - UAT:        myinvite-uat
    ///
    /// Firebase Console: https://console.firebase.google.com/u/0/project/myinvite-uat/firestore
    /// </summary>
    public class FirebaseIntegration
    {
        #region Fields & Constructor

        readonly FirestoreDb firestoreDb;
        const string collectionName = "Events";
        IConfiguration configuration;
        HttpClient client;

        /// <summary>
        /// Initializes the Firebase integration with Firestore DB, configuration, and HTTP client.
        /// </summary>
        /// <param name="firestoreDb">Firestore database instance created with the project ID from appsettings ("FireBaseProjId").</param>
        /// <param name="config">Application configuration to read Firebase settings.</param>
        /// <param name="client">HTTP client for legacy FCM API calls.</param>
        public FirebaseIntegration(FirestoreDb firestoreDb, IConfiguration config, HttpClient client)
        {
            this.firestoreDb = firestoreDb;
            configuration = config;
            this.client = client;
        }

        #endregion

        #region Firestore Operations

        /// <summary>
        /// Adds an event document to the Firestore "Events" collection.
        /// Converts the EF Core domain model to a Firestore document using <see cref="ConvertModelDocument"/>.
        /// </summary>
        /// <param name="_event">The event entity from SQL Server to sync to Firestore.</param>
        /// <returns>The created Firestore document.</returns>
        public async Task<EventDocument> AddEventTestAsync(Events _event)
        {
            var collection = firestoreDb.Collection(collectionName);
            var document = ConvertModelDocument.ConvertEventModelToDocument(_event);
            await collection.AddAsync(document);
            return document;
        }

        #endregion

        #region FCM Push Notifications (Legacy HTTP API)

        /// <summary>
        /// Sends a push notification to a city-based topic using the legacy HTTP FCM API.
        /// NOTE: This method uses a hardcoded legacy server key. Consider migrating to the
        /// Admin SDK approach used in <see cref="FirbaseAPI"/> which uses the V1 API.
        /// </summary>
        /// <param name="myEvent">The event to notify about.</param>
        /// <param name="cityTopic">The city topic name (will be lowercased).</param>
        /// <returns>The FCM response string.</returns>
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
            // Legacy FCM HTTP API approach (deprecated by Google)
            // TODO: Migrate to Admin SDK V1 API - use FirbaseAPI.NotifyTopicOrTokenAsync() instead
            var authKey = "AAAAAxAeYpQ:APA91bFQI7e3hkWDChFrNJZeo9umTdHdxYDgjpGs0K5P_MCVA3K0VuyWCqb7" +
                "BYNpDVVw5bB0iAg68UKxCXq3PQLE3MoNVbe-pJepZcKZc1nm1xWOBo-qP2YkjJp5ED9qumSSx2J0bMu5";
            var api = new AAM.Helpers.Common.FirebaseAPI(client, authKey);
            return await api.SendPushNotificationTopics(cityTopic, myEvent.EventTitle,
                $"New event is created! \nstarting in {myEvent.EventVenue} at {myEvent.AttendanceTime}");
        }

        #endregion
    }
}
