using Hangfire;
using Microsoft.AspNetCore.Mvc;
using EventPro.DAL.Models;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class HangfireController
    {
        private IBackgroundJobClient _backgroundJobClient;
        private IRecurringJobManager _recurringJobManager;
        private INotificationTokenService _NotificationTokenService;
        private readonly IGateKeeperMessageTemplates _gateKeeperMessageTemplates;
        public HangfireController(IBackgroundJobClient backgroundJobClient, IGateKeeperMessageTemplates gateKeeperMessageTemplates,
            IRecurringJobManager recurringJobManager, INotificationTokenService notificationTokenService)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
            _NotificationTokenService = notificationTokenService;
            _gateKeeperMessageTemplates = gateKeeperMessageTemplates;
        }

        [HttpGet]
        [Route("FireAndForgetJob")]
        public bool CreateFireAndForgetJob()
        {
            string x = string.Empty;
            _backgroundJobClient.Enqueue(() => Console.WriteLine("test"));
            return true;
        }

        [HttpGet]
        [Route("DelayedJob")]
        public bool CreateDelayedJob()
        {
            _backgroundJobClient.Schedule(() => Console.WriteLine("test"), TimeSpan.FromSeconds(1000000));
            return true;
        }


        [HttpGet]
        [Route("ReccuringJob")]
        public bool CreateReccuringJob()
        {
            _NotificationTokenService.SendNotifyTokensAsync();
            //_recurringJobManager.AddOrUpdate("NotifyBeforeEvent4", () => _NotificationTokenService.SendNotifyTokensAsync(), Cron.Daily(00, 1));
            return true;
        }

        [HttpGet]
        [Route("ContinuationJob")]
        public bool CreateContinuationJob()
        {
            var jobId = _backgroundJobClient.Schedule(() => Console.WriteLine("testContinuationJob"), TimeSpan.FromSeconds(45));
            return true;
        }

        [HttpPost]
        [Route("SendWhatsMsg")]
        public async Task<bool> SendWhatsMsg([FromBody] string gKPhoneNumber)
        {
            var gkmodel = new GkDetailsWhatsRemiderMsg() { GKFName = "sherief Mo", GKPhoneNumber = gKPhoneNumber };
            var gkDetails = new List<GkDetailsWhatsRemiderMsg>();
            gkDetails.Add(gkmodel);
            var model = new GKWhatsRemiderMsgModel()
            {
                AttendanceTime = DateTime.Now,
                GkDetails = gkDetails,
                EventTitle = "حفل ",
                EventVenue = "الرياض الحمراء",
                Title = "تذكير"
            };
            await _gateKeeperMessageTemplates.SendGateKeeperReminderWhatsAppAsync(model);
            return true;
        }
    }
}
