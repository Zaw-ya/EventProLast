using System.Text.RegularExpressions;

using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;

using Google.Apis.Logging;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        // Done 
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
        // Done
        public async Task SendArabicCardwithname(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendArabicCardwithname for EventId={EventId}, GuestsCount={Count}",
                events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                          .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings.ArabicCardWithGuestName;
            _logger.LogInformation("Using Twilio templateId={TemplateId}", templateId);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, ct) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string imagePathSegment = GetFullImageUrl($"{events.Id}/E00000{events.Id}_{guest.GuestId}_{guest.NoOfMembers}.jpg", "cards");

                    var parameters = new string[]
                    {
                imagePathSegment.ToString(),
                guest.FirstName.Trim(),
                    };

                    _logger.LogDebug("Sending card to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);

                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent card to GuestId={GuestId}, Phone={Phone}", guest.GuestId, fullPhoneNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send card to GuestId={GuestId}, Phone={Phone}", guest.GuestId, $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}");
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendArabicCardwithname for EventId={EventId}", events.Id);
        }

        // Done
        public async Task SendEnglishCard(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendEnglishCard for EventId={EventId}, GuestsCount={Count}",
                events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                          .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings.EnglihsCardWithoutGuestName;
            _logger.LogInformation("Using Twilio templateId={TemplateId}", templateId);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, ct) =>
            {
                
            });

            foreach (var guest in guests)
            {
                try
                {

                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string imagePathSegment = GetFullImageUrl(
                        $"{events.Id}/E00000{events.Id}_{guest.GuestId}_{guest.NoOfMembers}.jpg", "cards");

                    var parameters = new string[]
                    {
                imagePathSegment.ToString(),
                    };

                    _logger.LogDebug("Sending English card to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);

                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent English card to GuestId={GuestId}, Phone={Phone}",
                        guest.GuestId, fullPhoneNumber);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send English card to GuestId={GuestId}, Phone={Phone}",
                        guest.GuestId, $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}");
                }
            }

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendEnglishCard for EventId={EventId}", events.Id);
        }
        public async Task SendEnglishCardwithname(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendEnglishCardwithName for EventId={EventId}, GuestsCount={Count}",
                events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                          .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings.EnglishCardWithGuestName;
            _logger.LogInformation("Using Twilio templateId={TemplateId}", templateId);

            int counter = SetSendingCounter(guests, events);

            //await Parallel.ForEachAsync(guests, parallelOptions, async (guest, ct) =>
            //{
            foreach (var guest in guests)
            {

                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string imagePathSegment = GetFullImageUrl(
                        $"{events.Id}/E00000{events.Id}_{guest.GuestId}_{guest.NoOfMembers}.jpg", "cards");

                    var parameters = new string[]
                    {
                imagePathSegment.ToString(),
                guest.FirstName.Trim(),
                    };

                    _logger.LogDebug("Sending English card with name to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);

                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent English card with name to GuestId={GuestId}, Phone={Phone}",
                        guest.GuestId, fullPhoneNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send English card with name to GuestId={GuestId}, Phone={Phone}",
                        guest.GuestId, $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}");
                }
            }
            //});

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendEnglishCardwithName for EventId={EventId}", events.Id);
        }

        public async Task SendCardByIDBasic(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendCardByIDBasic for EventId={EventId} with {GuestCount} guests",
                                    events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            var templateId = events.CustomCardInvitationTemplateName;
            _logger.LogInformation("Using template '{TemplateId}' for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            foreach (var guest in guests)
            {
                //await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                //{
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string imagePathSegment = GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");

                var parameters = new string[]
                {
            imagePathSegment.ToString(),
                };

                _logger.LogInformation("Sending Basic Card to GuestId={GuestId}, Phone={Phone}, Image={Image}",
                                        guest.GuestId, fullPhoneNumber, imagePathSegment);

                await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);

                counter = UpdateCounter(guests, events, counter);

                _logger.LogDebug("Updated sending counter: {Counter} for GuestId={GuestId}", counter, guest.GuestId);
                //});
            }

            await updateDataBaseAndDisposeCache(guests, events);
            _logger.LogInformation("Finished SendCardByIDBasic for EventId={EventId}", events.Id);

            return;
        }
        public async Task SendCardByIDWithGusetName(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendCardByIDWithGusetName for EventId={EventId} with {GuestCount} guests",
                                    events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();

            var templateId = events.CustomCardInvitationTemplateName;
            _logger.LogInformation("Using custom template '{TemplateId}' for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            foreach (var guest in guests)
            {
                //await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                //{
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string imagePathSegment = GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");

                var parameters = new string[]
                {
            imagePathSegment.ToString(),
            guest.FirstName.Trim(),
                };

                _logger.LogInformation("Sending Card with Name to GuestId={GuestId}, Phone={Phone}, Image={Image}, Name={Name}",
                                        guest.GuestId, fullPhoneNumber, imagePathSegment, guest.FirstName.Trim());

                await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);

                counter = UpdateCounter(guests, events, counter);
                _logger.LogDebug("Updated sending counter: {Counter} for GuestId={GuestId}", counter, guest.GuestId);
                //});
            }

            await updateDataBaseAndDisposeCache(guests, events);
            _logger.LogInformation("Finished SendCardByIDWithGusetName for EventId={EventId}", events.Id);

            return;
        }

        public async Task SendCustomTemplateWithVariables(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendCustomTemplateWithVariables for EventId={EventId} with {GuestCount} guests",
                                    events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                            .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync();

            var templateId = events.CustomCardInvitationTemplateName;
            _logger.LogInformation("Using custom template '{TemplateId}' for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                _logger.LogDebug("Processing GuestId={GuestId}, Name={GuestName}", guest.GuestId, guest.FirstName);

                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                _logger.LogDebug("Resolved full phone number: {PhoneNumber}", fullPhoneNumber);

                var matches = Regex.Matches(events.CustomCardTemplateWithVariables, @"\{\{(.*?)\}\}");
                List<string> templateParameters = new List<string>();

                foreach (Match m in matches)
                {
                    string propName = m.Groups[1].Value;
                    string value = null;

                    if (propName == "GuestCard")
                    {
                        value = GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");
                        _logger.LogDebug("Variable '{Variable}' resolved as image URL: {Value}", propName, value);
                    }
                    else if (propName == "CountOfAdditionalInvitations")
                    {
                        value = (guest.NoOfMembers - 1)?.ToString() ?? "0";
                        _logger.LogDebug("Variable '{Variable}' resolved as additional invitations: {Value}", propName, value);
                    }
                    else
                    {
                        value = guest.GetType().GetProperty(propName)?
                                        .GetValue(guest, null)?.ToString();

                        if (value == null)
                        {
                            value = events.GetType().GetProperty(propName)?
                                            .GetValue(events, null)?.ToString();
                        }

                        if (value == null)
                            value = propName; // fallback

                        _logger.LogDebug("Variable '{Variable}' resolved as '{Value}' for GuestId={GuestId}", propName, value, guest.GuestId);
                    }

                    templateParameters.Add(value);
                }

                string[] parameters = templateParameters.ToArray();
                _logger.LogDebug("Final parameters array for GuestId={GuestId}: [{Parameters}]", guest.GuestId, string.Join(", ", parameters));

                _logger.LogInformation("Sending Custom Template to GuestId={GuestId}, Phone={Phone}", guest.GuestId, fullPhoneNumber);
                await SendCardAndUpdateGuest(events, templateId, guest, fullPhoneNumber, parameters, guests, profileSettings);
                _logger.LogInformation("Sent Custom Template successfully to GuestId={GuestId}", guest.GuestId);

                counter = UpdateCounter(guests, events, counter);
                _logger.LogDebug("Updated sending counter to {Counter} after processing GuestId={GuestId}", counter, guest.GuestId);
            });

            await updateDataBaseAndDisposeCache(guests, events);
            _logger.LogInformation("Finished SendCustomTemplateWithVariables for EventId={EventId}", events.Id);
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
