using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Common;
using EventPro.DAL.Models;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
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
    /// <summary>
    /// Scheduled notification service for sending Firebase Cloud Messaging (FCM) push notifications
    /// and WhatsApp reminders to gatekeepers before upcoming events.
    ///
    /// Triggered by Hangfire recurring jobs configured in HangfireController.
    /// Schedule is controlled by appsettings:
    ///   - "NotificationTimeCronLocalTime": FCM push notifications (default: 06:00 AM daily)
    ///   - "WhatsMSGTimeCronLocalTime":     WhatsApp reminders     (default: 06:30 AM daily)
    /// </summary>
    public class NotificationTokenService : INotificationTokenService
    {
        private readonly EventProContext db;
        private readonly IConfiguration _Configuration;
        private readonly IGateKeeperMessageTemplates _gateKeeperMessageTemplates;

        public NotificationTokenService(IConfiguration configuration, IGateKeeperMessageTemplates gateKeeperMessageTemplates)
        {
            _Configuration = configuration;
            db = new EventProContext(_Configuration);
            _gateKeeperMessageTemplates = gateKeeperMessageTemplates;
        }

        #region Firebase FCM - Query Events & Device Tokens

        /// <summary>
        /// Queries upcoming events within the reminder period and collects
        /// gatekeeper device tokens for FCM multicast notifications.
        /// </summary>
        /// <param name="beforeDays">Number of days ahead to look for events (from AppSettings.GateKeeperReminderPeriodForEvent).</param>
        /// <returns>List of multicast notification requests with device tokens.</returns>
        public async Task<List<MessageRequestTokens>> GetEventsWithToken(int beforeDays)
        {
            try
            {
                DateTime date = DateTime.Now;
                DateTime toNotifyDate = date.AddDays(beforeDays);
                // Query events within the reminder period and select notification details along with distinct device tokens for each event
                var messageRequestTokens = await db.Events.Include(x => x.EventGatekeeperMapping).ThenInclude(x => x.Gatekeeper)
                              .Where(ev => ev.AttendanceTime.Value >= date && ev.AttendanceTime.Value <= toNotifyDate)
                              .Select(ev => new MessageRequestTokens()
                              {
                                  Title = "تنبيه: موعد المناسبة قريب",
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
        #endregion


        #region Firebase FCM - Send Notifications

        /// <summary>
        /// Sends a push notification to a single device token or topic via FCM.
        /// </summary>
        public async Task<bool> SendMessageDeviceOrTokenAsync(MessageRequest request)
        {
            FirbaseAPI firbaseAPI = new FirbaseAPI();
            return await firbaseAPI.NotifyTopicOrTokenAsync(request);
        }

        /// <summary>
        /// Scheduled task: Sends FCM multicast notifications to all gatekeepers
        /// assigned to upcoming events within the configured reminder period.
        /// Called by Hangfire recurring job.
        /// </summary>
        public async Task SendNotifyTokensAsync()
        {
            try
            {
                int beforeDays = await db.AppSettings.Select(e => e.GateKeeperReminderPeriodForEvent).FirstOrDefaultAsync();
                if (beforeDays > 0)
                {
                    // get gatekeeper device tokens for events within the reminder period to send FCM notifications
                    var requests = await GetEventsWithToken(beforeDays);
                    Log.Information($"Sending notification with Details:{requests.ToJson()}");
                    if (requests != null)
                    {
                        FirbaseAPI firbaseAPI = new FirbaseAPI();
                        foreach (var request in requests)
                        {
                            if (request.Tokens.Count > 0)
                                // Send multicast notification to all device tokens for the event
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
        #endregion

        #region WhatsApp - Query Gatekeeper Details

        /// <summary>
        /// Queries upcoming events and collects gatekeeper phone numbers for WhatsApp reminders.
        /// </summary>
        public async Task<List<GKWhatsRemiderMsgModel>> GetGKsWithEvent(int beforeDays) //5
        {
            try
            {
                DateTime date = DateTime.Now;
                DateTime sendToDate = date.AddDays(beforeDays);
                // Query events within the reminder period and select gatekeeper details for WhatsApp messages
                var GKWhatsRemiderMsgModels = await db.Events.Include(x => x.EventGatekeeperMapping).ThenInclude(x => x.Gatekeeper)
                              .Where(ev => ev.AttendanceTime.Value >= date && ev.AttendanceTime.Value <= sendToDate)
                              .Select(ev => new GKWhatsRemiderMsgModel()
                              {
                                  Title = "تنبيه: موعد المناسبة قريب",
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

        #endregion


        #region WhatsApp - Send Reminders

        /// <summary>
        /// Scheduled task: Sends WhatsApp reminder messages to gatekeepers for upcoming events.
        /// Called by Hangfire recurring job.
        /// </summary>
        public async Task SendWhatsMsgToGK()
        {
            try
            {
                // get reminder period from appsettings and query gatekeepers with events within that period
                int beforeDays = await db.AppSettings.Select(e => e.GateKeeperReminderPeriodForEvent).FirstOrDefaultAsync();
                if (beforeDays > 0)
                {
                    // get gatekeeper details for events within the reminder period to send WhatsApp messages
                    var gksWhatsRemiders = await GetGKsWithEvent(beforeDays);

                    Log.Information($"Sending Twilio WhatsApp Message to gk with Details:{gksWhatsRemiders.ToJson()}");
                    if (gksWhatsRemiders != null)
                    {
                        foreach (var gkWhatsRemider in gksWhatsRemiders)
                        {
                            // Check if the reminder is for today or a future date to determine the message content
                            if (gkWhatsRemider.AttendanceTime?.ToString("dd/MM/yyyy") != DateTime.Now.ToString("dd/MM/yyyy"))
                            {
                                await _gateKeeperMessageTemplates.SendGateKeeperReminderWhatsAppAsync(gkWhatsRemider);
                            }
                            // If the event is today, send a different message (e.g., "Event is today" instead of "Event is coming up")
                            else
                            {
                                await _gateKeeperMessageTemplates.SendGateKeeperTodayReminderWhatsAppAsync(gkWhatsRemider);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error occured in NotificationTokenService/SendWhatsMsgToGK,ex:{ex.Message}");
            }
        }


        #endregion
    }
}