using System.Globalization;
using System.Text.RegularExpressions;

using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioReminderMessageTemplates : TwilioMessagingConfiguration, IReminderMessageTemplates
    {
        private readonly EventProContext db;
        public TwilioReminderMessageTemplates(
            IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService,
            ILogger<TwilioMessagingConfiguration> logger) 
            : base(configuration, memoryCacheStoreService, logger)
        {
            db = new EventProContext(configuration);
        }

        public async Task SendCustomTemplateWithVariables(List<Guest> guests, Events events)
        {
            var twilioProfile = await db.TwilioProfileSettings
                                      .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync();

            var templateId = events.ReminderTempId;

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var eventCalenderICS = events.Id.ToString() + ".ics";
                var matches = Regex.Matches(events.CustomReminderTemplateWithVariables, @"\{\{(.*?)\}\}");
                List<string> templateParameters = matches
                    .Cast<Match>()
                    .Select(m =>
                    {
                        string propName = m.Groups[1].Value;
                        if (propName == "GuestCard")
                        {
                            return events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                        }

                        if (propName == "ReminderHeaderImage")
                        {
                            return events.ReminderMsgHeaderImg ?? string.Empty;
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

                templateParameters.Add(eventCalenderICS);
                string[] parameters = templateParameters.ToArray();

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, twilioProfile);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendMarketingInterestedMsg(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                              .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                              .AsNoTracking()
                              .FirstOrDefaultAsync();
            string templateId;
            if (string.IsNullOrEmpty(events.ResponseInterestedOfMarketingMsg))
            {
                templateId = profileSettings?.MarketingInterestMsg;
            }
            else
            {
                templateId = events.ResponseInterestedOfMarketingMsg;
            }
                             
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var evntDate = Convert.ToDateTime(events.EventFrom);

                var parameters = new string[]
                {
                };

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendMarketingInterestedMsgWithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                            .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();
            string templateId;
            if (string.IsNullOrEmpty(events.ResponseInterestedOfMarketingMsg))
            {
                templateId = profileSettings?.MarketingInterestMsgWithImage;
            }
            else
            {
                templateId = events.ResponseInterestedOfMarketingMsg;
            }

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var evntDate = Convert.ToDateTime(events.EventFrom);

                var parameters = new string[]
                {
                    events.ResponseInterestedOfMarketingMsgHeaderImage
                };

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendMarketingNotInterestedMsg(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                            .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();
            string templateId;
            if (string.IsNullOrEmpty(events.ResponseNotInterestedOfMarketingMsg))
            {
                templateId = profileSettings?.MarketingNotInterestMsg;
            }
            else
            {
                templateId = events.ResponseNotInterestedOfMarketingMsg;
            }

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var evntDate = Convert.ToDateTime(events.EventFrom);

                var parameters = new string[]
                {
                };

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendMarketingNotInterestedMsgWithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            string templateId;
            if (string.IsNullOrEmpty(events.ResponseNotInterestedOfMarketingMsg))
            {
                templateId = profileSettings?.MarketingNotInterestMsgWithImage;
            }
            else
            {
                templateId = events.ResponseNotInterestedOfMarketingMsg;
            }

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var evntDate = Convert.ToDateTime(events.EventFrom);

                var parameters = new string[]
                {
                    events.ResponseNotInterestedOfMarketingMsgHeaderImage
                };

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendReminderCustom(List<Guest> guests, Events events)
        {
            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
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

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendReminderCustomWithHeaderImage(List<Guest> guests, Events events)
        {
            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";

                string[] parameters;
                if (events.SendingType == "Basic")
                {
                    templateId = profileSettings?.CustomReminderWithoutGuestNameWithHeaderImage;
                    parameters = new string[] { events.ReminderMsgHeaderImg, events.ReminderMessage };
                }
                else
                {
                    templateId = profileSettings?.CustomReminderWithGuestNameWithHeaderImage;
                    parameters = new string[] { events.ReminderMsgHeaderImg, guest.FirstName.Trim(), events.ReminderMessage };
                }

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendReminderWithTempId(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                    .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            var templateId = events.ReminderTempId;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";

                string[] parameters;
                if (events.SendingType == "Basic")
                {
                    parameters = new string[] {events.Id.ToString() +".ics"  };
                }
                else
                {
                    parameters = new string[] { guest.FirstName.Trim(), events.Id.ToString() + ".ics" };
                }

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendReminderWithTempIdWithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
        .Where(e => e.Name == events.choosenSendingWhatsappProfile)
        .AsNoTracking()
        .FirstOrDefaultAsync();
            var templateId = events.ReminderTempId;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";

                string[] parameters;
                if (events.SendingType == "Basic")
                {
                    parameters = new string[] {events.ReminderMsgHeaderImg , events.Id.ToString() + ".ics" };
                }
                else
                {
                    parameters = new string[] { events.ReminderMsgHeaderImg , guest.FirstName.Trim(), events.Id.ToString() + ".ics" };
                }

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendRTemp1(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = profileSettings?.ReminderTemp1;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
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

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        public async Task SendRTemp1WithCalenderICS(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = profileSettings?.ReminderTemp1WithCalenderIcs;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
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
                        events.Id.ToString() + ".ics"
                };

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        public async Task SendRTemp1WithHeaderImage(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = profileSettings?.ReminderTemp1WithHeaderImage;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var evntDate = Convert.ToDateTime(events.EventFrom);

                var parameters = new string[]
                {
                        events.ReminderMsgHeaderImg,
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                        evntDate.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                };

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        public async Task SendRTemp1WithHeaderImageWithCalenderICS(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = profileSettings?.ReminderTemp1WithHeaderImageWithCalenderIcs;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var evntDate = Convert.ToDateTime(events.EventFrom);

                var parameters = new string[]
                {
                        events.ReminderMsgHeaderImg,
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                        evntDate.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        events.Id.ToString() + ".ics"
                };

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        public async Task SendRTemp2or3(List<Guest> guests, Events events)
        {
            var twilioProfile = await db.TwilioProfileSettings
                            .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();

            var templateId = twilioProfile?.ReminderTemp2;

            if (events.ParentTitleGender != "Female")
            {
                templateId = twilioProfile?.ReminderTemp3;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
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

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, twilioProfile);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        public async Task SendRTemp2or3WithCalenderICS(List<Guest> guests, Events events)
        {
            var twilioProfile = await db.TwilioProfileSettings
                            .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();

            var templateId = twilioProfile?.ReminderTemp2WithCalenderIcs;

            if (events.ParentTitleGender != "Female")
            {
                templateId = twilioProfile?.ReminderTemp3WithCalenderIcs;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
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
                        events.Id.ToString() + ".ics"
                };

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, twilioProfile);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        public async Task SendRTemp2or3WithHeaderImage(List<Guest> guests, Events events)
        {
            var twilioProfile = await db.TwilioProfileSettings
                           .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                           .AsNoTracking()
                           .FirstOrDefaultAsync();

            var templateId = twilioProfile?.ReminderTemp2WithHeaderImage;

            if (events.ParentTitleGender != "Female")
            {
                templateId = twilioProfile?.ReminderTemp3;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var evntDate = Convert.ToDateTime(events.EventFrom);

                var parameters = new string[]
                {
                        events.ReminderMsgHeaderImg,
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        events.EventFrom.Value.ToString("dddd", new CultureInfo("ar-SA")),
                        events.EventFrom.Value.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        events.ParentTitle.Trim(),
                };

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, twilioProfile);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        public async Task SendRTemp2or3WithHeaderImageWithCalenderIcs(List<Guest> guests, Events events)
        {
            var twilioProfile = await db.TwilioProfileSettings
                           .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                           .AsNoTracking()
                           .FirstOrDefaultAsync();

            var templateId = twilioProfile?.ReminderTemp2WithHeaderImageWithCalenderIcs;

            if (events.ParentTitleGender != "Female")
            {
                templateId = twilioProfile?.ReminderTemp3WithHeaderImageWithCalenderIcs;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                var evntDate = Convert.ToDateTime(events.EventFrom);

                var parameters = new string[]
                {
                        events.ReminderMsgHeaderImg,
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        events.EventFrom.Value.ToString("dddd", new CultureInfo("ar-SA")),
                        events.EventFrom.Value.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        events.ParentTitle.Trim(),
                        events.Id.ToString() + ".ics"
                };

                await SendMessageAndUpdateGuest(events, guest, fullPhoneNumber, templateId, parameters, guests, twilioProfile);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        private async Task SendMessageAndUpdateGuest(Events events, Guest guest, string fullPhoneNumber, string templateId, string[] parameters, List<Guest> guests, TwilioProfileSettings profileSettings)
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
                    if (guest.ReminderMessageId != null)
                    {
                        _memoryCacheStoreService.delete(guest.ReminderMessageId);
                    }
                }
            }
        }
    }
}
