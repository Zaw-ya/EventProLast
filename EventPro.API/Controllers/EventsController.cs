using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EventPro.API.Models;
using EventPro.API.Services;
using EventPro.API.Services.WatiService.Interface;
using EventPro.Business.Storage.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Common;
using EventPro.DAL.Dto;
using EventPro.DAL.Models;
using EventPro.DAL.VMModels;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        private readonly ILogger<EventsController> _logger;
        private readonly IWatiService _watiService;
        private readonly IWhatsappSendingProviderService _whatsappSendingProvider;
        private readonly IBlobStorage _blobStorage;


        public EventsController(IConfiguration configuration, ILogger<EventsController> logger, IWatiService watiService,
            IWhatsappSendingProviderService whatsappSendingProvider,
            IBlobStorage blobStorage)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
            _logger = logger;
            _watiService = watiService;
            _whatsappSendingProvider = whatsappSendingProvider;
            _blobStorage = blobStorage;
        }

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

        [HttpGet("ReserveEvent")]
        public async Task<IActionResult> ReserveEvent(int eventId)
        {
            UserToken userToken = new UserToken();
            var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
            if (check != null)
            {
                return check;
            }

            var assignedKeeper = await db.EventGatekeeperMapping
                                 .Where(EM => EM.EventId == eventId)
                                 .FirstOrDefaultAsync();

            if (assignedKeeper != null)
            {
                return BadRequest("Event is already assigned to before. Refer to Administrator.");
            }

            var eventFrom = await db.Events.Where(x => x.Id == eventId).Select(x => x.EventFrom).FirstOrDefaultAsync();
            var assignedAtSameDate = await db.EventGatekeeperMapping.Include(x => x.Event)
                                           .Where(x => x.GatekeeperId == userToken.UserId &&
                                                   x.Event.EventFrom.Value.Date.Equals(eventFrom.Value.Date))
                                       .AnyAsync();
            if (assignedAtSameDate)
            {
                return BadRequest("Can not assign to more than one event on the same day.");
            }
            EventGatekeeperMapping egm = new EventGatekeeperMapping
            {
                EventId = eventId,
                GatekeeperId = userToken.UserId,
                AssignedBy = 1,
                AsssignedOn = DateTime.UtcNow,
                IsActive = true
            };
            db.EventGatekeeperMapping.Add(egm);
            db.SaveChanges();
            return Ok(egm);
        }

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

        [HttpPost("CheckIn")]
        public async Task<IActionResult> CheckIn(IFormFile? file, int eventId)
        {
            UserToken userToken = new UserToken();
            var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
            var headers = Request.Headers;
            string longitude = headers.Where(p => p.Key == "longitude").Select(p => p.Value).FirstOrDefault();
            string latitude = headers.Where(p => p.Key == "latitude").Select(p => p.Value).FirstOrDefault();
            _logger.LogInformation($"CheckIn-eventId:{eventId},GK_Id:{userToken?.UserId},latitude:{latitude},longitude:{longitude}");

            try
            {
                if (file == null)
                {
                    return BadRequest("Picture isn't uploaded correctly.");
                }
                if (check != null)
                {
                    return check;
                }
                if (userToken.Role != "Gatekeeper")
                {
                    return BadRequest("Only Gatekeeper is allowed");
                }

                var gkHistoryPath = _configuration.GetSection("GKHistoryPath").Value;
                string environment = _configuration.GetSection("environment").Value;
                var filename = file.FileName;
                var extension = file.ContentType.ToLower().Replace(@"image/", "");
                using var stream = file.OpenReadStream();
                await _blobStorage.UploadAsync(stream, extension, environment + gkHistoryPath + "/" + filename, cancellationToken: default);
                var history = new GKEventHistory
                {
                    CheckType = "In",
                    Event_Id = eventId,
                    GK_Id = userToken.UserId,
                    LogDT = DateTime.Now,
                    ImagePath = filename,
                    longitude = longitude,
                    latitude = latitude
                };

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

                }


                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in CheckIn,: {ex.Message} ,GK_Id{userToken?.UserId}");
                return BadRequest(new { message = $"Error occured,please try again Later" });

            }
        }

        [HttpGet("CheckOut")]
        public async Task<IActionResult> CheckOut(int eventId)
        {
            UserToken userToken = new UserToken();
            var headers = Request.Headers;
            string longitude = headers.Where(p => p.Key == "longitude").Select(p => p.Value).FirstOrDefault();
            string latitude = headers.Where(p => p.Key == "latitude").Select(p => p.Value).FirstOrDefault();
            _logger.LogInformation($"CheckOut-eventId:{eventId},GK_Id:{userToken?.UserId},latitude:{latitude},longitude:{longitude}");

            try
            {
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }
                if (userToken.Role != "Gatekeeper")
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

        [AllowAnonymous]
        [HttpGet("Locations")]
        public async Task<IActionResult> Locations()
        {
            return Ok(await db.EventLocations.Where(l => l.City.ToLower() != "other").Select(l => l.GetLocationFormatted()).ToListAsync());
        }

        [AllowAnonymous]
        [HttpGet("GetCountries")]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await db.Country
                        .Select(c => new CountryDto { id = c.Id, CountryName = c.CountryName })
                        .ToListAsync();
            return Ok(countries);
        }

        [AllowAnonymous]
        [HttpGet("GetCities/{countryId:int}")]
        public async Task<IActionResult> GetCities(int countryId)
        {
            var cities = await db.City.Where(c => c.CountryId == countryId)
                  .Select(c => new CityDto() { id = c.Id, CityName = c.CityName }).ToListAsync();
            return Ok(cities);
        }

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
                if (userToken.Role != "Gatekeeper")
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

                }

                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }
    }
}
