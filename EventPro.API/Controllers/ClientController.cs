using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.API.Models;
using EventPro.API.Services;
using EventPro.DAL.Dto;
using EventPro.DAL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;

        public ClientController(IConfiguration configuration)
        {
            db = new EventProContext(configuration);
            _configuration = configuration;
        }

        [HttpGet("index")]
        public IActionResult Index()
        {
            return Ok();
        }

        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            UserToken userToken = new UserToken();
            var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
            if (check != null)
            {
                return check;
            }

            UserDto user = await db.Users.Include(u => u.City).ThenInclude(c => c.Country)
                .Where(u => u.UserId == userToken.UserId)
                .Select(u => new UserDto()
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Email = u.Email,
                    Gender = u.Gender,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Address = u.City.Country.CountryName + " | " + u.City.CityName,
                    CityId = u.CityId,
                    PrimaryContactNo = u.PrimaryContactNo,
                    Role = u.Role
                }).FirstOrDefaultAsync();
            return Ok(user);
        }

        [HttpGet("myevents")]
        public async Task<IActionResult> MyEvents()
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }
                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);


                IQueryable<ClientEventsModel> clientEvents =
                                             db.Events.Where(e => e.CreatedFor == userToken.UserId && (e.IsDeleted == false || e.IsDeleted == null))
                                            .OrderByDescending(p => p.EventFrom)
                                            .Select(e => new ClientEventsModel()
                                            {
                                                Id = e.Id,
                                                EventTitle = e.SystemEventTitle,
                                                EventFrom = e.EventFrom,
                                                EventTo = e.EventTo,
                                                EventVenue = e.EventVenue
                                            });

                long count = 0;
                if (pageNo == 1)
                    count = await clientEvents.LongCountAsync();
                clientEvents = clientEvents.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize);
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<ClientEventsModel>
                { EntityList = await clientEvents.ToListAsync(), NoOfPages = noOfPages };

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

        [HttpGet("geteventclient")]
        public async Task<IActionResult> GetEventClient(int eventId)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }

                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<EventScannInfoModel> eventScannInfo =
                                             db.Guest.Include(g => g.ScanHistory)
                                            .Include(g => g.Event)
                                            .Where(g => g.EventId == eventId && (g.Event.IsDeleted == false || g.Event.IsDeleted == null))
                                            .OrderByDescending(g => g.FirstName)
                                            .Select(g => new EventScannInfoModel()
                                            {
                                                GuestId = g.GuestId,
                                                GuestName = g.FirstName + " " + g.LastName,
                                                Scanned = g.ScanHistory.Count(sh => sh.ResponseCode == "Allowed"),
                                                NoOfMembers = g.NoOfMembers,
                                                Response = g.Response,
                                                WhatsappStatus = g.WhatsappStatus
                                            });

                long count = 0;
                if (pageNo == 1)
                    count = await eventScannInfo.LongCountAsync();
                eventScannInfo = eventScannInfo.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize);
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<EventScannInfoModel>
                { EntityList = await eventScannInfo.ToListAsync(), NoOfPages = noOfPages };

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

        [HttpGet("getguestmessagestatus")]
        public async Task<IActionResult> GetGuestMessageStatus(int eventId)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }

                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = db.VwScannedInfo.Where(p => p.EventId == eventId);

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("searchguestmessagestatus")]
        public async Task<IActionResult> searchGuestMessageStatus(int eventId, string searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }


                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = db.VwScannedInfo.Where(p => p.EventId == eventId &&
                (p.FirstName.Contains(searchValue) ||
                ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                .Contains(searchValue)));

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getacceptedguests")]
        public async Task<IActionResult> GetAcceptedGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }


                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {

                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                        (p.Response.Equals("Confirm") ||
                       (p.Response.Equals("???? ??????"))) &&
                        (p.FirstName.Contains(searchValue) ||
                   ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                   .Contains(searchValue)));
                }
                else
                {
                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                        (p.Response.Equals("Confirm") ||
                       (p.Response.Equals("???? ??????"))));
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getdeclinedguests")]
        public async Task<IActionResult> GetDeclinedGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }


                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {

                    if (searchValue.First() == '0')
                    {
                        searchValue = searchValue.Substring(1);
                    }

                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                        (p.Response.Equals("Decline") ||
                        (p.Response.Equals("???????? ?? ??????"))) &&
                        (p.FirstName.Contains(searchValue) ||
                   ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                   .Contains(searchValue)));
                }
                else
                {
                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                       (p.Response.Equals("Decline") ||
                        (p.Response.Equals("???????? ?? ??????"))));
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getnoanswerguests")]
        public async Task<IActionResult> GetNoAnswerGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }


                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {

                    if (searchValue.First() == '0')
                    {
                        searchValue = searchValue.Substring(1);
                    }

                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                        (p.Response.Equals("Message Processed Successfully"))
                && ((p.TextFailed != true)
                && ((p.TextDelivered == true)
                || (p.TextRead == true)
                || !(p.TextSent == true && p.whatsappMessageId != null)))
                 &&
                        (p.FirstName.Contains(searchValue) ||
                   ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                   .Contains(searchValue)));
                }
                else
                {
                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                       (p.Response.Equals("Message Processed Successfully"))
                && ((p.TextFailed != true)
                && ((p.TextDelivered == true)
                || (p.TextRead == true)
                || !(p.TextSent == true && p.whatsappMessageId != null)))
                );
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getattendedguests")]
        public async Task<IActionResult> GetAttendedGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }


                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {
                    if (searchValue.First() == '0')
                    {
                        searchValue = searchValue.Substring(1);
                    }

                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                        (p.ScanId != 0 && p.ScanId != null) &&
                        (p.FirstName.Contains(searchValue) ||
                   ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                   .Contains(searchValue)));
                }
                else
                {
                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                       (p.ScanId != 0 && p.ScanId != null));
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getnotsentguests")]
        public async Task<IActionResult> GetNotSentGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }


                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {

                    if (searchValue.First() == '0')
                    {
                        searchValue = searchValue.Substring(1);
                    }

                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                        (p.MessageId == null) &&
                        (p.FirstName.Contains(searchValue) ||
                   ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                   .Contains(searchValue)));
                }
                else
                {
                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                       (p.MessageId == null));
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getfailedguests")]
        public async Task<IActionResult> GetFailedGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }



                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {

                    if (searchValue.First() == '0')
                    {
                        searchValue = searchValue.Substring(1);
                    }

                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                 p.Response.Equals("Message Processed Successfully")
                && (p.TextDelivered != true)
                && (p.TextRead != true)
                && ((p.TextSent == true && p.whatsappMessageId != null) || p.TextFailed == true)
                 &&
                        (p.FirstName.Contains(searchValue) ||
                   ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                   .Contains(searchValue)));
                }
                else
                {



                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                        p.Response.Equals("Message Processed Successfully")
                && (p.TextDelivered != true)
                && (p.TextRead != true)
                && ((p.TextSent == true && p.whatsappMessageId != null) || p.TextFailed == true)
                );
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getguestconfirmationstatisticsinfo")]
        public async Task<IActionResult> GetGuestConfirmationStatisticsInfo(int eventId)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }

                IQueryable<VwScannedInfo> guests = db.VwScannedInfo.Where(p => p.EventId == eventId);

                if (guests == null)
                {
                    return Ok(
                        new EventConfirmationMessagesStatistics
                        {
                            TotalGuestsNumber = 0,
                            AcceptedGuestsNumber = 0,
                            DeclienedGuestsNumber = 0,
                            NoAnswerGuestsNumber = 0,
                            AttendedGuestsNumber = 0,
                            FailedGuestsNumber = 0,
                            NotSentGuestsNumber = 0
                        }
                    );
                }

                var evnt = await db.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();

                if (evnt.WhatsappConfirmation != true)
                {
                    return Ok(
                       new EventConfirmationMessagesStatistics
                       {
                           TotalGuestsNumber = 0,
                           AcceptedGuestsNumber = 0,
                           DeclienedGuestsNumber = 0,
                           NoAnswerGuestsNumber = 0,
                           AttendedGuestsNumber = 0,
                           FailedGuestsNumber = 0,
                           NotSentGuestsNumber = 0
                       }
                   );
                }

                var totalGuestsNumber = guests.Sum(e => e.NoOfMembers);


                var acceptedGuestsNumber = guests.Where(p =>
                        (p.Response.Equals("Confirm") ||
                        (p.Response.Equals("???? ??????"))))
                    .Sum(e => e.NoOfMembers);

                var declinedGuestNumber = guests.Where(p =>
                        (p.Response.Equals("Decline") ||
                        (p.Response.Equals("???????? ?? ??????"))))
                    .Sum(e => e.NoOfMembers);

                var noAnswerGuestsNumber = guests.Where(p =>
                (p.Response.Equals("Message Processed Successfully"))
                && ((p.TextFailed != true)
                && ((p.TextDelivered == true)
                || (p.TextRead == true)
                || !(p.TextSent == true && p.whatsappMessageId != null)))
                ).Sum(e => e.NoOfMembers);

                var failedGuestsNumber = guests.Where(p =>
                p.Response.Equals("Message Processed Successfully")
                && (p.TextDelivered != true)
                && (p.TextRead != true)
                && ((p.TextSent == true && p.whatsappMessageId != null) || p.TextFailed == true)
                ).Sum(e => e.NoOfMembers);

                var notSentGuestsNumber = guests.Where(p =>
                (p.MessageId == null
                )).Sum(e => e.NoOfMembers);

                var attendedGuestsNumber = guests.Where(p =>
                    (p.ScanId != 0 && p.ScanId != null))
                    .Sum(e => e.NoOfMembers);


                var result = new EventConfirmationMessagesStatistics
                {
                    TotalGuestsNumber = totalGuestsNumber ?? 0,
                    AcceptedGuestsNumber = acceptedGuestsNumber ?? 0,
                    DeclienedGuestsNumber = declinedGuestNumber ?? 0,
                    NoAnswerGuestsNumber = noAnswerGuestsNumber ?? 0,
                    AttendedGuestsNumber = attendedGuestsNumber ?? 0,
                    FailedGuestsNumber = failedGuestsNumber ?? 0,
                    NotSentGuestsNumber = notSentGuestsNumber ?? 0
                };

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

        [HttpGet("getguestcardstatisticsinfo")]
        public async Task<IActionResult> GetGuestCardStatisticsInfo(int eventId)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }

                IQueryable<VwScannedInfo> guests = db.VwScannedInfo.Where(p => p.EventId == eventId);

                if (guests == null)
                {
                    return Ok(
                        new EventCardMessagesStatistics
                        {
                            TotalGuestsNumber = 0,
                            DeliveredGuestsNumber = 0,
                            FailedGuestsNumber = 0,
                            NotSentGuestsNumber = 0,
                            AttendedGuestsNumber = 0
                        }
                    );
                }

                var evnt = await db.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();

                if (evnt.WhatsappPush != true)
                {
                    return Ok(
                       new EventCardMessagesStatistics
                       {
                           TotalGuestsNumber = 0,
                           DeliveredGuestsNumber = 0,
                           FailedGuestsNumber = 0,
                           NotSentGuestsNumber = 0,
                           AttendedGuestsNumber = 0
                       }
                   );
                }

                var totalGuestsNumber = guests.Sum(e => e.NoOfMembers);

                var deliveredGuestsNumber = guests.Where(p =>
                          p.ImgRead == true ||
                          p.ImgDelivered == true ||
                         (p.ImgSent == true && p.whatsappMessageImgId == null && p.ImgFailed != true) ||
                         (p.ImgSent == true && p.whatsappMessageImgId != null && (p.TextDelivered == true || p.TextRead == true)))
                        .Sum(e => e.NoOfMembers);



                var failedGuestsNumber = guests.Where(p =>
                (p.ImgRead != true)
                && (p.ImgDelivered != true)
                && ((p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true) || p.ImgFailed == true)
                ).Sum(e => e.NoOfMembers);

                var notSentGuestsNumber = guests.Where(p =>
                (p.ImgSentMsgId == null
                )).Sum(e => e.NoOfMembers);

                var attendedGuestsNumber = guests.Where(p =>
                    (p.ScanId != 0 && p.ScanId != null))
                    .Sum(e => e.NoOfMembers);

                var result = new EventCardMessagesStatistics
                {
                    TotalGuestsNumber = totalGuestsNumber ?? 0,
                    DeliveredGuestsNumber = deliveredGuestsNumber ?? 0,
                    FailedGuestsNumber = failedGuestsNumber ?? 0,
                    NotSentGuestsNumber = notSentGuestsNumber ?? 0,
                    AttendedGuestsNumber = attendedGuestsNumber ?? 0
                };

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


        [HttpGet("getreadcardguests")]
        public async Task<IActionResult> GetReadCardGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }

                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {

                    if (searchValue.First() == '0')
                    {
                        searchValue = searchValue.Substring(1);
                    }

                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                         p.ImgRead == true
                           &&
                        (p.FirstName.Contains(searchValue) ||
                   ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                   .Contains(searchValue)));
                }
                else
                {



                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                        p.ImgRead == true
                          );
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }


                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getdeliverdcardguests")]
        public async Task<IActionResult> GetDeliverdCardGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }



                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {

                    if (searchValue.First() == '0')
                    {
                        searchValue = searchValue.Substring(1);
                    }

                    guests = db.VwScannedInfo
                                .Where(p =>
                                p.EventId == eventId &&
                                (
                                 p.ImgRead == true ||
                                 p.ImgDelivered == true ||
                                (p.ImgSent == true && p.whatsappMessageImgId == null && p.ImgFailed != true) ||
                                (p.ImgSent == true && p.whatsappMessageImgId != null && (p.TextDelivered == true || p.TextRead == true))
                               ) &&
                                (
                                 p.FirstName.Contains(searchValue) ||
                                ("+" + p.SecondaryContactNo + p.PrimaryContactNo).Contains(searchValue)
                                 )
                                 );
                }
                else
                {
                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId && (
                          p.ImgRead == true ||
                          p.ImgDelivered == true ||
                         (p.ImgSent == true && p.whatsappMessageImgId == null && p.ImgFailed != true) ||
                         (p.ImgSent == true && p.whatsappMessageImgId != null && (p.TextDelivered == true || p.TextRead == true))
                          ));
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();

                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getsentcardguests")]
        public async Task<IActionResult> GetSentCardGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }



                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {

                    if (searchValue.First() == '0')
                    {
                        searchValue = searchValue.Substring(1);
                    }

                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                  p.ImgFailed != true &&
                  p.ImgDelivered != true &&
                  p.ImgRead != true &&
                  p.ImgSent == true &&
                  !(p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true)
                 &&
                        (p.FirstName.Contains(searchValue) ||
                   ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                   .Contains(searchValue)));
                }
                else
                {



                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                  p.ImgFailed != true &&
                  p.ImgDelivered != true &&
                  p.ImgRead != true &&
                  p.ImgSent == true &&
                  !(p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true)
                );
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getfailedcardguests")]
        public async Task<IActionResult> GetFailedCardGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }



                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {

                    if (searchValue.First() == '0')
                    {
                        searchValue = searchValue.Substring(1);
                    }

                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                (p.ImgRead != true)
                && (p.ImgDelivered != true)
                && ((p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true) || p.ImgFailed == true)
                 &&
                        (p.FirstName.Contains(searchValue) ||
                   ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                   .Contains(searchValue)));
                }
                else
                {



                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                (p.ImgRead != true)
                && (p.ImgDelivered != true)
                && ((p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true) || p.ImgFailed == true)
                );
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getnotsentcardGuests")]
        public async Task<IActionResult> GetNotSentCardGuests(int eventId, string? searchValue)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }

                var headers = Request.Headers;
                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);

                IQueryable<VwScannedInfo> guests = null;

                if (!string.IsNullOrEmpty(searchValue))
                {

                    if (searchValue.First() == '0')
                    {
                        searchValue = searchValue.Substring(1);
                    }

                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                (p.ImgSentMsgId == null
                )
                 &&
                        (p.FirstName.Contains(searchValue) ||
                   ("+" + p.SecondaryContactNo + p.PrimaryContactNo)
                   .Contains(searchValue)));
                }
                else
                {



                    guests = db.VwScannedInfo
                        .Where(p => p.EventId == eventId &&
                (p.ImgSentMsgId == null
                )
                );
                }

                long count = await guests.CountAsync();
                guests = guests.OrderByDescending(e => e.GuestId);

                List<VwScannedInfo> guestsStatus;

                if (count == 0)
                {
                    return Ok(
                        new EntityListResult<VwScannedInfo>
                        {
                            EntityList = null,
                            NoOfPages = 0
                        }
                    );
                }

                guestsStatus = guests.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize).ToList();
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<VwScannedInfo>
                { EntityList = guestsStatus.ToList(), NoOfPages = noOfPages };

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

        [HttpGet("getmessagesstatisticsinfo")]
        public IActionResult GetMessagesStatisticsInfo(int eventId)
        {
            try
            {
                UserToken userToken = new UserToken();
                var check = UserFromHeaders.BadToken(Request.Headers, userToken, _configuration);
                if (check != null)
                {
                    return check;
                }

                IQueryable<VwScannedInfo> guests = db.VwScannedInfo.Where(p => p.EventId == eventId);

                if (guests == null)
                {
                    return Ok(
                        new EventMessagesStatistics
                        {
                            CongratulationMessages = null,
                            CardMessages = null,
                            EventLocationMessages = null,
                            ReminderMessages = null,
                            ConfirmationMessages = null,
                        }
                    );
                }


                ConfirmationMessagesStatistics confirmationMessages = new();


                confirmationMessages.ReadNumber = guests.Where(p =>
                        p.TextRead == true)
                        .Sum(e => e.NoOfMembers) ?? 0;

                confirmationMessages.DeliverdNumber = guests.Where(p =>
                         p.TextRead != true &&
                         p.TextDelivered == true)
                        .Sum(e => e.NoOfMembers) ?? 0;

                confirmationMessages.SentNumber = guests.Where(p =>
                  p.TextFailed != true &&
                  p.TextDelivered != true &&
                  p.TextRead != true &&
                  p.TextSent == true &&
                  !(p.TextSent == true && p.whatsappMessageId != null))
                   .Sum(e => e.NoOfMembers) ?? 0;

                confirmationMessages.FailedNumber = guests.Where(p =>
                 (p.TextRead != true)
                && (p.TextDelivered != true)
                && ((p.TextSent == true && p.whatsappMessageId != null) || p.TextFailed == true))
                   .Sum(e => e.NoOfMembers) ?? 0;

                confirmationMessages.NotSentNumber = guests.Where(p =>
                p.MessageId == null
                ).Sum(e => e.NoOfMembers) ?? 0;



                CardMessagesStatistics cardMessages = new();

                cardMessages.ReadNumber = guests.Where(p =>
                     p.ImgRead == true)
                     .Sum(e => e.NoOfMembers) ?? 0;

                cardMessages.DeliverdNumber = guests.Where(p =>
                         p.ImgRead != true &&
                         p.ImgDelivered == true)
                        .Sum(e => e.NoOfMembers) ?? 0;

                cardMessages.SentNumber = guests.Where(p =>
                  p.ImgFailed != true &&
                  p.ImgDelivered != true &&
                  p.ImgRead != true &&
                  p.ImgSent == true &&
                  !(p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true))
                   .Sum(e => e.NoOfMembers) ?? 0;

                cardMessages.FailedNumber = guests.Where(p =>
                 (p.ImgRead != true)
                && (p.ImgDelivered != true)
                && ((p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true) || p.ImgFailed == true))
                   .Sum(e => e.NoOfMembers) ?? 0;

                cardMessages.NotSentNumber = guests.Where(p =>
                p.ImgSentMsgId == null
                ).Sum(e => e.NoOfMembers) ?? 0;


                EventLocationMessagesStatistics eventLocationMessages = new();


                eventLocationMessages.ReadNumber = guests.Where(p =>
                    p.EventLocationRead == true)
                    .Sum(e => e.NoOfMembers) ?? 0;

                eventLocationMessages.DeliverdNumber = guests.Where(p =>
                         p.EventLocationRead != true &&
                         p.EventLocationDelivered == true)
                        .Sum(e => e.NoOfMembers) ?? 0;

                eventLocationMessages.SentNumber = guests.Where(p =>
                  p.EventLocationFailed != true &&
                  p.EventLocationDelivered != true &&
                  p.EventLocationRead != true &&
                  p.EventLocationSent == true &&
                  !(p.EventLocationSent == true && p.whatsappWatiEventLocationId != null && p.TextDelivered != true && p.TextRead != true))
                   .Sum(e => e.NoOfMembers) ?? 0;

                eventLocationMessages.FailedNumber = guests.Where(p =>
                 (p.EventLocationRead != true)
                && (p.EventLocationDelivered != true)
                && ((p.EventLocationSent == true && p.whatsappWatiEventLocationId != null && p.TextDelivered != true && p.TextRead != true) || p.EventLocationFailed == true))
                   .Sum(e => e.NoOfMembers) ?? 0;

                eventLocationMessages.NotSentNumber = guests.Where(p =>
                              p.waMessageEventLocationForSendingToAll == null
                              ).Sum(e => e.NoOfMembers) ?? 0;


                ReminderMessagesStatistics reminderMessages = new();

                reminderMessages.ReadNumber = guests.Where(p =>
                    p.ReminderMessageRead == true)
                    .Sum(e => e.NoOfMembers) ?? 0;

                reminderMessages.DeliverdNumber = guests.Where(p =>
                         p.ReminderMessageRead != true &&
                         p.ReminderMessageDelivered == true)
                        .Sum(e => e.NoOfMembers) ?? 0;

                reminderMessages.SentNumber = guests.Where(p =>
                  p.ReminderMessageFailed != true &&
                  p.ReminderMessageDelivered != true &&
                  p.ReminderMessageRead != true &&
                  p.ReminderMessageSent == true &&
                  !(p.ReminderMessageSent == true && p.ReminderMessageWatiId != null && p.TextDelivered != true && p.TextRead != true))
                   .Sum(e => e.NoOfMembers) ?? 0;

                reminderMessages.FailedNumber = guests.Where(p =>
                 (p.ReminderMessageRead != true)
                && (p.ReminderMessageDelivered != true)
                && ((p.ReminderMessageSent == true && p.ReminderMessageWatiId != null && p.TextDelivered != true && p.TextRead != true) || p.ReminderMessageFailed == true))
                   .Sum(e => e.NoOfMembers) ?? 0;

                reminderMessages.NotSentNumber = guests.Where(p =>
              p.ReminderMessageId == null
              ).Sum(e => e.NoOfMembers) ?? 0;

                CongratulationMessagesStatistics congratulationMessages = new();

                congratulationMessages.ReadNumber = guests.Where(p =>
                    p.ConguratulationMsgRead == true)
                    .Sum(e => e.NoOfMembers) ?? 0;

                congratulationMessages.DeliverdNumber = guests.Where(p =>
                         p.ConguratulationMsgRead != true &&
                         p.ConguratulationMsgDelivered == true)
                        .Sum(e => e.NoOfMembers) ?? 0;

                congratulationMessages.SentNumber = guests.Where(p =>
                  p.ConguratulationMsgFailed != true &&
                  p.ConguratulationMsgDelivered != true &&
                  p.ConguratulationMsgRead != true &&
                  p.ConguratulationMsgSent == true &&
                  !(p.ConguratulationMsgSent == true && p.WatiConguratulationMsgId != null && p.TextDelivered != true && p.TextRead != true))
                   .Sum(e => e.NoOfMembers) ?? 0;

                congratulationMessages.FailedNumber = guests.Where(p =>
                 (p.ConguratulationMsgRead != true)
                && (p.ConguratulationMsgDelivered != true)
                && ((p.ConguratulationMsgSent == true && p.WatiConguratulationMsgId != null && p.TextDelivered != true && p.TextRead != true) || p.ConguratulationMsgFailed == true))
                   .Sum(e => e.NoOfMembers) ?? 0;

                congratulationMessages.NotSentNumber = guests.Where(p =>
                                                 p.ConguratulationMsgId == null
                                                  ).Sum(e => e.NoOfMembers) ?? 0;

                var result = new EventMessagesStatistics
                {
                    ConfirmationMessages = confirmationMessages,
                    CardMessages = cardMessages,
                    EventLocationMessages = eventLocationMessages,
                    ReminderMessages = reminderMessages,
                    CongratulationMessages = congratulationMessages,
                };

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
    }
}
