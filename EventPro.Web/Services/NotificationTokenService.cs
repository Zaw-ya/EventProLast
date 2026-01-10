using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Common;
using EventPro.DAL.Models;
using EventPro.Services.WatiService.Interface;
using EventPro.Web.Extensions;
using EventPro.Web.Services.Interface;
using NuGet.Protocol;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.Web.Services
{
    public class NotificationTokenService : INotificationTokenService
    {
        private readonly EventProContext db;
        private readonly IConfiguration _Configuration;
        private readonly IWatiService _watiService;
        public NotificationTokenService(IConfiguration configuration, IWatiService watiService)
        {
            _Configuration = configuration;
            db = new EventProContext(_Configuration);
            _watiService = watiService;
        }
        public async Task<List<MessageRequestTokens>> GetEventsWithToken(int beforeDays)
        {
            try
            {
                DateTime date = DateTime.Now;
                DateTime toNotifyDate = date.AddDays(beforeDays);
                var messageRequestTokens = await db.Events.Include(x => x.EventGatekeeperMapping).ThenInclude(x => x.Gatekeeper)
                              .Where(ev => ev.AttendanceTime.Value >= date && ev.AttendanceTime.Value <= toNotifyDate)
                              .Select(ev => new MessageRequestTokens()
                              {
                                  Title = $"???????: ???? ?????? ?????",
                                  Body = $"{ev.EventTitle} \nIn {ev.EventVenue} \nStarting at {ev.AttendanceTime}",
                                  EventID = ev.Id,
                                  Tokens = ev.EventGatekeeperMapping.Select(x => x.Gatekeeper.DeviceId).Distinct().ToList()
                              }).ToListAsync();
                return messageRequestTokens;
            }
            catch (Exception ex)
            {
                Log.Error($"Error occured in NotificationTokenService/GetEventsWithToken,ex:{ex.Message}");
                return null;
            }
        }
        public async Task<List<GKWhatsRemiderMsgModel>> GetGKsWithEvent(int beforeDays)
        {
            try
            {
                DateTime date = DateTime.Now;
                DateTime sendToDate = date.AddDays(beforeDays);
                var GKWhatsRemiderMsgModels = await db.Events.Include(x => x.EventGatekeeperMapping).ThenInclude(x => x.Gatekeeper)
                              .Where(ev => ev.AttendanceTime.Value >= date && ev.AttendanceTime.Value <= sendToDate)
                              .Select(ev => new GKWhatsRemiderMsgModel()
                              {
                                  Title = $"???????: ???? ?????? ?????",
                                  EventTitle = ev.EventTitle,
                                  EventVenue = ev.EventVenue,
                                  GkDetails = ev.EventGatekeeperMapping.Select(x => new GkDetailsWhatsRemiderMsg
                                  {
                                      GKPhoneNumber = x.Gatekeeper.PrimaryContactNo,
                                      GKFName = x.Gatekeeper.FirstName + " " + x.Gatekeeper.LastName
                                  }).ToList(),
                                  AttendanceTime = ev.AttendanceTime,
                                  EventID = ev.Id
                              }).ToListAsync();
                return GKWhatsRemiderMsgModels;
            }
            catch (Exception ex)
            {
                Log.Error($"Error occured in NotificationTokenService/GetGKsWithEvent,ex:{ex.Message}");
                return null;
            }
        }

        public async Task<bool> SendMessageDeviceOrTokenAsync(MessageRequest request)
        {
            FirbaseAPI firbaseAPI = new FirbaseAPI();
            return await firbaseAPI.NotifyTopicOrTokenAsync(request);
        }
        public async Task SendNotifyTokensAsync()
        {
            try
            {
                int beforeDays = await db.AppSettings.Select(e => e.GateKeeperReminderPeriodForEvent).FirstOrDefaultAsync();
                if (beforeDays > 0)
                {
                    var requests = await GetEventsWithToken(beforeDays);
                    Log.Information($"Sending notification with Details:{requests.ToJson()}");
                    if (requests != null)
                    {
                        FirbaseAPI firbaseAPI = new FirbaseAPI();
                        foreach (var request in requests)
                        {
                            if (request.Tokens.Count > 0)
                                await firbaseAPI.NotifyTokensAsync(request);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error occured in NotificationTokenService/SendNotifyTokensAsync,ex:{ex.Message}");
            }
        }
        public async Task SendWhatsMsgToGK()
        {
            try
            {
                int beforeDays = await db.AppSettings.Select(e => e.GateKeeperReminderPeriodForEvent).FirstOrDefaultAsync();
                if (beforeDays > 0)
                {
                    var gksWhatsRemiders = await GetGKsWithEvent(beforeDays);
                    Log.Information($"Sending Whatsapp Message to gk with Details:{gksWhatsRemiders.ToJson()}");
                    if (gksWhatsRemiders != null)
                    {

                        await gksWhatsRemiders.ForEachAsync(async gkWhatsRemider =>
                        {
                            await Task.Delay(2000);
                            if (gkWhatsRemider.AttendanceTime?.ToString("dd/MM/yyyy") != DateTime.Now.ToString("dd/MM/yyyy"))
                            {
                                await _watiService.SendGateKeeperReminderMessage(gkWhatsRemider);
                            }
                            else
                            {
                                await _watiService.SendGateKeeperTodayReminderMessage(gkWhatsRemider);
                            }

                        });

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error occured in NotificationTokenService/SendWhatsMsgToGK,ex:{ex.Message}");
            }
        }
    }
}