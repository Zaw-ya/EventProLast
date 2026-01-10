using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using Newtonsoft.Json;
using RestSharp;
using System.Text;
using Twilio;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public abstract class TwilioMessagingConfiguration : IProviderMessagingConfiguration
    {

        protected readonly IConfiguration _configuration;
        protected IConfiguration CountrySendingProfileConfig;
        protected IConfiguration choosenSendingWhatsappProfileSettingsConfig;
        protected IMemoryCacheStoreService _memoryCacheStoreService;
        protected ParallelOptions parallelOptions;
        private readonly EventProContext db;
        public TwilioMessagingConfiguration(IConfiguration configuration ,
            IMemoryCacheStoreService memoryCacheStoreService)
        {
            _configuration = configuration;
            _memoryCacheStoreService = memoryCacheStoreService;
            db = new EventProContext(configuration);
            parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4 // number of cpu used
            };
        }

        protected async Task<string> SendWhatsAppTemplateMessageAsync(string toPhoneNumber, string contentSid, object[] parameters, int? cityId, int ChoosenNumberWithinCountry, TwilioProfileSettings profileSettings , string choosenCountryNumber)
        {
            var sendingProfile = profileSettings;

            try
            {
                string fromWhatsAppNumberSaudiChoosen = string.Empty;
                string fromWhatsAppNumberKuwaitChoosen = string.Empty;
                string FromWhatsAppNumberBahrainChoosen = string.Empty;
                string ChoosenSendingNumber = string.Empty;
                var accountSid = sendingProfile?.AccountSid;  
                var authToken = sendingProfile?.AuthToken; 
                var fromWhatsAppNumberSaudi1 = sendingProfile?.WhatsAppNumberSaudi1; 
                var fromWhatsAppNumberSaudi2 = sendingProfile?.WhatsAppNumberSaudi2; 
                var fromWhatsAppNumberKuwait1 = sendingProfile?.WhatsAppNumberKuwait1; 
                var fromWhatsAppNumberKuwait2 = sendingProfile?.WhatsAppNumberKuwait2; 
                var FromWhatsAppNumberBahrain1 = sendingProfile?.WhatsAppNumberBahrain1; 
                var FromWhatsAppNumberBahrain2 = sendingProfile?.WhatsAppNumberBahrain2; 
                var messagingServiceSid = sendingProfile?.MessagingServiceSid; 

                if (ChoosenNumberWithinCountry == 2)
                {
                    fromWhatsAppNumberSaudiChoosen = fromWhatsAppNumberSaudi2;
                    fromWhatsAppNumberKuwaitChoosen = fromWhatsAppNumberKuwait2;
                    FromWhatsAppNumberBahrainChoosen = FromWhatsAppNumberBahrain2;
                }
                else
                {
                    fromWhatsAppNumberSaudiChoosen = fromWhatsAppNumberSaudi1;
                    fromWhatsAppNumberKuwaitChoosen = fromWhatsAppNumberKuwait1;
                    FromWhatsAppNumberBahrainChoosen = FromWhatsAppNumberBahrain1;
                }

                if (choosenCountryNumber == "BAHRAIN")
                {
                    ChoosenSendingNumber = FromWhatsAppNumberBahrainChoosen;
                }
                else if (choosenCountryNumber == "KUWAIT")
                {
                    ChoosenSendingNumber = fromWhatsAppNumberKuwaitChoosen;
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
                return jsonResponse.sid;

            }
            catch (Exception ex)
            {
                return null;
            }
        }


        protected async Task SetTwilioAccountConfigurationAsync(string choosenSendingWhatsappProfile)
        {
            var twilioProfile = await db.TwilioProfileSettings.Where(e => e.Name == choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var accountSid = twilioProfile?.AccountSid;
            var authToken = twilioProfile?.AuthToken;
            TwilioClient.Init(accountSid, authToken);
        }

        protected int UpdateCounter(List<Guest> guests, Events events, int counter)
        {
            if (guests.Count > 1)
            {
                counter++;
                _memoryCacheStoreService.save(events.Id.ToString(), counter);
            }

            return counter;
        }

        protected int SetSendingCounter(List<Guest> guests, Events events)
        {
            int counter = 0;
            if (guests.Count > 1)
            {
                _memoryCacheStoreService.save(events.Id.ToString(), 0);
            }

            return counter;
        }

    }
}
