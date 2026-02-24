using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioReminderMessageTemplates : TwilioMessagingConfiguration, IReminderMessageTemplates
    {
        private readonly EventProContext db;
        private readonly ILogger<TwilioCardTemplates> _logger;

        public TwilioReminderMessageTemplates(
            IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService,
            ILogger<TwilioCardTemplates> logger)
            : base(configuration, memoryCacheStoreService, logger)
        {
            db = new EventProContext(configuration);
            _logger = logger;
        }




        #region Marketing Message Helpers
        #region Marketing Interested Messages

        public async Task SendMarketingInterestedMsg(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendMarketingInterestedMsg for EventId={EventId}, GuestsCount={Count}",
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

            string templateId;
            if (string.IsNullOrEmpty(events.ResponseInterestedOfMarketingMsg))
            {
                templateId = profileSettings?.MarketingInterestMsg;
            }
            else
            {
                templateId = events.ResponseInterestedOfMarketingMsg;
            }

            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var parameters = new string[] { };

                    _logger.LogDebug("Sending marketing interested msg to GuestId={GuestId}, Phone={Phone}",
                        guest.GuestId, fullPhoneNumber);

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent marketing interested msg to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send marketing interested msg to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendMarketingInterestedMsg for EventId={EventId}", events.Id);
        }

        public async Task SendMarketingInterestedMsgWithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendMarketingInterestedMsgWithHeaderImage for EventId={EventId}, GuestsCount={Count}",
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

            string templateId;
            if (string.IsNullOrEmpty(events.ResponseInterestedOfMarketingMsg))
            {
                templateId = profileSettings?.MarketingInterestMsgWithImage;
            }
            else
            {
                templateId = events.ResponseInterestedOfMarketingMsg;
            }

            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var parameters = new string[]
                    {
                        events.ResponseInterestedOfMarketingMsgHeaderImage
                    };

                    _logger.LogDebug("Sending marketing interested msg with header image to GuestId={GuestId}, Phone={Phone}",
                        guest.GuestId, fullPhoneNumber);

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent marketing interested msg with header image to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send marketing interested msg with header image to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendMarketingInterestedMsgWithHeaderImage for EventId={EventId}", events.Id);
        }

        #endregion

        #region Marketing Not Interested Messages

        public async Task SendMarketingNotInterestedMsg(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendMarketingNotInterestedMsg for EventId={EventId}, GuestsCount={Count}",
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

            string templateId;
            if (string.IsNullOrEmpty(events.ResponseNotInterestedOfMarketingMsg))
            {
                templateId = profileSettings?.MarketingNotInterestMsg;
            }
            else
            {
                templateId = events.ResponseNotInterestedOfMarketingMsg;
            }

            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var parameters = new string[] { };

                    _logger.LogDebug("Sending marketing not interested msg to GuestId={GuestId}, Phone={Phone}",
                        guest.GuestId, fullPhoneNumber);

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent marketing not interested msg to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send marketing not interested msg to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendMarketingNotInterestedMsg for EventId={EventId}", events.Id);
        }

        public async Task SendMarketingNotInterestedMsgWithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendMarketingNotInterestedMsgWithHeaderImage for EventId={EventId}, GuestsCount={Count}",
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

            string templateId;
            if (string.IsNullOrEmpty(events.ResponseNotInterestedOfMarketingMsg))
            {
                templateId = profileSettings?.MarketingNotInterestMsgWithImage;
            }
            else
            {
                templateId = events.ResponseNotInterestedOfMarketingMsg;
            }

            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var parameters = new string[]
                    {
                        events.ResponseNotInterestedOfMarketingMsgHeaderImage
                    };

                    _logger.LogDebug("Sending marketing not interested msg with header image to GuestId={GuestId}, Phone={Phone}",
                        guest.GuestId, fullPhoneNumber);

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent marketing not interested msg with header image to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send marketing not interested msg with header image to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendMarketingNotInterestedMsgWithHeaderImage for EventId={EventId}", events.Id);
        }

        #endregion

        #endregion

        #region Custom Template With Variables
        // Custom Reminder Template With Variables like GuestCard, ReminderHeaderImage, CountOfAdditionalInvitations etc.
        public async Task SendCustomTemplateWithVariables(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendCustomTemplateWithVariables for EventId={EventId}, GuestsCount={Count}",
                events.Id, guests.Count);

            var twilioProfile = await db.TwilioProfileSettings
                                      .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = events.ReminderTempId;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    _logger.LogDebug("Processing GuestId={GuestId}, Name={GuestName}", guest.GuestId, guest.FirstName);

                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var eventCalenderICS = GetIcsFileUrl(events.Id);
                    var matches = Regex.Matches(events.CustomReminderTemplateWithVariables, @"\{\{(.*?)\}\}");
                    List<string> templateParameters = matches
                        .Cast<Match>()
                        .Select(m =>
                        {
                            string propName = m.Groups[1].Value;
                            if (propName == "GuestCard")
                            {
                                var value = GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");
                                _logger.LogDebug("Variable '{Variable}' resolved as image URL: {Value}", propName, value);
                                return value;
                            }

                            if (propName == "ReminderHeaderImage")
                            {
                                var value = events.ReminderMsgHeaderImg ?? string.Empty;
                                _logger.LogDebug("Variable '{Variable}' resolved as header image: {Value}", propName, value);
                                return value;
                            }

                            if (propName == "CountOfAdditionalInvitations")
                            {
                                var value = (guest.NoOfMembers - 1)?.ToString() ?? "0";
                                _logger.LogDebug("Variable '{Variable}' resolved as additional invitations: {Value}", propName, value);
                                return value;
                            }

                            var val = guest.GetType().GetProperty(propName)?
                                              .GetValue(guest, null)?.ToString();
                            if (val == null)
                            {
                                val = events.GetType().GetProperty(propName)?
                                              .GetValue(events, null)?.ToString();
                            }

                            val = val ?? propName;
                            _logger.LogDebug("Variable '{Variable}' resolved as '{Value}' for GuestId={GuestId}", propName, val, guest.GuestId);
                            return val;
                        })
                        .ToList();

                    templateParameters.Add(eventCalenderICS);
                    string[] parameters = templateParameters.ToArray();

                    _logger.LogDebug("Final parameters array for GuestId={GuestId}: [{Parameters}]",
                        guest.GuestId, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, twilioProfile);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent custom template to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send custom template to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendCustomTemplateWithVariables for EventId={EventId}", events.Id);
        }

        #endregion

        #region Custom Reminder Messages
        // Custom Reminder Message with Guest Name if basic or without Guest Name if not basic
        public async Task SendReminderCustom(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendReminderCustom for EventId={EventId}, GuestsCount={Count}",
                events.Id, guests.Count);

            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";

                    string[] parameters;
                    if (events.SendingType == "Basic")
                    {
                        templateId = profileSettings?.CustomReminderWithoutGuestName;
                        parameters = new string[] { events.ReminderMessage };
                    }
                    else
                    {
                        templateId = profileSettings?.CustomReminderWithGuestName;
                        parameters = new string[] { guest.FirstName.Trim(), events.ReminderMessage };
                    }

                    _logger.LogDebug("Sending custom reminder to GuestId={GuestId}, Phone={Phone}, TemplateId={TemplateId}",
                        guest.GuestId, fullPhoneNumber, templateId);

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent custom reminder to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send custom reminder to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendReminderCustom for EventId={EventId}", events.Id);
        }
        // Custom Reminder Message with Header Image , Guest Name if basic or without Guest Name if not basic
        public async Task SendReminderCustomWithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendReminderCustomWithHeaderImage for EventId={EventId}, GuestsCount={Count}",
                events.Id, guests.Count);

            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";

                    string[] parameters;
                    if (events.SendingType == "Basic")
                    {
                        templateId = profileSettings?.CustomReminderWithoutGuestNameWithHeaderImage;
                        parameters = new string[] { GetReminderHeaderImage(events), events.ReminderMessage };
                    }
                    else
                    {
                        templateId = profileSettings?.CustomReminderWithGuestNameWithHeaderImage;
                        parameters = new string[] { GetReminderHeaderImage(events), guest.FirstName.Trim(), events.ReminderMessage };
                    }

                    _logger.LogDebug("Sending custom reminder with header image to GuestId={GuestId}, Phone={Phone}, TemplateId={TemplateId}",
                        guest.GuestId, fullPhoneNumber, templateId);

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent custom reminder with header image to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send custom reminder with header image to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendReminderCustomWithHeaderImage for EventId={EventId}", events.Id);
        }

        #endregion

        #region Reminder With Template ID
        // Reminder Message with Template ID with Guest Name if basic or without Guest Name if not basic
        public async Task SendReminderWithTempId(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendReminderWithTempId for EventId={EventId}, GuestsCount={Count}",
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

            var templateId = events.ReminderTempId;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";

                    string[] parameters;
                    if (events.SendingType == "Basic")
                    {
                        parameters = new string[] { GetIcsFileUrl(events.Id) };
                    }
                    else
                    {
                        parameters = new string[] { GetIcsFileUrl(events.Id), guest.FirstName.Trim() };
                    }

                    _logger.LogDebug("Sending reminder with temp ID to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent reminder with temp ID to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder with temp ID to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendReminderWithTempId for EventId={EventId}", events.Id);
        }
        // Reminder Message with Template ID with Header Image , Guest Name if basic or without Guest Name if not basic
        public async Task SendReminderWithTempIdWithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendReminderWithTempIdWithHeaderImage for EventId={EventId}, GuestsCount={Count}",
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

            var templateId = events.ReminderTempId;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId ?? "NULL", events.Id);

            if (string.IsNullOrEmpty(templateId))
            {
                _logger.LogError(
                    "TemplateId is NULL! EventId={EventId}, ReminderTempId property value: {ReminderTempId}",
                    events.Id,
                    events.ReminderTempId ?? "NULL");
                return;
            }

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";

                    string[] parameters;
                    if (events.SendingType == "Basic")
                    {
                        parameters = new string[] { GetReminderHeaderImage(events), GetIcsFileUrl(events.Id) };
                    }
                    else
                    {
                        parameters = new string[] { GetReminderHeaderImage(events), guest.FirstName.Trim(), GetIcsFileUrl(events.Id) };
                    }

                    _logger.LogDebug("Sending reminder with temp ID and header image to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent reminder with temp ID and header image to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder with temp ID and header image to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendReminderWithTempIdWithHeaderImage for EventId={EventId}", events.Id);
        }

        #endregion

        #region Reminder Template 1
        // Reminder Template 1 Methods with variations like with ICS, with Header Image, with both ICS and Header Image etc.
        public async Task SendRTemp1(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendRTemp1 for EventId={EventId}, GuestsCount={Count}",
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

            var templateId = profileSettings?.ReminderTemp1;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var evntDate = Convert.ToDateTime(events.EventFrom);

                    var parameters = new string[]
                    {
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                        evntDate.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                    };

                    _logger.LogDebug("Sending RTemp1 to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent RTemp1 to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send RTemp1 to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendRTemp1 for EventId={EventId}", events.Id);
        }
        // Reminder Template 1 with Calendar ICS Method , which adds an .ics file link to the message parameters for calendar integration .
        public async Task SendRTemp1WithCalenderICS(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendRTemp1WithCalenderICS for EventId={EventId}, GuestsCount={Count}",
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

            var templateId = profileSettings?.ReminderTemp1WithCalenderIcs;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var evntDate = Convert.ToDateTime(events.EventFrom);

                    var parameters = new string[]
                    {
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                        evntDate.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        GetIcsFileUrl(events.Id)
                    };

                    _logger.LogDebug("Sending RTemp1 with ICS to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent RTemp1 with ICS to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send RTemp1 with ICS to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendRTemp1WithCalenderICS for EventId={EventId}", events.Id);
        }
        // Reminder Template 1 with Header Image Method , which adds a header image link to the message parameters for enhanced visual appeal .
        public async Task SendRTemp1WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendRTemp1WithHeaderImage for EventId={EventId}, GuestsCount={Count}",
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

            var templateId = profileSettings?.ReminderTemp1WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var evntDate = Convert.ToDateTime(events.EventFrom);


                    var parameters = new string[]
                    {
                        GetReminderHeaderImage(events),
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                        evntDate.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                    };

                    _logger.LogDebug("Sending RTemp1 with header image to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent RTemp1 with header image to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send RTemp1 with header image to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendRTemp1WithHeaderImage for EventId={EventId}", events.Id);
        }
        // Reminder Template 1 with Header Image and Calendar ICS Method , which combines both a header image and an .ics file link in the message parameters for a comprehensive reminder experience .
        public async Task SendRTemp1WithHeaderImageWithCalenderICS(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendRTemp1WithHeaderImageWithCalenderICS for EventId={EventId}, GuestsCount={Count}",
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

            var templateId = profileSettings?.ReminderTemp1WithHeaderImageWithCalenderIcs;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    var messagrHeaderImage = GetReminderHeaderImage(events);
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var evntDate = Convert.ToDateTime(events.EventFrom);

                    var parameters = new string[]
                    {
                        messagrHeaderImage,
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                        evntDate.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        GetIcsFileUrl(events.Id)
                    };

                    _logger.LogDebug("Sending RTemp1 with header image and ICS to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent RTemp1 with header image and ICS to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send RTemp1 with header image and ICS to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendRTemp1WithHeaderImageWithCalenderICS for EventId={EventId}", events.Id);
        }

        #endregion

        #region Reminder Template 2 or 3 (Gender-based)
        // Reminder Template 2 or 3 Methods with variations like with ICS, with Header Image, with both ICS and Header Image etc.
        public async Task SendRTemp2or3(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendRTemp2or3 for EventId={EventId}, GuestsCount={Count}, Gender={Gender}",
                events.Id, guests.Count, events.ParentTitleGender);

            var twilioProfile = await db.TwilioProfileSettings
                            .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = twilioProfile?.ReminderTemp2;

            if (events.ParentTitleGender != "Female")
            {
                templateId = twilioProfile?.ReminderTemp3;
            }

            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId} (Gender={Gender})",
                templateId, events.Id, events.ParentTitleGender);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var evntDate = Convert.ToDateTime(events.EventFrom);

                    var parameters = new string[]
                    {
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        events.EventFrom.Value.ToString("dddd", new CultureInfo("ar-SA")),
                        events.EventFrom.Value.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        events.ParentTitle.Trim(),
                    };

                    _logger.LogDebug("Sending RTemp2or3 to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, twilioProfile);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent RTemp2or3 to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send RTemp2or3 to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendRTemp2or3 for EventId={EventId}", events.Id);
        }
        // Reminder Template 2 or 3 with Calendar ICS Method , which adds an .ics file link to the message parameters for calendar integration .
        public async Task SendRTemp2or3WithCalenderICS(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendRTemp2or3WithCalenderICS for EventId={EventId}, GuestsCount={Count}, Gender={Gender}",
                events.Id, guests.Count, events.ParentTitleGender);

            var twilioProfile = await db.TwilioProfileSettings
                            .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = twilioProfile?.ReminderTemp2WithCalenderIcs;

            if (events.ParentTitleGender != "Female")
            {
                templateId = twilioProfile?.ReminderTemp3WithCalenderIcs;
            }

            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId} (Gender={Gender})",
                templateId, events.Id, events.ParentTitleGender);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var evntDate = Convert.ToDateTime(events.EventFrom);

                    var parameters = new string[]
                    {
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        events.EventFrom.Value.ToString("dddd", new CultureInfo("ar-SA")),
                        events.EventFrom.Value.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        events.ParentTitle.Trim(),
                        GetIcsFileUrl(events.Id)
                    };

                    _logger.LogDebug("Sending RTemp2or3 with ICS to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, twilioProfile);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent RTemp2or3 with ICS to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send RTemp2or3 with ICS to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendRTemp2or3WithCalenderICS for EventId={EventId}", events.Id);
        }
        // Reminder Template 2 or 3 with Header Image Method , which adds a header image link to the message parameters for enhanced visual appeal .
        public async Task SendRTemp2or3WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendRTemp2or3WithHeaderImage for EventId={EventId}, GuestsCount={Count}, Gender={Gender}",
                events.Id, guests.Count, events.ParentTitleGender);

            var twilioProfile = await db.TwilioProfileSettings
                           .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                           .AsNoTracking()
                           .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = twilioProfile?.ReminderTemp2WithHeaderImage;

            if (events.ParentTitleGender != "Female")
            {
                templateId = twilioProfile?.ReminderTemp3;
            }

            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId} (Gender={Gender})",
                templateId, events.Id, events.ParentTitleGender);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var evntDate = Convert.ToDateTime(events.EventFrom);

                    var parameters = new string[]
                    {
                        GetReminderHeaderImage(events),
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        events.EventFrom.Value.ToString("dddd", new CultureInfo("ar-SA")),
                        events.EventFrom.Value.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        events.ParentTitle.Trim(),
                    };

                    _logger.LogDebug("Sending RTemp2or3 with header image to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, twilioProfile);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent RTemp2or3 with header image to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send RTemp2or3 with header image to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendRTemp2or3WithHeaderImage for EventId={EventId}", events.Id);
        }
        // Reminder Template 2 or 3 with Header Image and Calendar ICS Method , which combines both a header image and an .ics file link in the message parameters for a comprehensive reminder experience .
        public async Task SendRTemp2or3WithHeaderImageWithCalenderIcs(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendRTemp2or3WithHeaderImageWithCalenderIcs for EventId={EventId}, GuestsCount={Count}, Gender={Gender}",
                events.Id, guests.Count, events.ParentTitleGender);

            var twilioProfile = await db.TwilioProfileSettings
                           .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                           .AsNoTracking()
                           .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = twilioProfile?.ReminderTemp2WithHeaderImageWithCalenderIcs;

            if (events.ParentTitleGender != "Female")
            {
                templateId = twilioProfile?.ReminderTemp3WithHeaderImageWithCalenderIcs;
            }

            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId} (Gender={Gender})",
                templateId, events.Id, events.ParentTitleGender);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var evntDate = Convert.ToDateTime(events.EventFrom);

                    var parameters = new string[]
                    {
                        GetReminderHeaderImage(events),
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        events.EventFrom.Value.ToString("dddd", new CultureInfo("ar-SA")),
                        events.EventFrom.Value.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        events.ParentTitle.Trim(),
                        GetIcsFileUrl(events.Id)
                    };

                    _logger.LogDebug("Sending RTemp2or3 with header image and ICS to GuestId={GuestId}, Phone={Phone}, Parameters={Parameters}",
                        guest.GuestId, fullPhoneNumber, string.Join(", ", parameters));

                    await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, twilioProfile);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent RTemp2or3 with header image and ICS to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send RTemp2or3 with header image and ICS to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendRTemp2or3WithHeaderImageWithCalenderIcs for EventId={EventId}", events.Id);
        }

        #endregion

        #region Private Helper Methods
        // Common method to send WhatsApp template message and update guest information
        private async Task SendMessageAndUpdateGuest(Events events, Guest guest, string fullPhoneNumber, string templateId, string[] parameters, List<Guest> guests, TwilioProfileSettings profileSettings)
        {
            _logger.LogInformation(
                "Sending WhatsApp reminder. EventId={EventId}, GuestId={GuestId}, Phone={Phone}, TemplateId={TemplateId}",
                events.Id, guest.GuestId, fullPhoneNumber, templateId);

            try
            {
                var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);

                if (messageSid != null)
                {
                    guest.ReminderMessageId = messageSid;
                    guest.ReminderMessageRead = null;
                    guest.ReminderMessageDelivered = null;
                    guest.ReminderMessageSent = null;
                    guest.ReminderMessageFailed = null;

                    if (guests.Count > 1)
                    {
                        _memoryCacheStoreService.save(messageSid, 0);
                    }

                    _logger.LogInformation(
                        "Message sent successfully. GuestId={GuestId}, MessageSid={MessageSid}",
                        guest.GuestId, messageSid);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send message (null SID). GuestId={GuestId}",
                        guest.GuestId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception while sending reminder message. EventId={EventId}, GuestId={GuestId}",
                    events.Id, guest.GuestId);
            }
        }
        // Common method to update the database and dispose of cache entries
        private async Task updateDataBaseAndDisposeCache(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Updating database for EventId={EventId}, GuestsCount={Count}",
                events.Id, guests.Count);

            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();

            if (guests.Count > 1)
            {
                await Task.Delay(10000);
                _memoryCacheStoreService.delete(events.Id.ToString());
                foreach (var guest in guests)
                {
                    if (guest.ReminderMessageId != null)
                    {
                        _memoryCacheStoreService.delete(guest.ReminderMessageId);
                    }
                }
            }

            _logger.LogInformation("Database updated and cache disposed for EventId={EventId}", events.Id);
        }

        /// <summary>
        /// Gets the reminder header image URL with the 'events/' prefix trimmed.
        /// </summary>
        /// <param name="events">The event containing the header image</param>
        /// <returns>Trimmed header image URL or empty string if null</returns>
        private string GetReminderHeaderImage(Events events)
        {
            var headerImage = events.ReminderMsgHeaderImg ?? string.Empty;
            if (!string.IsNullOrEmpty(headerImage))
            {
                headerImage = Regex.Replace(headerImage, @".*events/", "");
            }
            return headerImage;
        }

        #endregion


    }
}
