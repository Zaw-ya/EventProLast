using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioDuplicateMessageTemplates : TwilioMessagingConfiguration, IDuplicateMessageTemplates
    {
        private readonly EventProContext db;
        public TwilioDuplicateMessageTemplates(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService) : base(configuration,
                memoryCacheStoreService)
        {
            db = new EventProContext(configuration);
        }
        public async Task SendArabicDuplicateAnswer(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                   .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                   .AsNoTracking()
                   .FirstOrDefaultAsync();
            var templateId = profileSettings?.ArabicDuplicateAnswer;
            await SendDuplicateAnswerMessage(guests, events, templateId,profileSettings);
            return;
        }

        public async Task SendEnglishDuplicateAnswer(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = profileSettings?.EnglishDuplicateAnswer;

            await SendDuplicateAnswerMessage(guests, events, templateId,profileSettings);
            return;
        }

        private async Task SendDuplicateAnswerMessage(List<Guest> guests, Events events, string? templateId,TwilioProfileSettings profileSettings)
        {
            foreach (var guest in guests)
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var parameters = new string[]
                {
                guest.FirstName.Trim(),
                };

                await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);
            }
        }
    }
}
