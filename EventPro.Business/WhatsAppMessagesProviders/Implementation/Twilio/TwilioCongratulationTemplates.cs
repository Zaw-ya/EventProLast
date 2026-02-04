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
        private readonly ILogger<TwilioCardTemplates> _logger;

        public TwilioCongratulationTemplates(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService,
            ILogger<TwilioCardTemplates> logger) : base(configuration,
                memoryCacheStoreService, logger)
        {
            db = new EventProContext(configuration);
            _logger = logger;
        }

        #region Congratulation Message To Owner
        // Sends congratulation messages to the event owner in Arabic.
        public async Task SendCongratulationMessageToOwner(List<Guest> guests, Events events, string message)
        {
            _logger.LogInformation("Starting SendCongratulationMessageToOwner for EventId={EventId}, GuestsCount={Count}",
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

            var templateId = profileSettings.ArabicCongratulationMessageToEventOwner;
            _logger.LogInformation("Using Arabic owner TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendMessageToOwnerAndUpdateGuest(guests, events, message, templateId);

            _logger.LogInformation("Finished SendCongratulationMessageToOwner for EventId={EventId}", events.Id);
        }
        // Sends congratulation messages to the event owner in English.
        public async Task SendCongratulationMessageToOwnerEnglish(List<Guest> guests, Events events, string message)
        {
            _logger.LogInformation("Starting SendCongratulationMessageToOwnerEnglish for EventId={EventId}, GuestsCount={Count}",
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

            var templateId = profileSettings.EnglishCongratulationMessageToEventOwner;
            _logger.LogInformation("Using English owner TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendMessageToOwnerAndUpdateGuest(guests, events, message, templateId);

            _logger.LogInformation("Finished SendCongratulationMessageToOwnerEnglish for EventId={EventId}", events.Id);
        }

        #endregion

        #region Default Templates (1-10)
        // Sends congratulation messages using default template from 1 to 10 , Constant message .
        public async Task SendTemp1(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp1 for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp1;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp1 for EventId={EventId}", events.Id);
        }
       
        public async Task SendTemp2(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp2 for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp2;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp2 for EventId={EventId}", events.Id);
        }

        public async Task SendTemp3(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp3 for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp3;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp3 for EventId={EventId}", events.Id);
        }

        public async Task SendTemp4(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp4 for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp4;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp4 for EventId={EventId}", events.Id);
        }

        public async Task SendTemp5(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp5 for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp5;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp5 for EventId={EventId}", events.Id);
        }

        public async Task SendTemp6(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp6 for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp6;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp6 for EventId={EventId}", events.Id);
        }

        public async Task SendTemp7(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp7 for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp7;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp7 for EventId={EventId}", events.Id);
        }

        public async Task SendTemp8(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp8 for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp8;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp8 for EventId={EventId}", events.Id);
        }

        public async Task SendTemp9(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp9 for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp9;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp9 for EventId={EventId}", events.Id);
        }

        public async Task SendTemp10(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp10 for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp10;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp10 for EventId={EventId}", events.Id);
        }

        #endregion

        #region Default Templates With Header Image (1-10)

        public async Task SendTemp1WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp1WithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                      .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                      .AsNoTracking()
                      .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp1WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp1WithHeaderImage for EventId={EventId}", events.Id);
        }

        public async Task SendTemp2WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp2WithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp2WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp2WithHeaderImage for EventId={EventId}", events.Id);
        }

        public async Task SendTemp3WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp3WithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp3WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp3WithHeaderImage for EventId={EventId}", events.Id);
        }

        public async Task SendTemp4WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp4WithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp4WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp4WithHeaderImage for EventId={EventId}", events.Id);
        }

        public async Task SendTemp5WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp5WithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp5WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp5WithHeaderImage for EventId={EventId}", events.Id);
        }

        public async Task SendTemp6WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp6WithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp6WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp6WithHeaderImage for EventId={EventId}", events.Id);
        }

        public async Task SendTemp7WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp7WithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp7WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp7WithHeaderImage for EventId={EventId}", events.Id);
        }

        public async Task SendTemp8WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp8WithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp8WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp8WithHeaderImage for EventId={EventId}", events.Id);
        }

        public async Task SendTemp9WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp9WithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp9WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp9WithHeaderImage for EventId={EventId}", events.Id);
        }

        public async Task SendTemp10WithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendTemp10WithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (profileSettings == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = profileSettings?.ThanksTemp10WithHeaderImage;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(guests, events, templateId, profileSettings);

            _logger.LogInformation("Finished SendTemp10WithHeaderImage for EventId={EventId}", events.Id);
        }

        #endregion

        #region Thanks By ID

        public async Task SendThanksById(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendThanksById for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var twilioProfile = await db.TwilioProfileSettings
                    .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = events.ThanksTempId;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var conguratulationId = Guid.NewGuid().ToString();
                    string[] parameters;
                    if (events.SendingType == "Basic")
                    {
                        parameters = new string[] { conguratulationId, };
                    }
                    else
                    {
                        parameters = new string[] { guest.FirstName.Trim(), conguratulationId, };
                    }

                    _logger.LogDebug("Sending thanks by ID to GuestId={GuestId}, Phone={Phone}", guest.GuestId, fullPhoneNumber);

                    await SendCustomMessageAndUpdateGuest(events, guest, fullPhoneNumber, conguratulationId, templateId, parameters, twilioProfile);

                    _logger.LogInformation("Successfully sent thanks by ID to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send thanks by ID to GuestId={GuestId}", guest.GuestId);
                }
            });

            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();

            _logger.LogInformation("Finished SendThanksById for EventId={EventId}", events.Id);
        }

        public async Task SendThanksByIdWithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendThanksByIdWithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var twilioProfile = await db.TwilioProfileSettings
                   .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                   .AsNoTracking()
                   .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = events.ThanksTempId;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var conguratulationId = Guid.NewGuid().ToString();
                    string[] parameters;
                    if (events.SendingType == "Basic")
                    {
                        parameters = new string[] { GetCongratulationHeaderImage(events), conguratulationId, };
                    }
                    else
                    {
                        parameters = new string[] { GetCongratulationHeaderImage(events), guest.FirstName.Trim(), conguratulationId, };
                    }

                    _logger.LogDebug("Sending thanks by ID with header image to GuestId={GuestId}, Phone={Phone}", guest.GuestId, fullPhoneNumber);

                    await SendCustomMessageAndUpdateGuest(events, guest, fullPhoneNumber, conguratulationId, templateId, parameters, twilioProfile);

                    _logger.LogInformation("Successfully sent thanks by ID with header image to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send thanks by ID with header image to GuestId={GuestId}", guest.GuestId);
                }
            });

            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();

            _logger.LogInformation("Finished SendThanksByIdWithHeaderImage for EventId={EventId}", events.Id);
        }

        #endregion

        #region Thanks Custom Message

        public async Task SendThanksCustom(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendThanksCustom for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            int counter = SetSendingCounter(guests, events);
            var twilioProfile = await db.TwilioProfileSettings
                                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
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

                    _logger.LogDebug("Sending custom thanks to GuestId={GuestId}, Phone={Phone}, TemplateId={TemplateId}",
                        guest.GuestId, fullPhoneNumber, templateId);

                    await SendCustomMessageAndUpdateGuest(events, guest, fullPhoneNumber, conguratulationId, templateId, parameters, twilioProfile);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent custom thanks to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send custom thanks to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendThanksCustom for EventId={EventId}", events.Id);
        }

        public async Task SendThanksCustomWithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendThanksCustomWithHeaderImage for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            int counter = SetSendingCounter(guests, events);
            var twilioProfile = await db.TwilioProfileSettings
                                .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var conguratulationId = Guid.NewGuid().ToString();
                    var templateId = "";
                    string[] parameters;
                    if (events.SendingType == "Basic")
                    {
                        templateId = twilioProfile?.CustomThanksWithoutGuestNameWithHeaderImage;
                        parameters = new string[] { GetCongratulationHeaderImage(events), events.ThanksMessage, conguratulationId, };
                    }
                    else
                    {
                        templateId = twilioProfile?.CustomThanksWithGuestNameWithHeaderImage;
                        parameters = new string[] { GetCongratulationHeaderImage(events), guest.FirstName.Trim(), events.ThanksMessage, conguratulationId, };
                    }

                    _logger.LogDebug("Sending custom thanks with header image to GuestId={GuestId}, Phone={Phone}, TemplateId={TemplateId}",
                        guest.GuestId, fullPhoneNumber, templateId);

                    await SendCustomMessageAndUpdateGuest(events, guest, fullPhoneNumber, conguratulationId, templateId, parameters, twilioProfile);
                    counter = UpdateCounter(guests, events, counter);

                    _logger.LogInformation("Successfully sent custom thanks with header image to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send custom thanks with header image to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("Finished SendThanksCustomWithHeaderImage for EventId={EventId}", events.Id);
        }

        #endregion

        #region Custom Template With Variables

        public async Task SendCustomTemplateWithVariables(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Starting SendCustomTemplateWithVariables for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

            var twilioProfile = await db.TwilioProfileSettings
                              .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                              .AsNoTracking()
                              .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            var templateId = events.ThanksTempId;
            _logger.LogInformation("Using TemplateId={TemplateId} for EventId={EventId}", templateId, events.Id);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    _logger.LogDebug("Processing GuestId={GuestId}, Name={GuestName}", guest.GuestId, guest.FirstName);

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
                                var value = GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");
                                _logger.LogDebug("Variable '{Variable}' resolved as image URL: {Value}", propName, value);
                                return value;
                            }

                            if (propName == "CongratulationHeaderImage")
                            {
                                var value = GetCongratulationHeaderImage(events) ?? string.Empty;
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

                    templateParameters.Add(conguratulationId);
                    string[] parameters = templateParameters.ToArray();

                    _logger.LogDebug("Final parameters array for GuestId={GuestId}: [{Parameters}]",
                        guest.GuestId, string.Join(", ", parameters));

                    await SendCustomMessageAndUpdateGuest(events, guest, fullPhoneNumber, conguratulationId, templateId, parameters, twilioProfile);

                    _logger.LogInformation("Successfully sent custom template to GuestId={GuestId}", guest.GuestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send custom template to GuestId={GuestId}", guest.GuestId);
                }
            });

            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();

            _logger.LogInformation("Finished SendCustomTemplateWithVariables for EventId={EventId}", events.Id);
        }

        #endregion

        #region Private Helper Methods

        private string GetCongratulationHeaderImage(Events events)
        {
            // Extract only the image file name from the full URL or path https://res.cloudinary.com/dewicwvoe/image/upload/events/50e30821a2f8c2122bfe78a013058fbb.jpg
            string congratulationHeaderImage = events.CongratulationMsgHeaderImg;
            if (!string.IsNullOrEmpty(congratulationHeaderImage))
                congratulationHeaderImage = Regex.Replace(congratulationHeaderImage, @".*events\/", "");
            // to get only 50e30821a2f8c2122bfe78a013058fbb.jpg
            return congratulationHeaderImage;
        }

        private async Task SendCustomMessageAndUpdateGuest(Events events, Guest guest, string fullPhoneNumber, string conguratulationId, string? templateId, string[] parameters, TwilioProfileSettings profileSettings)
        {
            _logger.LogInformation("Sending WhatsApp congratulation. EventId={EventId}, GuestId={GuestId}, Phone={Phone}, TemplateId={TemplateId}",
                events.Id, guest.GuestId, fullPhoneNumber, templateId);

            try
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

                    _logger.LogInformation("Message sent successfully. GuestId={GuestId}, MessageSid={MessageSid}",
                        guest.GuestId, messageSid);
                }
                else
                {
                    _logger.LogWarning("Failed to send message (null SID). GuestId={GuestId}", guest.GuestId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending congratulation message. EventId={EventId}, GuestId={GuestId}",
                    events.Id, guest.GuestId);
            }
        }

        private async Task SendDefaultMessageToGuestAndUpdateGuest(List<Guest> guests, Events events, string? templateId, TwilioProfileSettings profileSettings)
        {
            _logger.LogInformation("SendDefaultMessageToGuestAndUpdateGuest started. EventId={EventId}, TemplateId={TemplateId}, GuestsCount={Count}",
                events.Id, templateId, guests.Count);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var conguratulationId = Guid.NewGuid().ToString();
                    var parameters = new string[]
                    {
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        conguratulationId,
                    };

                    _logger.LogDebug("Sending default thanks to GuestId={GuestId}, Phone={Phone}", guest.GuestId, fullPhoneNumber);

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

                        _logger.LogInformation("Message sent successfully. GuestId={GuestId}, MessageSid={MessageSid}",
                            guest.GuestId, messageSid);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send message (null SID). GuestId={GuestId}", guest.GuestId);
                    }

                    counter = UpdateCounter(guests, events, counter);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception sending default thanks to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("SendDefaultMessageToGuestAndUpdateGuest finished. EventId={EventId}", events.Id);
        }

        private async Task SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest(List<Guest> guests, Events events, string? templateId, TwilioProfileSettings profileSettings)
        {
            _logger.LogInformation("SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest started. EventId={EventId}, TemplateId={TemplateId}, GuestsCount={Count}",
                events.Id, templateId, guests.Count);

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                try
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    var conguratulationId = Guid.NewGuid().ToString();
                    var parameters = new string[]
                    {
                        GetCongratulationHeaderImage(events),
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        conguratulationId,
                    };

                    _logger.LogDebug("Sending default thanks with header image to GuestId={GuestId}, Phone={Phone}", guest.GuestId, fullPhoneNumber);

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

                        _logger.LogInformation("Message sent successfully. GuestId={GuestId}, MessageSid={MessageSid}",
                            guest.GuestId, messageSid);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send message (null SID). GuestId={GuestId}", guest.GuestId);
                    }

                    counter = UpdateCounter(guests, events, counter);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception sending default thanks with header image to GuestId={GuestId}", guest.GuestId);
                }
            });

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("SendDefaultMessageToGuestWithHeaderImageAndUpdateGuest finished. EventId={EventId}", events.Id);
        }

        private async Task SendMessageToOwnerAndUpdateGuest(List<Guest> guests, Events events, string message, string? templateId)
        {
            _logger.LogInformation("SendMessageToOwnerAndUpdateGuest started. EventId={EventId}, TemplateId={TemplateId}, GuestsCount={Count}",
                events.Id, templateId, guests.Count);

            var twilioProfile = await db.TwilioProfileSettings
                    .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

            if (twilioProfile == null)
            {
                _logger.LogWarning("Twilio profile settings not found for {ProfileName}", events.choosenSendingWhatsappProfile);
                return;
            }

            int counter = SetSendingCounter(guests, events);

            foreach (var guest in guests)
            {
                try
                {
                    var eventSentOnNum = events.ConguratulationsMsgSentOnNumber;
                    string fullPhoneNumber = $"+{eventSentOnNum}";
                    var parameters = new string[]
                    {
                        guest.FirstName.Trim(),
                        message,
                    };

                    _logger.LogDebug("Sending congratulation to owner for GuestId={GuestId}, OwnerPhone={Phone}",
                        guest.GuestId, fullPhoneNumber);

                    string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, 1, twilioProfile, events.choosenSendingCountryNumber);
                    if (messageSid != null)
                    {
                        guest.ConguratulationMsgCount = 0;
                        _logger.LogInformation("Message sent to owner successfully. GuestId={GuestId}, MessageSid={MessageSid}",
                            guest.GuestId, messageSid);
                    }
                    else
                    {
                        _logger.LogError("Failed to send message to owner (null SID). GuestId={GuestId}", guest.GuestId);
                        throw new Exception();
                    }

                    counter = UpdateCounter(guests, events, counter);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception sending congratulation to owner for GuestId={GuestId}", guest.GuestId);
                    throw;
                }
            }

            await updateDataBaseAndDisposeCache(guests, events);

            _logger.LogInformation("SendMessageToOwnerAndUpdateGuest finished. EventId={EventId}", events.Id);
        }

        private async Task updateDataBaseAndDisposeCache(List<Guest> guests, Events events)
        {
            _logger.LogInformation("Updating database for EventId={EventId}, GuestsCount={Count}", events.Id, guests.Count);

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

            _logger.LogInformation("Database updated and cache disposed for EventId={EventId}", events.Id);
        }

        #endregion
    }
}
