using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using Newtonsoft.Json;
using RestSharp;
using System.Text;
using Twilio;
using Microsoft.Extensions.Logging;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    /// <summary>
    /// Abstract base class that provides core Twilio WhatsApp messaging configuration and functionality.
    /// This class serves as the foundation for all Twilio-based WhatsApp messaging implementations,
    /// handling authentication, phone number selection based on country, and message sending via Twilio's API.
    /// </summary>
    public abstract class TwilioMessagingConfiguration : IProviderMessagingConfiguration
    {

        protected readonly IConfiguration _configuration;
        protected IConfiguration CountrySendingProfileConfig;
        protected IConfiguration choosenSendingWhatsappProfileSettingsConfig;
        protected IMemoryCacheStoreService _memoryCacheStoreService;
        protected ParallelOptions parallelOptions;
        private readonly EventProContext db;
        private readonly ILogger<TwilioMessagingConfiguration> _logger;

        /// <summary>
        /// Constructor that initializes the Twilio messaging configuration.
        /// Sets up the database context, memory cache service, and parallel processing options.
        /// </summary>
        /// <param name="configuration">Application configuration for accessing settings</param>
        /// <param name="memoryCacheStoreService">Cache service for storing sending progress counters</param>
        public TwilioMessagingConfiguration(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService,
            ILogger<TwilioMessagingConfiguration> logger)
        {
            _configuration = configuration;
            _memoryCacheStoreService = memoryCacheStoreService;
            db = new EventProContext(configuration);
            parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4 // number of cpu used
            };
            _logger = logger;
        }

        /// <summary>
        /// Sends a WhatsApp template message via Twilio's REST API.
        ///
        /// This method performs the following steps:
        /// 1. Extracts Twilio credentials (AccountSid, AuthToken) from the profile settings
        /// 2. Selects the appropriate sender phone number based on:
        ///    - ChoosenNumberWithinCountry: Chooses between number 1 or 2 for each country
        ///    - choosenCountryNumber: Determines which country's number to use (BAHRAIN, KUWAIT, or Saudi as default)
        /// 3. Converts the parameters array into a JSON dictionary format required by Twilio
        ///    Example: ["Ahmed", "Event123"] becomes {"1": "Ahmed", "2": "Event123"}
        /// 4. Makes a POST request to Twilio's Messages API with the template content
        /// 5. Returns the message SID (unique identifier) if successful, or null if failed
        /// </summary>
        /// <param name="toPhoneNumber">Recipient's phone number (will be prefixed with 'whatsapp:')</param>
        /// <param name="contentSid">Twilio Content Template SID - identifies which pre-approved template to use</param>
        /// <param name="parameters">Dynamic values to substitute into the template placeholders</param>
        /// <param name="cityId">Optional city identifier (currently unused in this method)</param>
        /// <param name="ChoosenNumberWithinCountry">1 or 2 - selects between the two available numbers per country</param>
        /// <param name="profileSettings">Twilio profile containing all credentials and phone numbers</param>
        /// <param name="choosenCountryNumber">Country code string: "BAHRAIN", "KUWAIT", or defaults to Saudi</param>
        /// <returns>Message SID string if successful, null if an exception occurs</returns>
        protected async Task<string> SendWhatsAppTemplateMessageAsync(string toPhoneNumber, string contentSid, object[] parameters, int? cityId, int ChoosenNumberWithinCountry, TwilioProfileSettings profileSettings , string choosenCountryNumber)
        {
            var sendingProfile = profileSettings;

            try
            {
                string fromWhatsAppNumberSaudiChoosen = string.Empty;
                string fromWhatsAppNumberKuwaitChoosen = string.Empty;
                string FromWhatsAppNumberBahrainChoosen = string.Empty;
                string fromWhatsAppNumberEgyptChoosen = string.Empty;
                string ChoosenSendingNumber = string.Empty;
                var accountSid = sendingProfile?.AccountSid;
                var authToken = sendingProfile?.AuthToken;
                var fromWhatsAppNumberSaudi1 = sendingProfile?.WhatsAppNumberSaudi1;
                var fromWhatsAppNumberSaudi2 = sendingProfile?.WhatsAppNumberSaudi2;
                var fromWhatsAppNumberKuwait1 = sendingProfile?.WhatsAppNumberKuwait1;
                var fromWhatsAppNumberKuwait2 = sendingProfile?.WhatsAppNumberKuwait2;
                var FromWhatsAppNumberBahrain1 = sendingProfile?.WhatsAppNumberBahrain1;
                var FromWhatsAppNumberBahrain2 = sendingProfile?.WhatsAppNumberBahrain2;
                var fromWhatsAppNumberEgypt1 = sendingProfile?.WhatsAppNumberEgypt1;
                var fromWhatsAppNumberEgypt2 = sendingProfile?.WhatsAppNumberEgypt2;
                var messagingServiceSid = sendingProfile?.MessagingServiceSid;

                if (ChoosenNumberWithinCountry == 2)
                {
                    fromWhatsAppNumberSaudiChoosen = fromWhatsAppNumberSaudi2;
                    fromWhatsAppNumberKuwaitChoosen = fromWhatsAppNumberKuwait2;
                    FromWhatsAppNumberBahrainChoosen = FromWhatsAppNumberBahrain2;
                    fromWhatsAppNumberEgyptChoosen = fromWhatsAppNumberEgypt2;
                }
                else
                {
                    // Which nubmer the primary or secondary number to use
                    fromWhatsAppNumberSaudiChoosen = fromWhatsAppNumberSaudi1;
                    fromWhatsAppNumberKuwaitChoosen = fromWhatsAppNumberKuwait1;
                    FromWhatsAppNumberBahrainChoosen = FromWhatsAppNumberBahrain1;
                    fromWhatsAppNumberEgyptChoosen = fromWhatsAppNumberEgypt1;
                }

                if (choosenCountryNumber == "BAHRAIN")
                {
                    ChoosenSendingNumber = FromWhatsAppNumberBahrainChoosen;
                }
                else if (choosenCountryNumber == "KUWAIT")
                {
                    ChoosenSendingNumber = fromWhatsAppNumberKuwaitChoosen;
                }
                else if (choosenCountryNumber == "EGYPT")
                {
                    ChoosenSendingNumber = fromWhatsAppNumberEgyptChoosen;
                }
                else
                {
                    ChoosenSendingNumber = fromWhatsAppNumberSaudiChoosen;
                }

                TwilioClient.Init(accountSid, authToken);

                // Create the content variables dictionary
                var contentVariables = parameters.Select((p, index) => new KeyValuePair<string, string>((index + 1).ToString(), p.ToString()))
                                                 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // Serialize the content variables dictionary to JSON
                var contentVariablesJson = JsonConvert.SerializeObject(contentVariables);

                _logger.LogInformation(
                    "Sending WhatsApp template. To: {To}, From: {From}, TemplateId: {TemplateId}",
                    toPhoneNumber,
                    ChoosenSendingNumber,
                    contentSid
                );

                // Initialize RestClient and RestRequest
                var client = new RestClient($"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json");
                var request = new RestRequest("", Method.Post)
                    .AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}")))
                    .AddHeader("Content-Type", "application/x-www-form-urlencoded");

                // Add parameters to the request
                request.AddParameter("ContentSid", contentSid);
                request.AddParameter("To", $"whatsapp:{toPhoneNumber}");
                //============================================================================================================================================
                request.AddParameter("From", $"whatsapp:{ChoosenSendingNumber}");
                //=============================================================================================================================================
                request.AddParameter("ContentVariables", contentVariablesJson);
                request.AddParameter("MessagingServiceSid", messagingServiceSid);
                //request.AddParameter("StatusCallback", statusCallbackUrl);

                // Execute the request
                var response = await client.ExecuteAsync(request);

                var logResponse = response.Content.ToString();

                dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content);
                string messageSid = jsonResponse?.sid;

                if (!string.IsNullOrEmpty(messageSid))
                {
                    _logger.LogInformation("Message sent successfully. To: {To}, MessageSid: {Sid}", toPhoneNumber, messageSid);
                }
                else
                {
                    _logger.LogWarning("Failed to send message. To: {To}, TemplateId: {TemplateId}", toPhoneNumber, contentSid);
                }

                return messageSid;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending WhatsApp template. To: {To}, TemplateId: {TemplateId}", toPhoneNumber, contentSid);
                return null;
            }
        }


        /// <summary>
        /// Initializes the Twilio client with credentials from a named profile stored in the database.
        ///
        /// This method:
        /// 1. Queries the TwilioProfileSettings table to find a profile matching the given name
        /// 2. Extracts the AccountSid and AuthToken from that profile
        /// 3. Initializes the global TwilioClient with these credentials
        ///
        /// Use this when you need to switch between different Twilio accounts/profiles
        /// (e.g., different accounts for different clients or events).
        /// </summary>
        /// <param name="choosenSendingWhatsappProfile">The name of the Twilio profile to load from database</param>
        protected async Task SetTwilioAccountConfigurationAsync(string choosenSendingWhatsappProfile)
        {
            var twilioProfile = await db.TwilioProfileSettings.Where(e => e.Name == choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var accountSid = twilioProfile?.AccountSid;
            var authToken = twilioProfile?.AuthToken;
            TwilioClient.Init(accountSid, authToken);
        }

        /// <summary>
        /// Increments and persists the message sending counter for bulk operations.
        ///
        /// This method is used during batch WhatsApp message sending to track progress.
        /// The counter is stored in memory cache using the event ID as the key,
        /// allowing the UI to display real-time progress (e.g., "Sending 5 of 100 messages").
        ///
        /// Only increments when there are multiple guests (bulk send scenario).
        /// For single guest sends, the counter remains unchanged.
        /// </summary>
        /// <param name="guests">List of guests being messaged - counter only updates if count > 1</param>
        /// <param name="events">The event associated with this sending operation (used as cache key)</param>
        /// <param name="counter">Current counter value to increment</param>
        /// <returns>The incremented counter value (or unchanged if single guest)</returns>
        protected int UpdateCounter(List<Guest> guests, Events events, int counter)
        {
            if (guests.Count > 1)
            {
                counter++;
                _memoryCacheStoreService.save(events.Id.ToString(), counter);
            }
            _logger.LogInformation("Updated sending counter for Event ID: {EventId} to {Counter}", events.Id, counter);
            return counter;
        }

        /// <summary>
        /// Initializes the message sending counter to zero at the start of a bulk send operation.
        ///
        /// Call this method before starting to send messages to multiple guests.
        /// It resets the progress counter in memory cache to 0, preparing for
        /// the UpdateCounter method to track progress as each message is sent.
        ///
        /// The counter is stored with the event ID as key, so each event's
        /// sending progress is tracked independently.
        /// </summary>
        /// <param name="guests">List of guests to be messaged - counter only initializes if count > 1</param>
        /// <param name="events">The event associated with this sending operation (used as cache key)</param>
        /// <returns>Initial counter value (always 0)</returns>
        protected int SetSendingCounter(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Initializing sending counter for Event ID: {EventId} with {GuestCount} guests", events.Id, guests.Count);
            int counter = 0;
            if (guests.Count > 1)
            {
                _memoryCacheStoreService.save(events.Id.ToString(), 0);
                _logger.LogInformation("Sending counter set to 0 for Event ID: {EventId}", events.Id);
            }
            _logger.LogInformation("Sending counter initialization complete for Event ID: {EventId}", events.Id);
            return counter;
        }

        /// <summary>
        /// Constructs a full Cloudinary URL for a given relative path and folder.
        /// </summary>
        /// <param name="relativePath">The relative path of the image (e.g., "123/image.jpg")</param>
        /// <param name="folderName">The configuration key for the folder (e.g., "Card")</param>
        /// <returns>Full absolute URL string</returns>
        protected string GetFullImageUrl(string relativePath, string folderNameKey)
        {

            var cloudName = _configuration.GetSection("CloudinarySettings")["CloudName"];
            // Retrieve the folder path from configuration, e.g., Uploads:Card -> "/card"
            // We strip the leading slash if present to avoid double slashes when combining
            var folderPath = _configuration.GetSection("Uploads")[folderNameKey];
            
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = folderNameKey; // Fallback if key is not found, assume it's the folder name itself
            }
            
            // Normalize folder path: remove leading slash
            if (folderPath.StartsWith("/")) folderPath = folderPath.Substring(1);
            
            // Cloudinary URL format: https://res.cloudinary.com/{cloud_name}/image/upload/{folder}/{file}
            return $"https://res.cloudinary.com/{cloudName}/image/upload/{folderPath}/{relativePath}";
        }
    }
}
