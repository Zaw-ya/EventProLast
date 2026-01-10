using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioGateKeeperMessageTemplates : TwilioMessagingConfiguration, IGateKeeperMessageTemplates
    {
        private readonly EventProContext db;
        public TwilioGateKeeperMessageTemplates(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService) : base(configuration,
                memoryCacheStoreService)
        {
            db = new EventProContext(configuration);
        }
        public async Task SendCheckInMessage(GKEventHistory gkEventHistory)
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
                                  .Where(e => e.Name == "EventProKuwait")
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = "HX4391d7fedb7c544cc903f752e6dfc43d";

            string fullPhoneNumber = _phoneNumber_to;
            var parameters = new string[]
            {
                  gkEventHistory.ImagePath,
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
                                  .Where(e => e.Name == "EventProKuwait")
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = "HX497de6568637d48ba9a0ac0053b1e49a";

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
                                  .Where(e => e.Name == "EventProKuwait")
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = "HXf53a5e31dab4d229c9570c10678ff182";

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

        private async Task SendMessageAndUpdateGuest(Events events, string? templateId, string fullPhoneNumber, string[] parameters, TwilioProfileSettings profileSettings)
        {
            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, 1, profileSettings, "SAUDI");
        }
    }
}
