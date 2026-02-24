using EventPro.API.Models;
using EventPro.API.Services;
using EventPro.API.Services.WatiService.Interface;
using EventPro.Business.MemoryCacheStore.Implementaiion;
using EventPro.Business.Storage.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Common;
using EventPro.DAL.Dto;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.DAL.VMModels;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing events, gatekeeper operations, and location data.
    /// Handles event retrieval, calendar display, reservations, check-in/out, and geographic lookups.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        #region Private Fields

        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        private readonly ILogger<EventsController> _logger;
        private readonly IWhatsappSendingProviderService _whatsappSendingProvider;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly DistributedLockHelper _lockHelper;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the EventsController.
        /// </summary>
        /// <param name="configuration">Application configuration settings</param>
        /// <param name="logger">Logger for recording controller activities</param>
        /// <param name="whatsappSendingProvider">WhatsApp messaging provider service</param>
        /// <param name="blobStorage">Azure Blob storage service for file uploads</param>
        public EventsController(IConfiguration configuration, ILogger<EventsController> logger,
            IWhatsappSendingProviderService whatsappSendingProvider, ICloudinaryService cloudinaryService,
            DistributedLockHelper lockHelper)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
            _logger = logger;
            _cloudinaryService = cloudinaryService;
            _whatsappSendingProvider = whatsappSendingProvider;
            _lockHelper = lockHelper;
        }

        #endregion

        #region Event Retrieval Endpoints

        /// <summary>
        /// Gets paginated list of events with statistics for the authenticated gatekeeper.
        /// Uses stored procedure Proc_GetEventsStatsByGK for optimized data retrieval.
        /// </summary>
        /// <returns>Paginated list of events with statistics</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            UserToken userToken = new UserToken();
            var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
            if (check != null)
            {
                return check;
            }
            try
            {
                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);
                var outputpar = new SqlParameter("@NoOfPages", SqlDbType.Int) { Direction = ParameterDirection.Output };

                var eventsStatsByGK = db.Set<EventsStatsByGK>()
                              .FromSqlInterpolated($"exec Proc_GetEventsStatsByGK @GatekeeperId = {userToken?.UserId},@pageNo={pageNo},@PageSize={PageSize},@NoOfPages={outputpar} out")
                              .AsAsyncEnumerable();
                var result = new EntityListResult<EventsStatsByGK>
                { EntityList = await eventsStatsByGK.ToListAsync() };
                long noOfPages = 0;
                if (!string.IsNullOrEmpty(outputpar.Value.ToString()))
                    noOfPages = Convert.ToInt32(outputpar.Value.ToString());
                result.NoOfPages = noOfPages;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ErrorMessage = ex.Message,
                    FullMessage = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets available events for the calendar view.
        /// Returns events that are not assigned to any gatekeeper, are in the user's city,
        /// are marked to show on calendar, and are upcoming (within the last day).
        /// </summary>
        /// <returns>List of free events available for reservation</returns>
        [HttpGet("Calender")]
        public async Task<IActionResult> Calender()
        {
            UserToken userToken = new UserToken();
            var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
            if (check != null)
            {
                return check;
            }

            var cityId = await db.Users.Where(u => u.UserId == userToken.UserId).Select(u => u.CityId).FirstOrDefaultAsync();
            if (cityId == null)
            {
                return BadRequest("This account doesn't have address set correctly");
            }
            var freeEvents = await db.Events.Include(e => e.EventGatekeeperMapping)
                         .Where(e => !db.EventGatekeeperMapping.Select(m => m.EventId).Contains(e.Id)
                    && e.CityId == cityId
                    && e.ShowOnCalender == true
                    && e.EventFrom >= DateTime.Now.AddDays(-1)
                    && (e.IsDeleted != true))
                     .Select(e => new CalenderFreeEvents
                     {
                         Id = e.Id,
                         EventTitle = e.SystemEventTitle,
                         EventVenue = e.EventVenue,
                         EventFrom = e.EventFrom,
                         EventTo = e.EventTo,
                         ParentTitle = e.ParentTitle,
                         EventLocation = "https://maps.app.goo.gl/" + e.GmapCode
                     })
                .ToListAsync();

            return Ok(freeEvents);
        }

        #endregion

        #region Event Reservation Endpoints

        /// <summary>
        /// Reserves an event for the authenticated gatekeeper.
        /// Validates that the event is not already assigned and the gatekeeper
        /// is not already assigned to another event on the same date.
        /// </summary>
        /// <param name="eventId">The ID of the event to reserve</param>
        /// <returns>The created EventGatekeeperMapping record</returns>
        [HttpGet("ReserveEvent")]
        public async Task<IActionResult> ReserveEvent(int eventId)
        {
            UserToken userToken = new UserToken();
            var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
            if (check != null)
            {
                return check;
            }

            IActionResult result = null;

            var lockAcquired = await _lockHelper.RunWithLockAsync(
                resourceKey: $"reserve-event-{eventId}",
                action: async () =>
                {
                    // Only block if another gatekeeper has SELF-RESERVED (AssignedBy == GatekeeperId)
                    // Admin assignments (AssignedBy != GatekeeperId) are allowed in parallel
                    var alreadyReserved = await db.EventGatekeeperMapping
                        .Where(EM => EM.EventId == eventId && EM.GatekeeperId == EM.AssignedBy)
                        .FirstOrDefaultAsync();

                    if (alreadyReserved != null)
                    {
                        result = BadRequest("Event is already reserved by another gatekeeper.");
                        return;
                    }

                    var eventFrom = await db.Events
                        .Where(x => x.Id == eventId)
                        .Select(x => x.EventFrom)
                        .FirstOrDefaultAsync();

                    var assignedAtSameDate = await db.EventGatekeeperMapping
                        .Include(x => x.Event)
                        .Where(x => x.GatekeeperId == userToken.UserId &&
                                    x.Event.EventFrom.Value.Date == eventFrom.Value.Date)
                        .AnyAsync();

                    if (assignedAtSameDate)
                    {
                        result = BadRequest("Cannot assign to more than one event on the same day.");
                        return;
                    }

                    var egm = new EventGatekeeperMapping
                    {
                        EventId = eventId,
                        GatekeeperId = userToken.UserId,
                        AssignedBy = userToken.UserId,
                        AsssignedOn = DateTime.UtcNow,
                        IsActive = true
                    };

                    db.EventGatekeeperMapping.Add(egm);
                    await db.SaveChangesAsync();

                    try
                    {
                        var history = new GKEventHistory
                        {
                            CheckType = string.Empty,
                            Event_Id = eventId,
                            GK_Id = userToken.UserId,
                            LogDT = DateTime.Now,
                            longitude = string.Empty,
                            latitude = string.Empty
                        };

                        await _whatsappSendingProvider.SelectTwilioSendingProvider()
                            .GetGateKeeperMessageTemplates()
                            .SendGateKeeperassignEventMessage(history);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error sending WhatsApp message in ReserveEvent: {ex.Message}, GK_Id: {userToken?.UserId}");
                    }

                    result = Ok(egm);
                },
                expiry: TimeSpan.FromSeconds(10),
                wait: TimeSpan.FromSeconds(10),
                retry: TimeSpan.FromMilliseconds(200)
            );

            if (!lockAcquired)
                return BadRequest("Event is being reserved by another gatekeeper. Please try again.");

            return result;
        }

        /// <summary>
        /// Unassigns the authenticated gatekeeper from an event.
        /// Records the unassignment in the report table and sends WhatsApp notification.
        /// Only gatekeepers can unassign themselves from events.
        /// </summary>
        /// <param name="eventId">The ID of the event to unassign from</param>
        /// <returns>Success status</returns>
        [HttpGet("UnassignFromEvent")]
        public async Task<IActionResult> UnassignFromEvent(int eventId)
        {
            UserToken userToken = new UserToken();
            var headers = Request.Headers;
            var evntId = eventId;

            try
            {
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }
                if (userToken.Role != "GateKeeper")
                {
                    return BadRequest("Only Gatekeeper is allowed");
                }

                var userId = userToken.UserId;

                EventGatekeeperMapping egm = await db.EventGatekeeperMapping
                    .Where(p => p.GatekeeperId == userId && p.EventId == evntId)
                    .FirstOrDefaultAsync();

                if (egm == null) return BadRequest(new { success = false });

                db.EventGatekeeperMapping.Remove(egm);
                await db.SaveChangesAsync();


                var deletedRecord = new ReportDeletedEventsByGk
                {
                    EventId = eventId,
                    GatekeeperId = userId,
                    UnassignedOn = DateTime.UtcNow,
                    UnassignedById = userId,
                    UnassignedByName = userToken.UserName
                };

                await db.ReportDeletedEventsByGk.AddAsync(deletedRecord);
                await db.SaveChangesAsync();

                try
                {
                    var history = new GKEventHistory
                    {
                        CheckType = string.Empty,
                        Event_Id = eventId,
                        GK_Id = userToken.UserId,
                        LogDT = DateTime.Now,
                        longitude = string.Empty,
                        latitude = string.Empty
                    };

                    await _whatsappSendingProvider.SelectTwilioSendingProvider()
                             .GetGateKeeperMessageTemplates()
                             .SendGateKeeperUnassignEventMessage(history);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error occured while sending whatsapp message in UnassignFromEvent,: {ex.Message} ,GK_Id{userToken?.UserId}");
                }
                var events = await db.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();

                try
                {

                    if (events.ShowOnCalender == true)
                    {
                        // Build the notification payload targeting the city topic
                        var request = new MessageRequest()
                        {
                            Topic = $"{events.CityId}",
                            Title = events.EventTitle,
                            Body = $"New event is available now \nstarting in {events.EventVenue} at {events.AttendanceTime}"
                        };

                        var message = new Message()
                        {
                            Notification = new Notification
                            {
                                Title = request.Title,
                                Body = request.Body
                            },
                        };

                        // Route to topic (city) or direct device token
                        if (string.IsNullOrEmpty(request.Tokens))
                        {
                            message.Topic = request.Topic;
                        }
                        else
                        {
                            message.Token = request.Tokens;
                        }

                        // Fallback: initialize Firebase if not already done in Startup.cs
                        // Reads the JSON key filename from appsettings ("FireBaseJSON")
                        if (FirebaseApp.DefaultInstance == null)
                        {
                            var firebaseJson = _configuration["FireBaseJSON"];
                            FirebaseApp.Create(new AppOptions()
                            {
                                Credential = GoogleCredential.FromFile(
                                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, firebaseJson)),
                            });
                        }

                        // Send the notification via FCM V1 API
                        var messaging = FirebaseMessaging.DefaultInstance;
                        var result = await messaging.SendAsync(message);
                        _logger.LogInformation("Firebase FCM: Push notification sent for event {EventId} to topic {Topic}, result: {Result}", events.Id, request.Topic, result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Firebase FCM: Failed to send push notification for event {EventId}", events.Id);
                }


                return Ok("sucess removed from event");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in UnassignFromEvent,: {ex.Message} ,GK_Id{userToken?.UserId}");
                return BadRequest();
            }

        }

        #endregion

        #region Gatekeeper Check-In/Check-Out Endpoints

        /// <summary>
        /// Records a gatekeeper check-in for an event.
        /// Requires an image upload for verification. Captures GPS coordinates from headers.
        /// Sends WhatsApp notification upon successful check-in.
        /// Only gatekeepers can perform check-in operations.
        /// </summary>
        /// <param name="file">Image file for check-in verification</param>
        /// <param name="eventId">The ID of the event to check into</param>
        /// <returns>The created GKEventHistory record</returns>
        [HttpPost("CheckIn")]
        public async Task<IActionResult> CheckIn(IFormFile? file, int eventId)
        {
            UserToken userToken = new UserToken();
            var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
            var headers = Request.Headers;
            string longitude = headers.Where(p => p.Key == "longitude").Select(p => p.Value.ToString()).FirstOrDefault() ?? "0";
            string latitude = headers.Where(p => p.Key == "latitude").Select(p => p.Value.ToString()).FirstOrDefault() ?? "0";
            _logger.LogInformation($"CheckIn-eventId:{eventId},GK_Id:{userToken?.UserId},latitude:{latitude},longitude:{longitude}");

            try
            {
                _logger.LogInformation($"CheckIn-FileReceived:{file != null},eventId:{eventId},GK_Id:{userToken?.UserId}");
                if (file == null)
                {
                    return BadRequest("Picture isn't uploaded correctly.");
                }
                if (check != null)
                {
                    return check;
                }
                _logger.LogInformation($"CheckIn-UserAuthenticated:eventId:{eventId},GK_Id:{userToken?.UserId}");
                if (userToken.Role != "GateKeeper")
                {
                    return BadRequest("Only Gatekeeper is allowed");
                }

                var gkHistoryPath = _configuration.GetSection("GKHistoryPath").Value;
                string environment = _configuration.GetSection("environment").Value;
                //var filename = file.FileName;
                var extension = file.ContentType.ToLower().Replace(@"image/", "");
                using var stream = file.OpenReadStream();

                var fileName = $"{eventId}{Path.GetExtension(file.FileName)}";

                var imageUrl = await _cloudinaryService.UploadImageAsync(
                    stream,
                    fileName,
                    "GatekeeperEventLocation"
                );
                //await _blobStorage.UploadAsync(stream, extension, environment + gkHistoryPath + "/" + filename, cancellationToken: default);
                var history = new GKEventHistory
                {
                    CheckType = "In",
                    Event_Id = eventId,
                    GK_Id = userToken.UserId,
                    LogDT = DateTime.Now,
                    ImagePath = imageUrl,
                    longitude = longitude,
                    latitude = latitude
                };

                _logger.LogInformation(eventId + " " + userToken.UserId + " " + imageUrl);

                await db.GKEventHistory.AddAsync(history);
                await db.SaveChangesAsync();
                try
                {
                    await _whatsappSendingProvider.SelectTwilioSendingProvider()
                        .GetGateKeeperMessageTemplates()
                        .SendCheckInMessage(history);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error occured while sending whatsapp message in CheckIn,: {ex.Message} ,GK_Id{userToken?.UserId}");
                }


                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in CheckIn,: {ex.Message} ,GK_Id{userToken?.UserId}");
                return BadRequest(new { message = $"Error occured,please try again Later" });

            }
        }

        /// <summary>
        /// Records a gatekeeper check-out from an event.
        /// Validates that the gatekeeper has previously checked in.
        /// Captures GPS coordinates from headers and sends WhatsApp notification.
        /// Only gatekeepers can perform check-out operations.
        /// </summary>
        /// <param name="eventId">The ID of the event to check out from</param>
        /// <returns>The created GKEventHistory record</returns>
        [HttpGet("CheckOut")]
        public async Task<IActionResult> CheckOut(int eventId)
        {
            UserToken userToken = new UserToken();
            var headers = Request.Headers;
            string longitude = headers.Where(p => p.Key == "longitude").Select(p => p.Value.ToString()).FirstOrDefault() ?? "0";
            string latitude = headers.Where(p => p.Key == "latitude").Select(p => p.Value.ToString()).FirstOrDefault() ?? "0";
            _logger.LogInformation($"CheckOut-eventId:{eventId},GK_Id:{userToken?.UserId},latitude:{latitude},longitude:{longitude}");

            try
            {
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }
                if (userToken.Role != "GateKeeper")
                {
                    return BadRequest("Only Gatekeeper is allowed");
                }
                var oldCheckIn = await db.GKEventHistory
                    .FirstOrDefaultAsync(gk => gk.Event_Id == eventId && gk.GK_Id == userToken.UserId);
                if (oldCheckIn == null)
                {
                    return BadRequest("This gatekeeper didn't check IN yet to this event.");
                }

                var history = new GKEventHistory
                {
                    CheckType = "Out",
                    Event_Id = eventId,
                    GK_Id = userToken.UserId,
                    LogDT = DateTime.Now,
                    longitude = longitude,
                    latitude = latitude
                };

                await db.GKEventHistory.AddAsync(history);
                await db.SaveChangesAsync();
                var message = string.Empty;
                try
                {
                    await _whatsappSendingProvider.SelectTwilioSendingProvider()
                           .GetGateKeeperMessageTemplates()
                           .SendCheckOutMessage(history);

                }
                catch (Exception ex)
                {

                }

                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in checkout,: {ex.Message} ,GK_Id{userToken?.UserId}");
                return BadRequest(new { message = $"Error occured,please try again Later" });

            }
        }

        #endregion

        #region Location & Geographic Endpoints

        /// <summary>
        /// Gets all available event locations.
        /// Returns formatted location data excluding 'other' locations.
        /// This endpoint is publicly accessible without authentication.
        /// </summary>
        /// <returns>List of formatted event locations</returns>
        [AllowAnonymous]
        [HttpGet("Locations")]
        public async Task<IActionResult> Locations()
        {
            return Ok(await db.EventLocations.Where(l => l.City.ToLower() != "other").Select(l => l.GetLocationFormatted()).ToListAsync());
        }

        /// <summary>
        /// Gets all available countries.
        /// This endpoint is publicly accessible without authentication.
        /// </summary>
        /// <returns>List of countries with ID and name</returns>
        [AllowAnonymous]
        [HttpGet("GetCountries")]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await db.Country
                        .Select(c => new CountryDto { id = c.Id, CountryName = c.CountryName })
                        .ToListAsync();
            return Ok(countries);
        }

        /// <summary>
        /// Gets all cities for a specific country.
        /// This endpoint is publicly accessible without authentication.
        /// </summary>
        /// <param name="countryId">The ID of the country to get cities for</param>
        /// <returns>List of cities with ID and name</returns>
        [AllowAnonymous]
        [HttpGet("GetCities/{countryId:int}")]
        public async Task<IActionResult> GetCities(int countryId)
        {
            var cities = await db.City.Where(c => c.CountryId == countryId)
                  .Select(c => new CityDto() { id = c.Id, CityName = c.CityName }).ToListAsync();
            return Ok(cities);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Builds WhatsApp message body for check-in/check-out notifications.
        /// Creates a PinacleBody object with gatekeeper and event details.
        /// </summary>
        /// <param name="history">The GKEventHistory record containing check details</param>
        /// <param name="template">The WhatsApp template ID to use</param>
        /// <returns>Configured PinacleBody for WhatsApp API</returns>
        private async Task<PinacleBody> SendCheck(GKEventHistory history, string template)
        {
            var gkUser = await db.Users.Where(gk => gk.UserId == history.GK_Id).FirstOrDefaultAsync();
            var _event = await db.Events.Where(e => e.Id == history.Event_Id).FirstOrDefaultAsync();
            string location = $"https://www.google.com/maps/search/?api=1&query={history.latitude},{history.longitude}";
            string _phoneNumber_to = await db.AppSettings.Select(e => e.GateKeeperCheckNotificationsNumber).FirstOrDefaultAsync();
            PinacleMessage msg = new PinacleMessage
            {
                templateid = template,
                placeholders = new string[]
                {
                    gkUser.FirstName+" "+ gkUser.LastName,
                    _event.Id.ToString(),
                    _event.SystemEventTitle ?? "Title",
                    _event.EventVenue ?? "Venue",
                    _event.EventLocation ?? "Location",
                    location,
                    DateTime.Now.ToString("tt h:mm yyyy/MM/dd", new CultureInfo("ar-EG"))
                },
            };
            PinacleBody ikart = new PinacleBody
            {
                from = "966582991745",
                to = _phoneNumber_to,
                message = msg,
                type = "template",
                gotomodule = 0
            };
            return ikart;
        }

        /// <summary>
        /// Builds WhatsApp media message body for sending images.
        /// Creates a PinacleMediaBody object for image attachments.
        /// </summary>
        /// <param name="imgUrl">The URL of the image to send</param>
        /// <returns>Configured PinacleMediaBody for WhatsApp API</returns>
        private async Task<PinacleMediaBody> SendImage(string imgUrl)
        {
            string _fromNo = "966582991745";
            string _phoneNumber_to = await db.AppSettings.Select(e => e.GateKeeperCheckNotificationsNumber).FirstOrDefaultAsync();
            var templateId = _configuration.GetSection("CheckIn_img_Template").Value;
            PinacleMediaMessage msg = new PinacleMediaMessage
            {
                templateid = templateId,
                url = imgUrl
            };
            PinacleMediaBody ikart = new PinacleMediaBody
            {
                from = _fromNo,
                to = _phoneNumber_to,
                message = msg,
                type = "template"
            };
            return ikart;
        }

        #endregion

        #region Test/Debug Endpoints

        /// <summary>
        /// Test endpoint for verifying GKEventHistory table operations.
        /// Adds a sample record to the database for testing purposes.
        /// </summary>
        /// <returns>Success status</returns>
        //[AllowAnonymous]
        [HttpGet("TestTable")]
        public async Task<IActionResult> TestNewTable()
        {
            await db.GKEventHistory.AddAsync(new GKEventHistory
            {
                CheckType = "In",
                Event_Id = 2,
                GK_Id = 3,
                ImagePath = "",
                LogDT = DateTime.Now,
            });
            await db.SaveChangesAsync();
            return Ok();
        }

        #endregion
    }
}
