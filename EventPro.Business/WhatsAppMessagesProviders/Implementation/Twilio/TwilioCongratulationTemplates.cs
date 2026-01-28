using System.Text.RegularExpressions;

using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioCongratulationTemplates : TwilioMessagingConfiguration, ICongratulationsMessageTemplates
    {
        private readonly EventProContext db;
        public TwilioCongratulationTemplates(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService, ILogger<TwilioMessagingConfiguration> logger) : base(configuration,
                memoryCacheStoreService, logger)
        {
            db = new EventProContext(configuration);
        }

        public async Task SendCongratulationMessageToOwner(List<Guest> guests, Events events, string message)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = await db.TwilioProfileSettings
                             .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                             .AsNoTracking()
                             .Select(e => e.ArabicCongratulationMessageToEventOwner)
                             .FirstOrDefaultAsync();
            await SendMessageToOwnerAndUpdateGuest(guests, events, message, templateId);
            return;
        }

        public async Task SendCongratulationMessageToOwnerEnglish(List<Guest> guests, Events events, string message)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = await db.TwilioProfileSettings
                             .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                             .AsNoTracking()
                             .Select(e => e.EnglishCongratulationMessageToEventOwner)
                             .FirstOrDefaultAsync();
            await SendMessageToOwnerAndUpdateGuest(guests, events, message, templateId);
            return;
        }

        public async Task SendTemp1(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp1;
            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp10(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp10;
            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp2(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp2;
            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp3(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp3;
            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp4(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp4;
            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp5(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp5;
            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp6(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp6;
            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp7(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp7;
            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp8(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp8;
            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp9(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp9;
            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendThanksById(List<Guest> guests, Events events)
        {
            var twilioProfile = await db.TwilioProfileSettings
                    .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var conguratulationId = Guid.NewGuid().ToString();
                var templateId = events.ThanksTempId;
                string[] parameters;
                if (events.SendingType == "Basic")
                {
                    parameters = new string[] { conguratulationId, };

                }
                else
                {
                    parameters = new string[] { guest.FirstName.Trim(), conguratulationId, };
                }
                await SendCustomMessageAndUpdateGuest(events, guest, fullPhoneNumber, conguratulationId, templateId, parameters, twilioProfile);
            });
            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();
            return;
        }
        public async Task SendThanksCustom(List<Guest> guests, Events events)
        {
            int counter = SetSendingCounter(guests, events);
            var twilioProfile = await db.TwilioProfileSettings
                                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var conguratulationId = Guid.NewGuid().ToString();
                var templateId = "";
                string[] parameters;
                if (events.SendingType == "Basic")
                {
                    templateId = twilioProfile?.CustomThanksWithoutGuestName;
                    parameters = new string[] { events.ThanksMessage, conguratulationId, };

                }
                else
                {
                    templateId = twilioProfile?.CustomThanksWithGuestName;
                    parameters = new string[] { guest.FirstName.Trim(), events.ThanksMessage, conguratulationId, };
                }
                await SendCustomMessageAndUpdateGuest(events, guest, fullPhoneNumber, conguratulationId, templateId, parameters, twilioProfile);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        private async Task SendCustomMessageAndUpdateGuest(Events events, Guest guest, string fullPhoneNumber, string conguratulationId, string? templateId, string[] parameters, TwilioProfileSettings profileSettings)
        {
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
                guest.ConguratulationMsgSent = null;
                guest.ConguratulationMsgRead = null;
                guest.ConguratulationMsgDelivered = null;
                guest.ConguratulationMsgFailed = null;

                _memoryCacheStoreService.save(messageSid, 0);
            }

            //await Task.Delay(1000);
        }
        private async Task SendDefaultMessageToGuestAndUpdateGuest(List<Guest> guests, Events events, string? templateId, TwilioProfileSettings profileSettings)
        {
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var conguratulationId = Guid.NewGuid().ToString();
                var parameters = new string[]
                {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,

                };

                var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);
                if (messageSid != null)
                {
                    guest.ConguratulationMsgId = messageSid;
                    guest.ConguratulationMsgLinkId = conguratulationId;
                    guest.ConguratulationMsgCount = 1;
                    guest.ConguratulationMsgSent = null;
                    guest.ConguratulationMsgRead = null;
                    guest.ConguratulationMsgDelivered = null;
                    guest.ConguratulationMsgFailed = null;

                    if (guests.Count > 1)
                    {
                        _memoryCacheStoreService.save(messageSid, 0);
                    }
                }
                counter = UpdateCounter(guests, events, counter);
                //await Task.Delay(300);
            });
            await updateDataBaseAndDisposeCache(guests, events);
        }

        private async Task SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(List<Guest> guests, Events events, string? templateId, TwilioProfileSettings profileSettings)
        {
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var conguratulationId = Guid.NewGuid().ToString();
                var parameters = new string[]
                {
                events.CongratulationMsgHeaderImg,
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,

                };

                var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);
                if (messageSid != null)
                {
                    guest.ConguratulationMsgId = messageSid;
                    guest.ConguratulationMsgLinkId = conguratulationId;
                    guest.ConguratulationMsgCount = 1;
                    guest.ConguratulationMsgSent = null;
                    guest.ConguratulationMsgRead = null;
                    guest.ConguratulationMsgDelivered = null;
                    guest.ConguratulationMsgFailed = null;

                    if (guests.Count > 1)
                    {
                        _memoryCacheStoreService.save(messageSid, 0);
                    }
                }
                counter = UpdateCounter(guests, events, counter);
                //await Task.Delay(300);
            });
            await updateDataBaseAndDisposeCache(guests, events);
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
                    if (guest.ConguratulationMsgId != null)
                    {
                        _memoryCacheStoreService.delete(guest.ConguratulationMsgId);
                    }
                }
            }
        }

        private async Task SendMessageToOwnerAndUpdateGuest(List<Guest> guests, Events events, string message, string? templateId)
        {
            var twilioProfile = await db.TwilioProfileSettings
                    .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            int counter = SetSendingCounter(guests, events);
            foreach (var guest in guests)
            {
                var eventSentOnNum = events.ConguratulationsMsgSentOnNumber;
                var detectTheEvet = events;
                string fullPhoneNumber = $"+{eventSentOnNum}";
                var parameters = new string[]
                {
                guest.FirstName.Trim(),
                message,
                };
                string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, detectTheEvet.CityId, 1, twilioProfile, events.choosenSendingCountryNumber);
                if (messageSid != null)
                {
                    guest.ConguratulationMsgCount = 0;
                }
                else
                {
                    throw new Exception();
                }
                counter = UpdateCounter(guests, events, counter);
            }
            await updateDataBaseAndDisposeCache(guests, events);
        }

        public async Task SendThanksCustomWithHeaderImage(List<Guest> guests, Events events)
        {
            int counter = SetSendingCounter(guests, events);
            var twilioProfile = await db.TwilioProfileSettings
                                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var conguratulationId = Guid.NewGuid().ToString();
                var templateId = "";
                string[] parameters;
                if (events.SendingType == "Basic")
                {
                    templateId = twilioProfile?.CustomThanksWithoutGuestNameWithHeaderImage;
                    parameters = new string[] { events.CongratulationMsgHeaderImg, events.ThanksMessage, conguratulationId, };

                }
                else
                {
                    templateId = twilioProfile?.CustomThanksWithGuestNameWithHeaderImage;
                    parameters = new string[] { events.CongratulationMsgHeaderImg, guest.FirstName.Trim(), events.ThanksMessage, conguratulationId, };
                }
                await SendCustomMessageAndUpdateGuest(events, guest, fullPhoneNumber, conguratulationId, templateId, parameters, twilioProfile);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendTemp1WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                      .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                      .AsNoTracking()
                      .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp1WithHeaderImage;
            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp2WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
           .Where(e => e.Name == events.choosenSendingWhatsappProfile)
           .AsNoTracking()
           .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp2WithHeaderImage;
            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp3WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
           .Where(e => e.Name == events.choosenSendingWhatsappProfile)
           .AsNoTracking()
           .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp3WithHeaderImage;
            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp4WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
          .Where(e => e.Name == events.choosenSendingWhatsappProfile)
          .AsNoTracking()
          .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp4WithHeaderImage;
            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp5WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
          .Where(e => e.Name == events.choosenSendingWhatsappProfile)
          .AsNoTracking()
          .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp5WithHeaderImage;
            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp6WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
          .Where(e => e.Name == events.choosenSendingWhatsappProfile)
          .AsNoTracking()
          .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp6WithHeaderImage;
            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp7WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
           .Where(e => e.Name == events.choosenSendingWhatsappProfile)
           .AsNoTracking()
           .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp7WithHeaderImage;
            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp8WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
            .Where(e => e.Name == events.choosenSendingWhatsappProfile)
            .AsNoTracking()
            .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp8WithHeaderImage;
            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp9WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
           .Where(e => e.Name == events.choosenSendingWhatsappProfile)
           .AsNoTracking()
           .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp9WithHeaderImage;
            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendTemp10WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
           .Where(e => e.Name == events.choosenSendingWhatsappProfile)
           .AsNoTracking()
           .FirstOrDefaultAsync();
            var templateId = profileSettings?.ThanksTemp10WithHeaderImage;
            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);
            return;
        }

        public async Task SendThanksByIdWithHeaderImage(List<Guest> guests, Events events)
        {
            var twilioProfile = await db.TwilioProfileSettings
                   .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                   .AsNoTracking()
                   .FirstOrDefaultAsync();
            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var conguratulationId = Guid.NewGuid().ToString();
                var templateId = events.ThanksTempId;
                string[] parameters;
                if (events.SendingType == "Basic")
                {
                    parameters = new string[] { events.CongratulationMsgHeaderImg, conguratulationId, };

                }
                else
                {
                    parameters = new string[] { events.CongratulationMsgHeaderImg, guest.FirstName.Trim(), conguratulationId, };
                }
                await SendCustomMessageAndUpdateGuest(events, guest, fullPhoneNumber, conguratulationId, templateId, parameters, twilioProfile);
            });
            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();
            return;
        }

        public async Task SendCustomTemplateWithVariables(List<Guest> guests, Events events)
        {
            var twilioProfile = await db.TwilioProfileSettings
                              .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                              .AsNoTracking()
                              .FirstOrDefaultAsync();
            var templateId = events.ThanksTempId;
            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var conguratulationId = Guid.NewGuid().ToString();

                var matches = Regex.Matches(events.CustomCongratulationTemplateWithVariables, @"\{\{(.*?)\}\}");
                List<string> templateParameters = matches
                    .Cast<Match>()
                    .Select(m =>
                    {
                        string propName = m.Groups[1].Value;
                        if (propName == "GuestCard")
                        {
                            return events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                        }

                        if(propName == "CongratulationHeaderImage")
                        {
                            return events.CongratulationMsgHeaderImg ?? string.Empty;
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
                templateParameters.Add(conguratulationId);
                string[] parameters = templateParameters.ToArray();
                await SendCustomMessageAndUpdateGuest(events, guest, fullPhoneNumber, conguratulationId, templateId, parameters, twilioProfile);
            });
            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();
            return;
        }
    }
}
