using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioDeclineMessageTemplates : TwilioMessagingConfiguration, IDeclineMessageTemplates
    {
        private readonly EventProContext db;
        public TwilioDeclineMessageTemplates(
            IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService,
            ILogger<TwilioMessagingConfiguration> logger) 
            : base(configuration, memoryCacheStoreService, logger)
        {
            db = new EventProContext(configuration);
        }
        public async Task SendCustomDeclineTemplate(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                   .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                   .AsNoTracking()
                   .FirstOrDefaultAsync();
            var templateId = events.DeclineTempId;
            if (string.IsNullOrEmpty(templateId))
                throw new InvalidOperationException("Decline template ID is not configured on this event. Please set the Decline Template ID in the event settings.");

            foreach (var guest in guests)
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string[] parameters;
                if (events.SendingType == "Basic")
                {
                    parameters = new string[] { };
                }
                else
                {
                    parameters = new string[] { guest.FirstName.Trim(), };
                }
                var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);
            }
            return;
        }

        public async Task SendDeclineTemplate(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                               .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                               .AsNoTracking()
                               .FirstOrDefaultAsync();
            var templateId = profileSettings?.ArabicDecline;
            if (string.IsNullOrEmpty(templateId))
                throw new InvalidOperationException("Decline template ID is not configured in the Twilio profile. Please open the profile settings and fill in the required template ID.");

            foreach (Guest guest in guests)
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var parameters = new string[] { };
                var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);
            }
            return;
        }
    }
}
