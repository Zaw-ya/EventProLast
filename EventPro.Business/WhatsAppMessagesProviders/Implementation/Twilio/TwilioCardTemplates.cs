using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using System.Text.RegularExpressions;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioCardTemplates : TwilioMessagingConfiguration, ICardMessageTemplates
    {
        private readonly EventProContext db;
        public TwilioCardTemplates(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService) : base(configuration,
                memoryCacheStoreService)
        {
            db = new EventProContext(configuration);

        }
        public async Task SendArabicCard(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = profileSettings?.ArabicCardWithoutGuestName;

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                var parameters = new string[]
                {
                imagePathSegment.ToString(),
                };

                await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }



        public async Task SendArabicCardwithname(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = profileSettings?.ArabicCardWithGuestName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                var parameters = new string[]
                {
                imagePathSegment.ToString(),
                guest.FirstName.Trim(),
                };

                await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCardByIDBasic(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomCardInvitationTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                var parameters = new string[]
                {
               imagePathSegment.ToString(),
                };

                await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCardByIDWithGusetName(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = events.CustomCardInvitationTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                var parameters = new string[]
                {
               imagePathSegment.ToString(),
               guest.FirstName.Trim(),
                };

                await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendEnglishCard(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = profileSettings?.EnglihsCardWithoutGuestName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                var parameters = new string[]
                {
               imagePathSegment.ToString(),
                };

                await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendEnglishCardwithname(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = profileSettings?.EnglishCardWithGuestName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                var parameters = new string[]
                {
                imagePathSegment.ToString(),
                guest.FirstName.Trim(),
                };

                await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);
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
                    if (guest.ImgSentMsgId != null)
                    {
                        _memoryCacheStoreService.delete(guest.ImgSentMsgId);
                    }
                }
            }
        }

        private async Task SendCardAndUpdateGuest(Events events, string? templateId, Guest guest, string fullPhoneNumber, string[] parameters,List<Guest> guests, TwilioProfileSettings profileSettings)
        {
            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.ImgSentMsgId = messageSid;
                guest.ImgSenOn = DateTime.Now.ToString();
                guest.Qrresponse = "Message Processed Successfully";
                guest.ImgDelivered = null;
                guest.ImgFailed = null;
                guest.ImgSent = null;
                guest.ImgRead = null;
                guest.WhatsappStatus = "sent";

                if (guests.Count > 1)
                {
                    _memoryCacheStoreService.save(messageSid, 0);
                }

            }
            else
            {
                guest.WhatsappStatus = "error";
                guest.ImgSentMsgId = null;
                guest.ImgSenOn = null;
                guest.ImgDelivered = null;
                guest.ImgFailed = null;
                guest.ImgSent = null;
                guest.ImgRead = null;
            }

            //await Task.Delay(300);
        }

        public async Task SendCustomTemplateWithVariables(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                            .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync();
            var templateId = events.CustomCardInvitationTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var matches = Regex.Matches(events.CustomCardTemplateWithVariables, @"\{\{(.*?)\}\}");
                List<string> templateParameters = matches
                    .Cast<Match>()
                    .Select(m =>
                    {
                        string propName = m.Groups[1].Value;
                        if (propName == "GuestCard")
                        {
                            return events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                        }

                        if (propName == "CountOfAdditionalInvitations")
                        {
                            return (guest.NoOfMembers - 1)?.ToString() ?? "0";
                        }
                        var value = guest.GetType().GetProperty(propName)?
                                          .GetValue(guest, null)?.ToString();
                        if (value == null)
                        {
                            value = events.GetType().GetProperty(propName)?
                                          .GetValue(events, null)?.ToString();
                        }
                        return value ?? propName;
                    })
                    .ToList();

                string[] parameters = templateParameters.ToArray();

                await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
    }
}
