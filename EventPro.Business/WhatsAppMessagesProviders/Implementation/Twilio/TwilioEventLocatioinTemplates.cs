using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioEventLocatioinTemplates : TwilioMessagingConfiguration, IEventLocationMessageTemplates
    {
        private readonly EventProContext db;
        public TwilioEventLocatioinTemplates(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService) : base(configuration,
                memoryCacheStoreService)
        {
            db = new EventProContext(configuration);
        }

        public async Task SendArabicEventLocation(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = profileSettings?.ArabicEventLocation;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var location = "https://maps.app.goo.gl/" + events.GmapCode;
                var parameters = new string[]
                {
               location,
                };

                await SendMessageAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests,profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        private async Task SendMessageAndUpdateGuest(Events events, string? templateId, Guest guest, string fullPhoneNumber, string[] parameters, List<Guest> guests,TwilioProfileSettings profileSettings)
        {
            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.waMessageEventLocationForSendingToAll = messageSid;

                if (guests.Count > 1)
                {
                    _memoryCacheStoreService.save(messageSid, 0);
                }
            }
            guest.EventLocationSent = null;
            guest.EventLocationDelivered = null;
            guest.EventLocationRead = null;
            guest.EventLocationFailed = null;

            //await Task.Delay(300);
        }

        public async Task SendEnglishEventLocation(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                      .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                      .AsNoTracking()
                      .FirstOrDefaultAsync();
            var templateId = profileSettings?.EnglishEventLocation;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var location = "https://maps.app.goo.gl/" + events.GmapCode;
                var parameters = new string[]
                {
               location,
                };

                await SendMessageAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        private async Task updateDataBaseAndDisposeCache(List<Guest> guests, Events events)
        {
            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();
            if (guests.Count > 1)
            {
                await Task.Delay(10000);
                _memoryCacheStoreService.delete(events.Id.ToString());
                foreach (var guest in guests)
                {
                    if (guest.waMessageEventLocationForSendingToAll != null)
                    {
                        _memoryCacheStoreService.delete(guest.waMessageEventLocationForSendingToAll);
                    }
                }
            }
        }
    }
}
