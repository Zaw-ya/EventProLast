using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Globalization;
using System.Text.RegularExpressions;
namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioGateKeeperMessageTemplates : TwilioMessagingConfiguration, IGateKeeperMessageTemplates
    {
        private readonly EventProContext db;
        private readonly ILogger<TwilioMessagingConfiguration> _logger;
        public TwilioGateKeeperMessageTemplates(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService, ILogger<TwilioMessagingConfiguration> logger) : base(configuration,
                memoryCacheStoreService, logger)
        {
            db = new EventProContext(configuration);
            _logger = logger;
        }
        public async Task SendCheckInMessage(GKEventHistory gkEventHistory)
        {
            var gkUser = await db.Users.Where(gk => gk.UserId == gkEventHistory.GK_Id)
                         .FirstOrDefaultAsync();
            var _event = await db.Events.Where(e => e.Id == gkEventHistory.Event_Id)
                         .FirstOrDefaultAsync();
            string gmapCode = "https://maps.app.goo.gl/" + _event.GmapCode;
            string location = $"https://www.google.com/maps/search/?api=1&query={gkEventHistory.latitude},{gkEventHistory.longitude}";
            string _phoneNumber_to = await db.AppSettings.Select(e => e.GateKeeperCheckNotificationsNumber).FirstOrDefaultAsync();

            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == _event.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = "HX9991cf36445fd255ccf087f67990a061";

            string fullPhoneNumber = _phoneNumber_to;

            var parameters = new string[]
            {
                  GetrHeaderImage(gkEventHistory),
                 _event.SystemEventTitle,
                 _event.Id.ToString(),
                 gkUser.FirstName + gkUser.LastName,
                 gmapCode,
                 location,
                DateTime.Now.ToString("'Date: 'dd - MM yyyy 'Time: 'hh - mm - ss tt")
            };

            await SendMessageAndUpdateGuest(_event, templateId, fullPhoneNumber, parameters, profileSettings);

            return;
        }

        public async Task SendCheckOutMessage(GKEventHistory gkEventHistory)
        {
            var gkUser = await db.Users.Where(gk => gk.UserId == gkEventHistory.GK_Id)
                          .FirstOrDefaultAsync();
            var _event = await db.Events.Where(e => e.Id == gkEventHistory.Event_Id)
                         .FirstOrDefaultAsync();
            string gmapCode = "https://maps.app.goo.gl/" + _event.GmapCode;
            string location = $"https://www.google.com/maps/search/?api=1&query={gkEventHistory.latitude},{gkEventHistory.longitude}";
            string _phoneNumber_to = await db.AppSettings.Select(e => e.GateKeeperCheckNotificationsNumber)
                .FirstOrDefaultAsync();

            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == _event.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            var templateId = "HX1bb7e3aed171813184bab392b299096d";

            string fullPhoneNumber = _phoneNumber_to;
            var parameters = new string[]
            {
                 _event.SystemEventTitle,
                 _event.Id.ToString(),
                 gkUser.FirstName + gkUser.LastName,
                 gmapCode,
                 location,
                DateTime.Now.ToString("'Date: 'dd - MM yyyy 'Time: 'hh - mm - ss tt")
            };

            await SendMessageAndUpdateGuest(_event, templateId, fullPhoneNumber, parameters, profileSettings);

            return;
        }


        public async Task SendGateKeeperUnassignEventMessage(GKEventHistory gkEventHistory)
        {
            var gkUser = await db.Users.Where(gk => gk.UserId == gkEventHistory.GK_Id)
                                     .FirstOrDefaultAsync();
            var _event = await db.Events.Where(e => e.Id == gkEventHistory.Event_Id)
                         .FirstOrDefaultAsync();

            string _phoneNumber_to = await db.AppSettings.Select(e => e.GateKeeperCheckNotificationsNumber)
                .FirstOrDefaultAsync();

            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == _event.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            var templateId = "HX0674d54271e0abd6e854f266bf385f59";

            string fullPhoneNumber = _phoneNumber_to;
            var parameters = new string[]
            {
                 _event.SystemEventTitle,
                 _event.Id.ToString(),
                 gkUser.FirstName + gkUser.LastName,
                 gkUser.UserId.ToString(),
                 DateTime.Now.ToString("'Date: 'dd - MM yyyy 'Time: 'hh - mm - ss tt")
            };

            await SendMessageAndUpdateGuest(_event, templateId, fullPhoneNumber, parameters, profileSettings);

            return;
        }


        #region Gatekeeper WhatsApp Reminders

        // TODO: Replace these placeholder template IDs with actual ContentSid values from Twilio Console
        // after creating the templates below.
        private const string GK_REMINDER_UPCOMING_TEMPLATE_ID = "HX81a848431511bae683b274872e1a9981"; // gk_reminder_upcoming
        private const string GK_REMINDER_TODAY_TEMPLATE_ID = "HX7859fb3caa301d5e617e093d07e8632f";    // gk_reminder_today

        /// <summary>
        /// Sends a WhatsApp template reminder to gatekeepers for upcoming events (not today).
        /// Template placeholders: {{1}}=name, {{2}}=eventTitle, {{3}}=day, {{4}}=date, {{5}}=venue, {{6}}=time
        /// Uses "MyInviteTwilio" Twilio profile from DB.
        /// </summary>
        public async Task SendGateKeeperReminderWhatsAppAsync(GKWhatsRemiderMsgModel gkWhatsRemider)
        {
            var EventData = await db.Events.Where(e => e.Id == gkWhatsRemider.EventID)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == EventData.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var templateId = GK_REMINDER_UPCOMING_TEMPLATE_ID;

            try
            {
                await Parallel.ForEachAsync(gkWhatsRemider.GkDetails, async (gk, ct) =>
                {
                    if (string.IsNullOrEmpty(gk.GKPhoneNumber))
                        return;

                    var parameters = new string[]
                    {
                        gk.GKFName,                                                                  // {{1}} name
                        gkWhatsRemider.EventTitle,                                                   // {{2}} eventTitle
                        gkWhatsRemider.AttendanceTime?.ToString("dddd", new CultureInfo("ar-AE")),   // {{3}} day
                        gkWhatsRemider.AttendanceTime?.ToString("dd/MM/yyyy"),                        // {{4}} date
                        gkWhatsRemider.EventVenue,                                                   // {{5}} venue
                        gkWhatsRemider.AttendanceTime?.ToString("HH:mm")                             // {{6}} time
                    };

                    await SendWhatsAppTemplateMessageAsync(gk.GKPhoneNumber, templateId, parameters, EventData.CityId, EventData.ChoosenNumberWithinCountry, profileSettings, EventData.choosenSendingCountryNumber);
                    await Task.Delay(1000);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending GK upcoming reminder WhatsApp for EventID: {EventID}", gkWhatsRemider.EventID);
            }
        }

        /// <summary>
        /// Sends a WhatsApp template reminder to gatekeepers for events happening today.
        /// Template placeholders: {{1}}=name, {{2}}=eventTitle, {{3}}=venue, {{4}}=time
        /// Uses "MyInviteTwilio" Twilio profile from DB.
        /// </summary>
        public async Task SendGateKeeperTodayReminderWhatsAppAsync(GKWhatsRemiderMsgModel gkWhatsRemider)
        {

            var EventData = await db.Events.Where(e => e.Id == gkWhatsRemider.EventID)
            .AsNoTracking()
            .FirstOrDefaultAsync();

            var profileSettings = await db.TwilioProfileSettings
                .Where(e => e.Name == EventData.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();



            var templateId = GK_REMINDER_TODAY_TEMPLATE_ID;

            try
            {
                await Parallel.ForEachAsync(gkWhatsRemider.GkDetails, async (gk, ct) =>
                {
                    if (string.IsNullOrEmpty(gk.GKPhoneNumber))
                        return;

                    var parameters = new string[]
                    {
                        gk.GKFName,                                       // {{1}} name
                        gkWhatsRemider.EventTitle,                        // {{2}} eventTitle
                        gkWhatsRemider.EventVenue,                        // {{3}} venue
                        gkWhatsRemider.AttendanceTime?.ToString("HH:mm")  // {{4}} time
                    };

                    await SendWhatsAppTemplateMessageAsync(gk.GKPhoneNumber, templateId, parameters, EventData.CityId, EventData.ChoosenNumberWithinCountry, profileSettings, EventData.choosenSendingCountryNumber);
                    await Task.Delay(1000);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending GK today reminder WhatsApp for EventID: {EventID}", gkWhatsRemider.EventID);
            }
        }

        #endregion

        #region Private Helpers

        private async Task SendMessageAndUpdateGuest(Events events, string? templateId, string fullPhoneNumber, string[] parameters, TwilioProfileSettings profileSettings)
        {
            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, 1, profileSettings, "EGYPT");
        }

        private string GetrHeaderImage(GKEventHistory events)
        {
            var headerImage = events.ImagePath ?? string.Empty;
            if (!string.IsNullOrEmpty(headerImage))
            {
                headerImage = Regex.Replace(headerImage, @".*GatekeeperEventLocation/", "");
            }
            return headerImage;
        }
        #endregion
    }
}
