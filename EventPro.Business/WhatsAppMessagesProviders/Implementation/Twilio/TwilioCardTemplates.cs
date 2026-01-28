using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using System.Text.RegularExpressions;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioCardTemplates : TwilioMessagingConfiguration, ICardMessageTemplates
    {
        private readonly EventProContext db;
        private readonly ILogger<TwilioCardTemplates> _logger;
        public TwilioCardTemplates(IConfiguration configuration,
        IMemoryCacheStoreService memoryCacheStoreService,
        ILogger<TwilioCardTemplates> logger) : base(configuration,
            memoryCacheStoreService,logger)
        {
            db = new EventProContext(configuration);
            _logger = logger;
        }
        public async Task SendArabicCard(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
                "SendArabicCard started. EventId {EventId}, GuestsCount {GuestsCount}",
                events.Id,
                guests.Count
            );

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogError(
                    "Twilio profile settings not found. EventId {EventId}, Profile {Profile}",
                    events.Id,
                    events.choosenSendingWhatsappProfile
                );
                return;
            }

            var templateId = profileSettings.ArabicCardWithoutGuestName;

            _logger.LogInformation(
                "Using Arabic template. EventId {EventId}, TemplateId {TemplateId}",
                events.Id,
                templateId
            );

            int counter = SetSendingCounter(guests, events);

            try
            {
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, _) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string imagePathSegment =
                        GetFullImageUrl($"{events.Id}/E00000{events.Id}_{guest.GuestId}_{guest.NoOfMembers}.jpg", "cards");

                    var parameters = new[] { imagePathSegment };

                    await SendCardAndUpdateGuest(
                        events,
                        templateId,
                        guest,
                        fullPhoneNumber,
                        parameters,
                        guests,
                        profileSettings
                    );

                    counter = UpdateCounter(guests, events, counter);
                });

                _logger.LogInformation(
                    "SendArabicCard finished successfully. EventId {EventId}",
                    events.Id
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while sending Arabic cards. EventId {EventId}",
                    events.Id
                );
                throw;
            }

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation(
                "Database updated and cache disposed. EventId {EventId}",
                events.Id
            );
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
                string imagePathSegment = GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");
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
                string imagePathSegment = GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");
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
                string imagePathSegment = GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");
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
                string imagePathSegment = GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");
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
            // Gharabawy : فرضا العميل كان عاوز حاجه علي مزاجه الي بيحصل اني بضيف التيمبلت الي عاوز يبعتها دي
            // ف تويليو ثم بروح اضيفها تبع الايفينت مش تبع البروفايل سيتينج

            // علي عكس مثلا الحاجات الي الاستنادرد
            //             var templateId = profileSettings?.ArabicCardWithoutGuestName;
            // كان بيروح يجيبها من البروفايل سيتينجز 
            var templateId = events.CustomCardInvitationTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string imagePathSegment = GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");
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

        // Send only the guests cards
        public async Task SendCustomTemplateWithVariables(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                            .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync();

            var templateId = events.CustomCardInvitationTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions,
                async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                // مرحبًا {{FirstName}}, انت مدعو لحفلنا في {{City}} بتاريخ {{EventDate}}.
                // Each thing between {{ }} should be replaced

                var matches = Regex.Matches(events.CustomCardTemplateWithVariables, @"\{\{(.*?)\}\}");
                
                // Loop on the matched results [first-name, City, EventDate] (example)
                List<string> templateParameters = matches
                    .Cast<Match>()
                    .Select(m =>
                    {
                        string propName = m.Groups[1].Value;
                        // Here we using the reflection to know each key in the mathes 
                        if (propName == "GuestCard")
                        {
                            return GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");
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

        private async Task SendCardAndUpdateGuest(
    Events events,
    string? templateId,
    Guest guest,
    string fullPhoneNumber,
    string[] parameters,
    List<Guest> guests,
    TwilioProfileSettings profileSettings)
        {
            _logger.LogInformation(
                "Sending WhatsApp card. EventId {EventId}, GuestId {GuestId}, Phone {Phone}, TemplateId {TemplateId}",
                events.Id,
                guest.GuestId,
                fullPhoneNumber,
                templateId
            );

            try
            {
                string messageSid = await SendWhatsAppTemplateMessageAsync(
                    fullPhoneNumber,
                    templateId,
                    parameters,
                    events.CityId,
                    events.ChoosenNumberWithinCountry,
                    profileSettings,
                    events.choosenSendingCountryNumber
                );

                if (!string.IsNullOrEmpty(messageSid))
                {
                    guest.ImgSentMsgId = messageSid;
                    guest.ImgSenOn = DateTime.Now.ToString();
                    guest.Qrresponse = "Message Processed Successfully";
                    guest.WhatsappStatus = "sent";

                    if (guests.Count > 1)
                        _memoryCacheStoreService.save(messageSid, 0);

                    _logger.LogInformation(
                        "Message sent successfully. GuestId {GuestId}, MessageSid {MessageSid}",
                        guest.GuestId,
                        messageSid
                    );
                }
                else
                {
                    guest.WhatsappStatus = "error";
                    _logger.LogWarning(
                        "Failed to send message. GuestId {GuestId}",
                        guest.GuestId
                    );
                }
            }
            catch (Exception ex)
            {
                guest.WhatsappStatus = "error";
                _logger.LogError(
                    ex,
                    "Exception while sending message. EventId {EventId}, GuestId {GuestId}",
                    events.Id,
                    guest.GuestId
                );
            }
            //await Task.Delay(300);
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
    }
}
