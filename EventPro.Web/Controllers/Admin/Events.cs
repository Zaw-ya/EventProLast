using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPro.DAL.Extensions;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.Web.Common;
using EventPro.Web.Extensions;
using EventPro.Web.Filters;
using EventPro.Web.Models;
using EventPro.Web.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public IActionResult Events()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");

            return View("Events - Copy");
        }

        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        [HttpPost("~/api/GetEvents")]
        public async Task<IActionResult> GetEvents()
        {
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            var sortColumn = Request.Form[string.Concat("columns[", Request.Form["order[0][column]"], "][name]")];
            var sortColumnDirection = Request.Form["order[0][dir]"];

            IQueryable<VwEvents> events_ = db.VwEvents.Where(e => (e.EventTo >= DateTime.Now && e.IsDeleted != true) && (
            string.IsNullOrEmpty(searchValue) ? true
            : (e.Id.ToString().Contains(searchValue))
            || (e.LinkedEvent.ToString().Contains(searchValue))
            || (e.EventTo.ToString().Contains(searchValue))
            || (e.EventFrom.ToString().Contains(searchValue))
            || (e.SystemEventTitle.Contains(searchValue))
            || (e.EventVenue.Contains(searchValue))
            || (e.CreatedOn.ToString().Contains(searchValue))
            || (string.Concat(e.FirstName, " ", e.LastName).Contains(searchValue))
            || (e.Status.Contains(searchValue))
            )).AsNoTracking();

            if (searchValue == "upcoming")
            {
                events_ = db.VwEvents.Where(e => e.EventFrom > DateTime.Now)
                           .AsNoTracking();
            }

            if (searchValue == "in-progress")
            {
                events_ = db.VwEvents.Where(e => e.EventTo >= DateTime.Now && e.EventFrom <= DateTime.Now)
                           .AsNoTracking();

            }

            if (!(string.IsNullOrEmpty(sortColumn)) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                events_ = events_.OrderBy(string.Concat(sortColumn, " ", sortColumnDirection)).Reverse();
            }

            var result = await events_.Skip(skip).Take(pageSize).ToListAsync();
            List<EventVM> eventsVM = new();

            foreach (var evnt in result)
            {
                EventVM eventVM = new(evnt);
                eventsVM.Add(eventVM);
            }

            var recordsTotal = await events_.CountAsync();
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = eventsVM
            };

            return Ok(jsonData);
        }

        // /Admin/DeleteEvent/
        [AuthorizeRoles("Administrator")]
        [HttpGet()]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await db.Events.FindAsync(id);
            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            eventItem.IsDeleted = true;
            eventItem.DeletedOn = DateTime.UtcNow;
            eventItem.DeletedBy = userId;
            eventItem.ShowOnCalender = false;

            var listOfEventGatekeeper = await db.EventGatekeeperMapping
                .Where(e => e.EventId == eventItem.Id)
                .ToListAsync();

            if (listOfEventGatekeeper.Any())
            {
                db.EventGatekeeperMapping.RemoveRange(listOfEventGatekeeper);
            }

            await db.SaveChangesAsync();
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.DeleteEvent);

            return Ok(new { success = true, message = "Event deleted successfully" });
        }

        [AuthorizeRoles("Administrator")]
        public IActionResult ShowDeletedEvents()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Deleted Events", "/admin");

            return View("DeletedEvent");
        }

        // /Admin/GetDeletedEvents
        [AuthorizeRoles("Administrator")]
        [HttpPost()]
        public async Task<IActionResult> GetDeletedEvents()
        {
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            var sortColumn = Request.Form[string.Concat("columns[", Request.Form["order[0][column]"], "][name]")];
            var sortColumnDirection = Request.Form["order[0][dir]"];

            IQueryable<VwEvents> events_ = db.VwEvents.Where(e => (e.IsDeleted == true) &&
            (e.EventTo >= DateTime.Now) && (
            string.IsNullOrEmpty(searchValue) ? true
            : (e.Id.ToString().Contains(searchValue))
            || (e.LinkedEvent.ToString().Contains(searchValue))
            || (e.EventTo.ToString().Contains(searchValue))
            || (e.EventFrom.ToString().Contains(searchValue))
            || (e.SystemEventTitle.Contains(searchValue))
            || (e.EventVenue.Contains(searchValue))
            || (e.DeletedOn.ToString().Contains(searchValue))
            || (string.Concat(e.FirstName, " ", e.LastName).Contains(searchValue))
            || (e.Status.Contains(searchValue))
            )).AsNoTracking();

            if (searchValue == "upcoming")
            {
                events_ = db.VwEvents.Where(e => e.IsDeleted == true && e.EventFrom > DateTime.Now)
                           .AsNoTracking();
            }

            if (searchValue == "in-progress")
            {
                events_ = db.VwEvents.Where(e => e.IsDeleted == true && e.EventTo >= DateTime.Now && e.EventFrom <= DateTime.Now)
                           .AsNoTracking();

            }

            if (!(string.IsNullOrEmpty(sortColumn)) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                events_ = events_.OrderBy(string.Concat(sortColumn, " ", sortColumnDirection)).Reverse();
            }

            var result = await events_.Skip(skip).Take(pageSize).ToListAsync();
            List<EventVM> eventsVM = new();

            foreach (var evnt in result)
            {
                EventVM eventVM = new(evnt);
                eventsVM.Add(eventVM);
            }

            var recordsTotal = await events_.CountAsync();
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = eventsVM
            };

            return Ok(jsonData);
        }

        // /Admin/RestoreEvent/id
        [AuthorizeRoles("Administrator")]
        [HttpGet()]
        public async Task<IActionResult> RestoreEvent(int id)
        {
            var eventItem = await db.Events.FindAsync(id);
            if (eventItem == null)
                return NotFound(new { message = "Event not found or not deleted." });

            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            eventItem.IsDeleted = false;
            eventItem.DeletedOn = null;
            eventItem.DeletedBy = null;

            await db.SaveChangesAsync();
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.RestoreEvent);
            return Ok();
        }

        [AuthorizeRoles("Administrator")]
        public IActionResult EventByOperator()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");
            Dictionary<int, string> userInfo = new Dictionary<int, string>();
            var users = db.VwUsers.ToList();
            foreach (var user in users)
            {
                userInfo.Add(user.UserId, user.FirstName + " " + user.LastName);
            }
            ViewBag.Users = userInfo;
            var events_ = db.VwEvents.Where(p => p.IsDeleted == false).OrderByDescending(p => p.Id).Take(50).ToList();
            ViewBag.FilePath = _configuration.GetSection("Uploads").GetSection("Invoice").Value;
            ViewBag.Gatekeeper = db.VwUsers.Where(p => p.RoleName == "Gatekeeper").ToList();
            ViewBag.Events = events_;
            ViewBag.Client = new SelectList(db.VwUsers.Where(p => p.RoleName == "Operator").ToList(), "UserId", "FullName");
            return View();
        }

        [HttpPost]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> EventByOperator(IFormCollection collection)
        {
            string createdBy = collection["createdBy"];
            string eventStatues = collection["eventStatus"];
            //string eventId = collection["eventId"];
            string dateFrom = collection["dateFrom"];
            string dateTo = collection["dateTo"];
            ViewBag.createdBy = createdBy;
            ViewBag.dateFrom = dateFrom;
            ViewBag.dateTo = dateTo;


            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");
            Dictionary<int, string> userInfo = new Dictionary<int, string>();
            var users = db.VwUsers.ToList();
            foreach (var user in users)
            {
                userInfo.Add(user.UserId, user.FirstName + " " + user.LastName);
            }
            ;
            ViewBag.Users = userInfo;
            var events_ = db.VwEvents
            .Where(p => p.IsDeleted == false)
            .OrderByDescending(p => p.Id)
            .ToList();

            if (createdBy != null && createdBy.Length > 0)
            {
                ViewBag.createdByName = userInfo.Where(p => p.Key == Convert.ToInt32(createdBy)).Select(p => p.Value).FirstOrDefault();
                var evntOperators = await db.EventOperator
                                     .Where(eo => eo.OperatorId == Convert.ToInt32(createdBy)
                                      && eo.Event.IsDeleted != true)
                                     .Select(e => e.Event)
                                     .ToListAsync();

                events_ = events_.Where(e => evntOperators.Select(p => p.Id)
                                          .Contains(e.Id))
                                            .ToList();

            }
            if (dateFrom != null && dateFrom.Length > 0)
                events_ = events_.Where(p => p.CreatedOn >= Convert.ToDateTime(dateFrom)).ToList();
            if (dateTo != null && dateTo.Length > 0)
                events_ = events_.Where(p => p.CreatedOn <= Convert.ToDateTime(dateTo).AddDays(1)).ToList();

            //events_ = events_.Where(p => p.EventTo < DateTime.Today).ToList();
            ViewBag.FilePath = _configuration.GetSection("Uploads").GetSection("Invoice").Value;
            ViewBag.Gatekeeper = db.VwUsers.Where(p => p.RoleName == "Gatekeeper").ToList();
            ViewBag.Events = events_;
            ViewBag.Client = new SelectList(db.VwUsers.Where(p => p.RoleName == "Operator").ToList(), "UserId", "FullName");
            return View();
        }

        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> EventByGatekeeper()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");

            ViewBag.Users =
             new SelectList(await db.Users.Include(u => u.RoleNavigation).Include(u => u.City)
                             .Where(u => u.RoleNavigation.RoleName == "Gatekeeper" && u.Approved.Value)
                             .OrderByDescending(x => x.FirstName)
                          .Select(u => new BasicData { Id = u.UserId, Value = u.City.CityName + "/" + u.FirstName + " " + u.LastName })
                          .ToListAsync()
             , "Id", "Value");

            ViewBag.Address = new SelectList(await db.City.Include(c => c.Country)
               .Select(u => new BasicData { Id = u.Id, Value = u.Country.CountryName + "/" + u.CityName })
                .OrderBy(u => u.Value)
                 .ToListAsync(), "Id", "Value");

            ViewBag.Events = db.Events.Where(e => e.IsDeleted == false).Include(e => e.EventGatekeeperMapping).ThenInclude(ev => ev.Gatekeeper)
                .Include(e => e.City).ThenInclude(c => c.Country)
                  .Select(e => new EventByGatekeeperModel
                  {
                      Id = e.Id,
                      EventTitle = e.EventTitle,
                      SystemEventTitle = e.SystemEventTitle,
                      CreatedFor = e.CreatedFor,
                      EventFrom = e.EventFrom,
                      EventTo = e.EventTo,
                      EventVenue = e.EventVenue,
                      linkedTo = e.LinkedEvent,
                      Location = e.City.Country.CountryName + "/" + e.City.CityName,
                      GatekeeperIds = string.Join(", ", e.EventGatekeeperMapping.ToList().Select(x => x.GatekeeperId.ToString())),
                      GatekeeperNames = string.Join(", ", e.EventGatekeeperMapping.ToList().Select(x => x.Gatekeeper.FirstName + " " + x.Gatekeeper.LastName))
                  })
                 .OrderByDescending(p => p.Id).Take(200).ToList();
            return View();
        }


        [HttpPost]
        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> EventByGatekeeper(IFormCollection collection)
        {
            string gkId = collection["GKId"];
            string cityId = collection["Address"];
            string type = collection["type"];
            string dateFrom = collection["startFrom"];
            string dateTo = collection["startTo"];
            string assigned = collection["Assigned"];

            ViewBag.gkId = gkId;
            ViewBag.cityId = cityId;
            ViewBag.startFrom = dateFrom;
            ViewBag.startTo = dateTo;
            ViewBag.type = type;
            ViewBag.assigned = assigned;

            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");

            ViewBag.Users =
           new SelectList(await db.Users.Include(u => u.RoleNavigation).Include(u => u.City)
                           .Where(u => u.RoleNavigation.RoleName == "Gatekeeper" && u.Approved.Value)
                           .OrderByDescending(x => x.FirstName)
                        .Select(u => new BasicData { Id = u.UserId, Value = u.City.CityName + "/" + u.FirstName + " " + u.LastName })
                        .ToListAsync()
           , "Id", "Value");

            ViewBag.Address = new SelectList(await db.City.Include(c => c.Country)
               .Select(u => new BasicData { Id = u.Id, Value = u.Country.CountryName + "/" + u.CityName })
                .OrderBy(u => u.Value)
                 .ToListAsync(), "Id", "Value");

            ViewBag.Events = await db.Events.Include(e => e.EventGatekeeperMapping).ThenInclude(ev => ev.Gatekeeper)
                         .Include(e => e.City).ThenInclude(c => c.Country)
                         .Where(x =>
                                      x.IsDeleted == false &&
                                      ((assigned == "true" && ((string.IsNullOrEmpty(gkId) && x.EventGatekeeperMapping.Any()) || (!string.IsNullOrEmpty(gkId) && x.EventGatekeeperMapping.Any(x => x.GatekeeperId == int.Parse(gkId)))))
                                       || (assigned == "false" && !x.EventGatekeeperMapping.Any())
                                       || string.IsNullOrEmpty(assigned)) &&
                                   (
                                     string.IsNullOrEmpty(type) ||
                                    (type == "active" && x.EventTo >= DateTime.Today) ||
                                    (type == "archived" && x.EventTo < DateTime.Today)
                                    ) &&
                                   (string.IsNullOrEmpty(gkId) || x.EventGatekeeperMapping.Any(x => x.GatekeeperId == int.Parse(gkId)))
                                && (string.IsNullOrEmpty(cityId) || x.CityId == int.Parse(cityId))
                                && (string.IsNullOrEmpty(dateFrom) || x.EventFrom >= Convert.ToDateTime(dateFrom))
                                && (string.IsNullOrEmpty(dateTo) || x.EventFrom <= Convert.ToDateTime(dateTo))
                                     )
                         .Select(e => new EventByGatekeeperModel
                         {
                             Id = e.Id,
                             EventTitle = e.EventTitle,
                             SystemEventTitle = e.SystemEventTitle,
                             CreatedFor = e.CreatedFor,
                             EventFrom = e.EventFrom,
                             EventTo = e.EventTo,
                             EventVenue = e.EventVenue,
                             linkedTo = e.LinkedEvent,
                             Location = e.City.Country.CountryName + "/" + e.City.CityName,
                             GatekeeperIds = string.Join(", ", e.EventGatekeeperMapping.ToList().Select(x => x.GatekeeperId.ToString())),
                             GatekeeperNames = string.Join(", ", e.EventGatekeeperMapping.ToList().Select(x => x.Gatekeeper.FirstName + " " + x.Gatekeeper.LastName))
                         }).OrderByDescending(p => p.Id).Take(200).ToListAsync();

            return View();
        }

        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> DeleteAllGuests(int id)
        {
            var guests = await db.Guest.Where(e => e.EventId == id)
                .AsNoTracking()
                .ToListAsync();
            db.Guest.RemoveRange(guests);
            await db.SaveChangesAsync();
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            await _blobStorage.DeleteFolderAsync(environment + cardPreview + "/" + id + "/", cancellationToken: default);

            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.DeleteAllGuests);
            return Ok();
        }

        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> DeleteAllGuestsCards(int id)
        {
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            await _blobStorage.DeleteFolderAsync(environment + cardPreview + "/" + id + "/", cancellationToken: default);

            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.DeleteAllGuestsCards);
            return Ok();
        }


        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> ResetAllGuestsStatus(int id)
        {
            var guests = await db.Guest.Where(e => e.EventId == id)
                .ToListAsync();
            foreach (var guest in guests)
            {
                guest.Response = null;
                guest.TextDelivered = null;
                guest.TextRead = null;
                guest.TextSent = null;
                guest.TextFailed = null;
                guest.MessageId = null;
                guest.ConguratulationMsgId = null;
                guest.ConguratulationMsgFailed = null;
                guest.ConguratulationMsgDelivered = null;
                guest.ConguratulationMsgSent = null;
                guest.ConguratulationMsgRead = null;
                guest.ImgDelivered = null;
                guest.ImgFailed = null;
                guest.ImgRead = null;
                guest.ImgSent = null;
                guest.TextFailed = null;
                guest.ImgSentMsgId = null;
                guest.WaresponseTime = null;
                guest.whatsappMessageEventLocationId = null;
                guest.EventLocationSent = null;
                guest.EventLocationRead = null;
                guest.EventLocationDelivered = null;
                guest.EventLocationFailed = null;
                guest.waMessageEventLocationForSendingToAll = null;
                guest.ReminderMessageId = null;
                guest.ReminderMessageSent = null;
                guest.ReminderMessageRead = null;
                guest.ReminderMessageDelivered = null;
                guest.ReminderMessageFailed = null;
            }

            await db.SaveChangesAsync();

            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.ResetAllGuestsStatus);
            return Ok();
        }

        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> AllowSendConfirmationMessageAgain(int id)
        {
            var guests = await db.Guest.Where(e => e.EventId == id)
                .ToListAsync();

            foreach (var guest in guests)
            {
                guest.Response = null;
                guest.TextDelivered = null;
                guest.TextRead = null;
                guest.TextSent = null;
                guest.TextFailed = null;
                guest.MessageId = null;
                guest.WaresponseTime = null;
            }

            await db.SaveChangesAsync();
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.AllowSendConfirmationAgain);
            return Ok();
        }

        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> AllowSendCardMessageAgain(int id)
        {
            var guests = await db.Guest.Where(e => e.EventId == id)
                .ToListAsync();

            foreach (var guest in guests)
            {
                guest.ImgDelivered = null;
                guest.ImgFailed = null;
                guest.ImgRead = null;
                guest.ImgSent = null;
                guest.TextFailed = null;
                guest.ImgSentMsgId = null;
            }

            await db.SaveChangesAsync();

            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.AllowSendCardsAgain);
            return Ok();
        }

        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> AllowSendEventLocationMessageAgain(int id)
        {
            var guests = await db.Guest.Where(e => e.EventId == id)
                .ToListAsync();

            foreach (var guest in guests)
            {
                guest.EventLocationSent = null;
                guest.EventLocationRead = null;
                guest.EventLocationDelivered = null;
                guest.EventLocationFailed = null;
                guest.waMessageEventLocationForSendingToAll = null;
            }

            await db.SaveChangesAsync();

            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.AllowSendEventLocationAgain);
            return Ok();
        }


        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> AllowSendReminderMessageAgain(int id)
        {
            var guests = await db.Guest.Where(e => e.EventId == id)
                .ToListAsync();

            foreach (var guest in guests)
            {
                guest.ReminderMessageId = null;
                guest.ReminderMessageSent = null;
                guest.ReminderMessageRead = null;
                guest.ReminderMessageDelivered = null;
                guest.ReminderMessageFailed = null;
            }

            await db.SaveChangesAsync();

            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.AllowSendRemindersAgain);
            return Ok();
        }

        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> AllowSendCongratulationMessageAgain(int id)
        {
            var guests = await db.Guest.Where(e => e.EventId == id)
                .ToListAsync();

            foreach (var guest in guests)
            {
                guest.ConguratulationMsgId = null;
                guest.ConguratulationMsgFailed = null;
                guest.ConguratulationMsgDelivered = null;
                guest.ConguratulationMsgSent = null;
                guest.ConguratulationMsgRead = null;
            }

            await db.SaveChangesAsync();

            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.AllowSendCongratulationsAgain);
            return Ok();
        }

        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> EventsByGatekeeper()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");

            ViewBag.Users =
             new SelectList(await db.Users.Include(u => u.RoleNavigation).Include(u => u.City)
                             .Where(u => u.RoleNavigation.RoleName == "Gatekeeper" && u.Approved.Value)
                             .OrderByDescending(x => x.FirstName)
                          .Select(u => new BasicData { Id = u.UserId, Value = u.City.CityName + "/" + u.FirstName + " " + u.LastName })
                          .ToListAsync()
             , "Id", "Value");

            ViewBag.Address = new SelectList(await db.City.Include(c => c.Country)
               .Select(u => new BasicData { Id = u.Id, Value = u.Country.CountryName + "/" + u.CityName })
                .OrderBy(u => u.Value)
                 .ToListAsync(), "Id", "Value");
            return View();
        }

        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> FilterEventsByGK()
        {
            var filterDto = Request.Form.GetFilters();
            if (!string.IsNullOrEmpty(filterDto.SortColumn) &&
                new[] { nameof(EventByGatekeeperModel.Location), nameof(EventByGatekeeperModel.GatekeeperIds), nameof(EventByGatekeeperModel.GatekeeperNames) }.Any(c => filterDto.SortColumn.Contains(c)))
            {
                return Ok(null);
            }
            string eventTitle = Request.Form["EventTitle"];
            string eventId = Request.Form["EventId"];
            string cityId = Request.Form["Address"];
            string type = Request.Form["type"];
            string dateFrom = Request.Form["startFrom"];
            string dateTo = Request.Form["startTo"];
            string assigned = Request.Form["assigned"];
            string gkId = Request.Form["gkId"];

            IQueryable<Events> events = db.Events.Include(e => e.EventGatekeeperMapping).ThenInclude(ev => ev.Gatekeeper)
                                           .Include(e => e.City).ThenInclude(c => c.Country).Include(e => e.TypeNavigation);

            if (!string.IsNullOrEmpty(filterDto.SearchValue))
                events = events.Where(b => b.EventTitle.Contains(filterDto.SearchValue!) ||
                                     b.Id.ToString().Contains(filterDto.SearchValue!) ||
                                     b.EventGatekeeperMapping.Any(x => x.Gatekeeper.FirstName.Contains(filterDto.SearchValue!)) ||
                                     b.EventGatekeeperMapping.Any(x => x.Gatekeeper.LastName.Contains(filterDto.SearchValue!))
                              );

            events = events.Where(x =>
                              (string.IsNullOrEmpty(eventTitle) || (x.EventTitle.Contains(eventTitle)))
                           && (string.IsNullOrEmpty(eventId) || (x.Id == int.Parse(eventId)))

                     && ((assigned == "true" && ((string.IsNullOrEmpty(gkId) && x.EventGatekeeperMapping.Any()) || (!string.IsNullOrEmpty(gkId) && x.EventGatekeeperMapping.Any(x => x.GatekeeperId == int.Parse(gkId)))))
                                       || (assigned == "false" && !x.EventGatekeeperMapping.Any())
                                       || string.IsNullOrEmpty(assigned))
                                    && (string.IsNullOrEmpty(type) || (type == "active" && x.EventTo >= DateTime.Today) || (type == "archived" && x.EventTo < DateTime.Today))
                                    && (string.IsNullOrEmpty(gkId) || x.EventGatekeeperMapping.Any(x => x.GatekeeperId == int.Parse(gkId)))
                                && (string.IsNullOrEmpty(cityId) || x.CityId == int.Parse(cityId))
                                && (string.IsNullOrEmpty(dateFrom) || x.EventFrom >= Convert.ToDateTime(dateFrom))
                                && (string.IsNullOrEmpty(dateTo) || x.EventFrom <= Convert.ToDateTime(dateTo))
                           );
            var recordsTotal = events.Count();
            events = events
                 .OrderBy($"{filterDto.SortColumn} {filterDto.SortColumnDirection}")
                 .Skip(filterDto.Skip)
                 .Take(filterDto.PageSize);
            var eventsList = await events.Select(e => new EventByGatekeeperModel
            {
                Id = e.Id,
                EventTitle = e.EventTitle,
                Icon = e.TypeNavigation.Icon,
                CreatedFor = e.CreatedFor,
                EventFrom = e.EventFrom,
                EventTo = e.EventTo,

                EventVenue = e.EventVenue,
                linkedTo = e.LinkedEvent,
                Location = e.City.Country.CountryName + "/" + e.City.CityName,
                GatekeeperIds = string.Join(", ", e.EventGatekeeperMapping.ToList().Select(x => x.GatekeeperId.ToString())),
                GatekeeperNames = string.Join(", ", e.EventGatekeeperMapping.ToList().Select(x => x.Gatekeeper.FirstName + " " + x.Gatekeeper.LastName))
            }).ToListAsync();

            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal, data = eventsList };

            return Ok(jsonData);
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> GatekeeperChecks()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");

            ViewBag.Users =
             new SelectList(await db.Users.Include(u => u.RoleNavigation).Include(u => u.City)
                             .Where(u => u.RoleNavigation.RoleName == "Gatekeeper" && u.Approved.Value)
                             .OrderByDescending(x => x.FirstName)
                          .Select(u => new BasicData { Id = u.UserId, Value = u.City.CityName + "/" + u.FirstName + " " + u.LastName })
                          .ToListAsync()
             , "Id", "Value");

            ViewBag.Address = new SelectList(await db.City.Include(c => c.Country)
               .Select(u => new BasicData { Id = u.Id, Value = u.Country.CountryName + "/" + u.CityName })
                .OrderBy(u => u.Value)
                 .ToListAsync(), "Id", "Value");

            ViewBag.Events = db.GKEventHistory.Include(e => e.Gatekeeper)
                .Include(e => e.Event).ThenInclude(e => e.City).ThenInclude(c => c.Country)
                  .Select(gkh => new GatekeeperChecksModel
                  {
                      Id = gkh.Event_Id,
                      EventTitle = gkh.Event.EventTitle,
                      EventFrom = gkh.Event.EventFrom,
                      EventTo = gkh.Event.EventTo,
                      EventVenue = gkh.Event.EventVenue,
                      linkedTo = gkh.Event.LinkedEvent,
                      Location = gkh.Event.City.Country.CountryName + "/" + gkh.Event.City.CityName,
                      GatekeeperId = gkh.GK_Id,
                      GatekeeperName = gkh.Gatekeeper.FirstName + " " + gkh.Gatekeeper.LastName,
                      CheckType = gkh.CheckType,
                      LogDate = gkh.LogDT.ToString("dd/MM/yyyy h:mm tt"),
                      latitude = gkh.latitude,
                      longitude = gkh.longitude
                  })
                 .OrderByDescending(p => p.Id).Take(200).ToList();
            return View();
        }

        [HttpPost]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> GatekeeperChecks(IFormCollection collection)
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");

            ViewBag.Users =
             new SelectList(await db.Users.Include(u => u.RoleNavigation).Include(u => u.City)
                             .Where(u => u.RoleNavigation.RoleName == "Gatekeeper" && u.Approved.Value)
                             .OrderByDescending(x => x.FirstName)
                          .Select(u => new BasicData { Id = u.UserId, Value = u.City.CityName + "/" + u.FirstName + " " + u.LastName })
                          .ToListAsync()
             , "Id", "Value");

            ViewBag.Address = new SelectList(await db.City.Include(c => c.Country)
               .Select(u => new BasicData { Id = u.Id, Value = u.Country.CountryName + "/" + u.CityName })
                .OrderBy(u => u.Value)
                 .ToListAsync(), "Id", "Value");

            string gkId = collection["GKId"];
            string cityId = collection["Address"];
            string type = collection["type"];

            string dateFrom = collection["startFrom"];
            string dateTo = collection["startTo"];

            ViewBag.gkId = gkId;
            ViewBag.cityId = cityId;
            ViewBag.startFrom = dateFrom;
            ViewBag.startTo = dateTo;
            ViewBag.type = type;

            ViewBag.Events = db.GKEventHistory.Include(e => e.Gatekeeper)
                .Include(e => e.Event).ThenInclude(e => e.City).ThenInclude(c => c.Country)
                .Where(gkh =>
                                    (string.IsNullOrEmpty(gkId) || gkh.GK_Id == int.Parse(gkId))
                                && (string.IsNullOrEmpty(type) || gkh.CheckType.ToLower().Trim() == type.ToLower().Trim())
                                && (string.IsNullOrEmpty(cityId) || gkh.Event.CityId == int.Parse(cityId))
                                && (string.IsNullOrEmpty(dateFrom) || gkh.Event.EventFrom >= Convert.ToDateTime(dateFrom))
                                && (string.IsNullOrEmpty(dateTo) || gkh.Event.EventFrom <= Convert.ToDateTime(dateTo))
                         )
                  .Select(gkh => new GatekeeperChecksModel
                  {
                      Id = gkh.Event_Id,
                      EventTitle = gkh.Event.EventTitle,
                      EventFrom = gkh.Event.EventFrom,
                      EventTo = gkh.Event.EventTo,
                      EventVenue = gkh.Event.EventVenue,
                      linkedTo = gkh.Event.LinkedEvent,
                      Location = gkh.Event.City.Country.CountryName + "/" + gkh.Event.City.CityName,
                      GatekeeperId = gkh.GK_Id,
                      GatekeeperName = gkh.Gatekeeper.FirstName + " " + gkh.Gatekeeper.LastName,
                      CheckType = gkh.CheckType,
                      LogDate = gkh.LogDT.ToString("dd/MM/yyyy h:mm tt"),
                      latitude = gkh.latitude,
                      longitude = gkh.longitude
                  })
                 .OrderByDescending(p => p.Id).Take(200).ToList();
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> GatekeepersCheck()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");

            ViewBag.Users =
             new SelectList(await db.Users.Include(u => u.RoleNavigation).Include(u => u.City)
                             .Where(u => u.RoleNavigation.RoleName == "Gatekeeper" && u.Approved.Value)
                             .OrderByDescending(x => x.FirstName)
                          .Select(u => new BasicData { Id = u.UserId, Value = u.City.CityName + "/" + u.FirstName + " " + u.LastName })
                          .ToListAsync()
             , "Id", "Value");

            ViewBag.Address = new SelectList(await db.City.Include(c => c.Country)
               .Select(u => new BasicData { Id = u.Id, Value = u.Country.CountryName + "/" + u.CityName })
                .OrderBy(u => u.Value)
                 .ToListAsync(), "Id", "Value");
            return View();
        }

        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> FilterGKsCheck()
        {
            var filterDto = Request.Form.GetFilters();

            var forbiddenSorts = new[] { nameof(GatekeeperChecksModel.Location), nameof(GatekeeperChecksModel.GatekeeperName) };
            if (!string.IsNullOrEmpty(filterDto.SortColumn) && forbiddenSorts.Contains(filterDto.SortColumn))
                return Ok(new { recordsFiltered = 0, recordsTotal = 0, data = new List<object>() });

            string eventTitle = Request.Form["EventTitle"];
            string eventId = Request.Form["EventId"];
            string cityId = Request.Form["Address"];
            string type = Request.Form["type"];
            string dateFrom = Request.Form["startFrom"];
            string dateTo = Request.Form["startTo"];
            string gkId = Request.Form["gkId"];
            string checkType = Request.Form["checktype"];

            int? eId = int.TryParse(eventId, out var tmpEid) ? tmpEid : null;
            int? gkIdParsed = int.TryParse(gkId, out var tmpGkId) ? tmpGkId : null;
            int? cityIdParsed = int.TryParse(cityId, out var tmpCityId) ? tmpCityId : null;
            DateTime? fromDate = DateTime.TryParse(dateFrom, out var tmpFrom) ? tmpFrom : null;
            DateTime? toDate = DateTime.TryParse(dateTo, out var tmpTo) ? tmpTo : null;

            var query = db.GKEventHistory
                .AsNoTracking()
                .Include(e => e.Gatekeeper)
                .Include(e => e.Event).ThenInclude(e => e.City).ThenInclude(c => c.Country)
                .Include(e => e.Event).ThenInclude(e => e.TypeNavigation)
                .Where(e => e.Event != null && e.Event.IsDeleted != true);

            if (!string.IsNullOrEmpty(filterDto.SearchValue))
            {
                var search = filterDto.SearchValue;
                query = query.Where(b =>
                    b.Event.SystemEventTitle.Contains(search) ||
                    b.Id.ToString().Contains(search) ||
                    b.Event.Id.ToString().Contains(search) ||
                    b.Gatekeeper.FirstName.Contains(search) ||
                    b.Gatekeeper.LastName.Contains(search));
            }

            query = query.Where(x =>
                (string.IsNullOrEmpty(eventTitle) || x.Event.SystemEventTitle.Contains(eventTitle)) &&
                (!eId.HasValue || x.Event.Id == eId.Value) &&
                (!gkIdParsed.HasValue || x.GK_Id == gkIdParsed.Value) &&
                (!cityIdParsed.HasValue || x.Event.CityId == cityIdParsed.Value) &&
                (string.IsNullOrEmpty(type) ||
                    (type == "active" && x.Event.EventTo >= DateTime.Today) ||
                    (type == "archived" && x.Event.EventTo < DateTime.Today)) &&
                (string.IsNullOrEmpty(checkType) || x.CheckType.Equals(checkType, StringComparison.OrdinalIgnoreCase)) &&
                (!fromDate.HasValue || x.Event.EventFrom >= fromDate.Value) &&
                (!toDate.HasValue || x.Event.EventFrom <= toDate.Value)
            );

            var sortColumn = filterDto.SortColumn;                 // may be null
            var sortDir = (filterDto.SortColumnDirection ?? "asc").ToLower();

            switch (sortColumn)
            {
                case "EventTitle":
                    query = sortDir == "desc"
                        ? query.OrderByDescending(x => x.Event.SystemEventTitle)
                        : query.OrderBy(x => x.Event.SystemEventTitle);
                    break;

                case "EventFrom":
                    query = sortDir == "desc"
                        ? query.OrderByDescending(x => x.Event.EventFrom)
                        : query.OrderBy(x => x.Event.EventFrom);
                    break;

                case "EventTo":
                    query = sortDir == "desc"
                        ? query.OrderByDescending(x => x.Event.EventTo)
                        : query.OrderBy(x => x.Event.EventTo);
                    break;

                case "CheckType":
                    query = sortDir == "desc"
                        ? query.OrderByDescending(x => x.CheckType)
                        : query.OrderBy(x => x.CheckType);
                    break;

                case "LogDate":
                    query = sortDir == "desc"
                        ? query.OrderByDescending(x => x.LogDT)
                        : query.OrderBy(x => x.LogDT);
                    break;

                case "Id":
                    query = sortDir == "desc"
                        ? query.OrderByDescending(x => x.Id)
                        : query.OrderBy(x => x.Id);
                    break;

                default:
                    // fallback sort
                    query = query.OrderBy(x => x.Event.EventFrom);
                    break;
            }

            var total = await query.CountAsync();

            var data = await query
                .Skip(filterDto.Skip)
                .Take(filterDto.PageSize)
                .Select(gkh => new GatekeeperChecksModel
                {
                    Id = gkh.Event_Id,
                    EventTitle = gkh.Event.SystemEventTitle,
                    Icon = gkh.Event.TypeNavigation.Icon,
                    EventFrom = gkh.Event.EventFrom,
                    EventTo = gkh.Event.EventTo,
                    EventVenue = gkh.Event.EventVenue,
                    linkedTo = gkh.Event.LinkedEvent,
                    Location = gkh.Event.City.Country.CountryName + "/" + gkh.Event.City.CityName,
                    GatekeeperId = gkh.GK_Id,
                    GatekeeperName = gkh.Gatekeeper.FirstName + " " + gkh.Gatekeeper.LastName,
                    CheckType = gkh.CheckType,
                    LogDate = gkh.LogDT.ToString("dd/MM/yyyy h:mm tt"),
                    latitude = gkh.latitude,
                    longitude = gkh.longitude
                })
                .ToListAsync();

            return Ok(new { recordsFiltered = total, recordsTotal = total, data });
        }

        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public IActionResult ArchiveEvents()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");

            return View("ArchiveEvents - Copy");
        }


        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        [HttpPost("~/api/GetArchiveEvents")]
        public async Task<IActionResult> GetArchiveEvents()
        {
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            var sortColumn = Request.Form[string.Concat("columns[", Request.Form["order[0][column]"], "][name]")];
            var sortColumnDirection = Request.Form["order[0][dir]"];

            IQueryable<VwEvents> events_ = db.VwEvents.Where(e => (e.EventTo < DateTime.Now && e.IsDeleted != true) && (
            string.IsNullOrEmpty(searchValue) ? true
            : (e.Id.ToString().Contains(searchValue))
            || (e.LinkedEvent.ToString().Contains(searchValue))
            || (e.EventTo.ToString().Contains(searchValue))
            || (e.EventFrom.ToString().Contains(searchValue))
            || (e.EventTitle.Contains(searchValue))
            || (e.EventVenue.Contains(searchValue))
            || (e.CreatedOn.ToString().Contains(searchValue))
            || (string.Concat(e.FirstName, " ", e.LastName).Contains(searchValue))
            || (e.Status.Contains(searchValue))
            )).AsNoTracking();


            if (!(string.IsNullOrEmpty(sortColumn)) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                events_ = events_.OrderBy(string.Concat(sortColumn, " ", sortColumnDirection)).Reverse();
            }

            var result = await events_.Skip(skip).Take(pageSize).ToListAsync();
            List<EventVM> eventsVM = new();

            foreach (var evnt in result)
            {
                EventVM eventVM = new(evnt);
                eventsVM.Add(eventVM);
            }

            var recordsTotal = await events_.CountAsync();
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = eventsVM
            };

            return Ok(jsonData);
        }

        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        [HttpPost]
        public async Task<IActionResult> Events(IFormCollection collection)
        {
            string clientName = collection["clientName"];
            string eventStatues = collection["eventStatus"];
            string dateFrom = collection["dateFrom"];
            string dateTo = collection["dateTo"];

            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");
            Dictionary<int, string> userInfo = new Dictionary<int, string>();
            var users = db.VwUsers.ToList();
            foreach (var user in users)
            {
                userInfo.Add(user.UserId, user.FirstName + " " + user.LastName);
            }
            var events_ = await db.VwEvents.OrderByDescending(p => p.Id).ToListAsync();

            if (clientName != null && clientName.Length > 0)
            {
                events_ = events_.Where(p => p.CreatedFor <= Convert.ToInt32(clientName)).ToList();

            }

            if (dateFrom != null && dateFrom.Length > 0)
            {
                events_ = events_.Where(p => p.EventFrom >= Convert.ToDateTime(dateFrom)).ToList();
            }

            if (dateTo != null && dateTo.Length > 0)
            {
                events_ = events_.Where(p => p.EventTo <= Convert.ToDateTime(dateTo)).ToList();
            }

            if (eventStatues != null && eventStatues.Length > 0)
            {
                if (eventStatues.Contains("In Progress") && eventStatues.Contains("Upcoming") && eventStatues.Contains("Past"))
                    events_ = events_.ToList();
                else if (eventStatues.Contains("In Progress") && eventStatues.Contains("Upcoming"))
                    events_ = events_.Where(p => p.EventTo >= DateTime.Today).ToList();
                else if (eventStatues.Contains("Past") && eventStatues.Contains("Upcoming"))
                    events_ = events_.Where(p => p.EventFrom < DateTime.Today || p.EventFrom > DateTime.Today).ToList();
                else if (eventStatues.Contains("Past"))
                    events_ = events_.Where(p => p.EventTo < DateTime.Today).ToList();
                else if (eventStatues.Contains("Upcoming"))
                    events_ = events_.Where(p => p.EventFrom > DateTime.Today).ToList();
                else if (eventStatues.Contains("In Progress"))
                    events_ = events_.Where(p => p.EventFrom <= DateTime.Today && p.EventTo >= DateTime.Today).ToList();

            }
            ViewBag.Users = userInfo;
            ViewBag.Client = new SelectList(db.VwUsers.Where(p => p.RoleName == "Client").ToList(), "UserId", "FullName");
            ViewBag.FilePath = _configuration.GetSection("Uploads").GetSection("Invoice").Value;
            ViewBag.Gatekeeper = db.VwUsers.Where(p => p.RoleName == "Gatekeeper").ToList();
            ViewBag.Events = events_;
            return View();
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> _Events()
        {
            var clients = new List<DAL.Models.Users>();
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var createdForIds = await db.Events
                    .Where(e => e.EventOperators.Any(p => p.OperatorId == userId))
                    .Select(e => e.CreatedFor)
                    .Distinct()
                .ToListAsync();
                clients = await db.Users.Where(p => (createdForIds.Contains(p.UserId) || p.CreatedBy == userId) &&
                p.Role == 2)
                    .OrderByDescending(e => e.UserId)
                    .ToListAsync();
            }
            else
            {
                clients = await db.Users.Where(p => p.Role == 2)
                    .OrderByDescending(e => e.UserId)
                    .ToListAsync();
            }

            ViewBag.Type = new SelectList(await db.EventCategory.AsNoTracking().ToListAsync(), "EventId", "Category");
            ViewBag.Users = new SelectList(clients,
                "UserId", "UserName");

            ViewBag.LinkedEvents = new SelectList(await db.Events.Where(p => p.EventTo >= DateTime.Today)
                .Select(x => new BasicData { Id = x.Id, Value = x.Id + "-" + x.SystemEventTitle })
                .AsNoTracking()
                .ToListAsync(), "Id", "Value");

            ViewBag.Icon = "nav-icon fas fa-calendar";
            ViewBag.EventLocations =
                new SelectList((await db.City.Include(c => c.Country).AsNoTracking().ToListAsync())
                .Select(e => new { e.Id, Location = e.CityName + "|" + e.Country.CountryName })
                , "Id", "Location");

            SetBreadcrum("Events", "/admin");
            return View("EditEvent - Copy", new Events());
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> _Events(Events events)
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (events.EventFrom > events.EventTo)
            {
                ViewBag.ErrorDate = "(End date should be greater then from date)";
                ViewBag.Type = new SelectList(db.EventCategory.ToList(), "EventId", "Category");
                ViewBag.Users = new SelectList(db.VwUsers.Where(p => p.RoleName == "Client").ToList(), "UserId", "UserName");
                ViewBag.Icon = "nav-icon fas fa-calendar";
                ViewBag.LinkedEvents = new SelectList(await db.Events.Where(p => p.EventTo >= DateTime.Today).Select(x => new BasicData { Id = x.Id, Value = x.Id + "-" + x.EventTitle }).AsNoTracking().ToListAsync(), "Id", "Value");
                ViewBag.EventLocations =
               new SelectList((await db.City.Include(c => c.Country).AsNoTracking().ToListAsync())
               .Select(e => new { e.Id, Location = e.CityName + "|" + e.Country.CountryName }), "Id", "Location");
                SetBreadcrum("Events", "/admin");
                return View("EditEvent - Copy", events);
            }
            if (string.IsNullOrEmpty(events.EventDescription) || string.IsNullOrWhiteSpace(events.EventDescription))
            {
                ViewBag.ErrorDesc = "(Event Description Can not be empty)";
                ViewBag.Type = new SelectList(db.EventCategory.ToList(), "EventId", "Category");
                ViewBag.Users = new SelectList(db.VwUsers.Where(p => p.RoleName == "Client").ToList(), "UserId", "UserName");
                ViewBag.Icon = "nav-icon fas fa-calendar";
                ViewBag.LinkedEvents = new SelectList(await db.Events.Where(p => p.EventTo >= DateTime.Today).Select(x => new BasicData { Id = x.Id, Value = x.Id + "-" + x.EventTitle }).AsNoTracking().ToListAsync(), "Id", "Value");
                SetBreadcrum("Events", "/admin");
                ViewBag.EventLocations =
               new SelectList((await db.City.Include(c => c.Country).ToListAsync())
               .Select(e => new { e.Id, Location = e.CityName + "|" + e.Country.CountryName })
                , "Id", "Location");
                return View("EditEvent - Copy", events);
            }

            if (events.GmapCode.Length > 0)
            {
                string[] gmaps = events.GmapCode.Split('/');
                if (gmaps.Length > 0)
                {
                    events.GmapCode = gmaps[gmaps.Length - 1];
                }
            }

            var files = Request.Form.Files;
            string path = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            string filename = string.Empty;
            string extension = string.Empty;
            bool hasFile = false;
            foreach (var file in files)
            {
                if (file.ContentType.Contains("image") && file.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(string.Empty, "Image size must be less than 5 MB");
                    return View("EditEvent - Copy", events);
                }
                else if (file.ContentType.Contains("image"))
                {
                    extension = file.ContentType.ToLower().Replace(@"image/", "");
                }

                if (file.ContentType.Contains("video") && file.Length > 15 * 1024 * 1024)
                {
                    ModelState.AddModelError(string.Empty, "Video size must be less than 15 MB");
                    return View("EditEvent - Copy", events);
                }
                else if (file.ContentType.Contains("video"))
                {
                    extension = file.ContentType.ToLower().Replace(@"video/", "");
                }

                // Check for PDF files
                if (file.ContentType.Contains("application/pdf"))
                {
                    extension = "pdf";
                    if (file.Length > 15 * 1024 * 1024)
                    {
                        ModelState.AddModelError(string.Empty, "PDF size must be less than 15 MB");
                        return View("EditEvent - Copy", events);
                    }
                }

                filename = Guid.NewGuid() + "." + extension;
                using (var fileStream = new FileStream(path + @"\" + filename, FileMode.Create))
                {
                    hasFile = true;
                    file.CopyTo(fileStream);
                }
            }
            events.SendingConfiramtionMessagesLinksLanguage = "Arabic";
            events.LinkGuestsCardText = "????? ?????? ?????? ???";
            events.ConfirmationButtonsType = "QuickReplies";
            events.IsDeleted = false;
            events.MessageHeaderImage = filename;
            events.CreatedBy = userId;
            events.CreatedOn = DateTime.UtcNow;
            events.Status = "Active";
            events.CityId = events.CityId;
            events.WhatsappProviderName = "Default";
            events.AttendanceTime = events.EventFrom.Value + events.AttendanceTime.Value.TimeOfDay;
            events.LeaveTime = events.LeaveTime.Value;
            events.ContactName = events.ContactName;
            events.ContactPhone = events.ContactPhone;
            events.ShowFailedSendingEventLocationLink = true;
            events.ShowFailedSendingCongratulationLink = true;
            events.ChoosenNumberWithinCountry = 1;
            var settings = await db.AppSettings.FirstOrDefaultAsync();
            events.choosenSendingWhatsappProfile = settings.WhatsappDefaultTwilioProfile;
            City city = await db.City.Where(e => e.Id == events.CityId)
                .Include(e => e.Country)
                .FirstOrDefaultAsync();
            if (city.Country.CountryName.Contains("??????"))
            {
                events.choosenSendingCountryNumber = "KUWAIT";
            }
            else if (city.Country.CountryName.Contains("???????"))
            {
                events.choosenSendingCountryNumber = "BAHRAIN";
            }
            else
            {
                events.choosenSendingCountryNumber = "SAUDI";
            }

            await db.Events.AddAsync(events);
            await db.SaveChangesAsync();

            var eventOperator = new EventOperator()
            {
                OperatorId = userId,
                EventId = events.Id,
                EventStart = events.EventFrom,
                EventEnd = events.EventTo
            };

            await db.EventOperator.AddAsync(eventOperator);
            await db.SaveChangesAsync();

            await _auditLogService.AddAsync(userId, events.Id);

            try
            {
                if (events.ShowOnCalender == true)
                {
                    var request = new MessageRequest()
                    {
                        Topic = $"{events.CityId}",
                        Title = events.EventTitle,
                        Body = $"New event is created! \nstarting in {events.EventVenue} at {events.AttendanceTime}"
                    };

                    var message = new Message()
                    {
                        Notification = new Notification
                        {
                            Title = request.Title,
                            Body = request.Body
                        },
                    };
                    if (string.IsNullOrEmpty(request.Tokens))
                    {
                        message.Topic = request.Topic;
                    }
                    else
                    {
                        message.Token = request.Tokens;
                    }
                    if (FirebaseApp.DefaultInstance == null)
                    {
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EventPro-1b0ca-firebase-adminsdk-yp15k-b2881aed59.json")),
                        });
                    }

                    var messaging = FirebaseMessaging.DefaultInstance;
                    var result = await messaging.SendAsync(message);
                }
            }
            catch (Exception ex)
            {
            }

            await CreateReminderIcsFileAsync(events);
            Log.Information($"Event with ID: {events.Id} created by:{userId}");
            return RedirectToAction(AppAction.Events, AppController.Admin);
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> EditEvent(int id)
        {
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var events = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            events.ReminderMessage = events.ReminderMessage?.Replace("\\n", "\r\n").TrimEnd();
            events.ThanksMessage = events.ThanksMessage?.Replace("\\n", "\r\n").TrimEnd();

            await SetControls(events);
            return View(events);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> EditEvent([FromForm] Events events)
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (events.EventFrom > events.EventTo)
            {
                await SetControls(events);
                ViewBag.ErrorDate = "(Date should be greater then event from date)";
                return View("EditEvent", events);
            }
            if (string.IsNullOrEmpty(events.EventDescription) || string.IsNullOrWhiteSpace(events.EventDescription))
            {
                await SetControls(events);
                ViewBag.ErrorDesc = "(Event Description Can not be empty)";
                return View("EditEvent", events);
            }

            Events evt = await db.Events.Where(p => p.Id == events.Id)
                .FirstOrDefaultAsync();

            if (events.GmapCode.Length > 0)
            {
                string[] gmaps = events.GmapCode.Split('/');
                if (gmaps.Length > 0)
                {
                    events.GmapCode = gmaps[gmaps.Length - 1];
                }
            }
            var files = Request.Form.Files;
            if (files.Count != 0)
            {
                string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
                string path = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
                string filename = string.Empty;
                bool hasFile = false;
                string extension = string.Empty;
                foreach (var file in files)
                {
                    if (file.ContentType.Contains("image") && file.Length > 5 * 1024 * 1024)
                    {
                        await SetControls(events);
                        ModelState.AddModelError(string.Empty, "Image size must be less than 5 MB");
                        return View("EditEvent", events);
                    }
                    else if (file.ContentType.Contains("image"))
                    {
                        extension = file.ContentType.ToLower().Replace(@"image/", "");
                    }

                    if (file.ContentType.Contains("video") && file.Length > 15 * 1024 * 1024)
                    {
                        await SetControls(events);
                        ModelState.AddModelError(string.Empty, "Video size must be less than 15 MB");
                        return View("EditEvent", events);
                    }
                    else if (file.ContentType.Contains("video"))
                    {
                        extension = file.ContentType.ToLower().Replace(@"video/", "");
                    }

                    // Check for PDF files
                    if (file.ContentType.Contains("application/pdf"))
                    {
                        extension = "pdf";

                        if (file.Length > 15 * 1024 * 1024)
                        {
                            await SetControls(events);
                            ModelState.AddModelError(string.Empty, "PDF size must be less than 15 MB");
                            return View("EditEvent", events);
                        }
                    }

                    filename = Guid.NewGuid() + "." + extension;

                    using var stream = file.OpenReadStream();
                    await _blobStorage.UploadAsync(stream, extension, environment + path + "/" + filename, cancellationToken: default);
                    hasFile = true;

                    if (extension != "mp4" && extension != "png" && extension != "jpg" && extension != "jpeg" && extension != "pdf")
                    {
                        await SetControls(events);
                        ModelState.AddModelError(string.Empty, $"the file extension is not supported");
                        return View("EditEvent", events);
                    }

                    if (file.Name.Equals("MessageHeaderImage"))
                    {
                        evt.MessageHeaderImage = filename;

                    }

                    if (file.Name.Equals("ReminderMsgHeaderImg"))
                    {
                        evt.ReminderMsgHeaderImg = filename;

                    }

                    if (file.Name.Equals("CongratulationMsgHeaderImg"))
                    {
                        evt.CongratulationMsgHeaderImg = filename;

                    }
                    if (file.Name.Equals("ResponseInterestedOfMarketingMsgHeaderImage"))
                    {
                        evt.ResponseInterestedOfMarketingMsgHeaderImage = filename;

                    }
                    if (file.Name.Equals("ResponseNotInterestedOfMarketingMsgHeaderImage"))
                    {
                        evt.ResponseNotInterestedOfMarketingMsgHeaderImage = filename;

                    }

                }

            }
            evt.CustomConfirmationTemplateWithVariables = events.CustomConfirmationTemplateWithVariables;
            evt.CustomCardTemplateWithVariables = events.CustomCardTemplateWithVariables;
            evt.CustomReminderTemplateWithVariables = events.CustomReminderTemplateWithVariables;
            evt.CustomCongratulationTemplateWithVariables = events.CustomCongratulationTemplateWithVariables;
            evt.SendingConfiramtionMessagesLinksLanguage = events.SendingConfiramtionMessagesLinksLanguage;
            evt.LinkGuestsCardText = events.LinkGuestsCardText;
            evt.LinkGuestsLocationEmbedSrc = events.LinkGuestsLocationEmbedSrc;
            evt.ConfirmationButtonsType = events.ConfirmationButtonsType;
            evt.ResponseInterestedOfMarketingMsg = events.ResponseInterestedOfMarketingMsg;
            evt.ResponseNotInterestedOfMarketingMsg = events.ResponseNotInterestedOfMarketingMsg;
            evt.SystemEventTitle = events.SystemEventTitle;
            evt.WhatsappProviderName = events.WhatsappProviderName ?? evt.WhatsappProviderName;
            evt.choosenSendingCountryNumber = events.choosenSendingCountryNumber ?? evt.choosenSendingCountryNumber;
            evt.choosenSendingWhatsappProfile = events.choosenSendingWhatsappProfile ?? evt.choosenSendingWhatsappProfile;
            evt.ChoosenNumberWithinCountry = events.ChoosenNumberWithinCountry == 0 ? evt.ChoosenNumberWithinCountry : events.ChoosenNumberWithinCountry;
            evt.ShowFailedSendingCongratulationLink = events.ShowFailedSendingCongratulationLink;
            evt.ShowFailedSendingEventLocationLink = events.ShowFailedSendingEventLocationLink;
            evt.FailedSendingConfiramtionMessagesLinksLanguage = events.FailedSendingConfiramtionMessagesLinksLanguage;
            evt.WhatsappConfirmation = events.WhatsappConfirmation;
            evt.WhatsappPush = events.WhatsappPush;
            evt.CreatedFor = events.CreatedFor;
            evt.FailedGuestsReminderMessage = events.FailedGuestsReminderMessage;
            evt.FailedGuestsCongratulationMsg = events.FailedGuestsCongratulationMsg;
            evt.FailedGuestsCardText = events.FailedGuestsCardText;
            evt.FailedGuestsLocationEmbedSrc = events.FailedGuestsLocationEmbedSrc;
            evt.FailedGuestsMessag = events.FailedGuestsMessag;
            evt.ConguratulationsMsgType = events.ConguratulationsMsgType;
            evt.ConguratulationsMsgTemplateName = events.ConguratulationsMsgTemplateName;
            evt.MessageHeaderText = events.MessageHeaderText;
            evt.CardInvitationTemplateType = events.CardInvitationTemplateType;
            evt.CustomCardInvitationTemplateName = events.CustomCardInvitationTemplateName;
            evt.CustomInvitationMessageTemplateName = events.CustomInvitationMessageTemplateName;
            evt.EventTitle = events.EventTitle;
            evt.ParentTitleGender = events.ParentTitleGender;
            evt.MessageLanguage = events.MessageLanguage;
            evt.EventDescription = events.EventDescription;
            evt.ParentTitle = events.ParentTitle;
            evt.Type = events.Type;
            evt.GmapCode = events.GmapCode;
            evt.EventVenue = events.EventVenue;
            evt.ModifiedBy = userId;
            evt.ModifiedOn = DateTime.UtcNow;
            evt.SendInvitation = events.SendInvitation;
            evt.CityId = events.CityId;
            evt.CityId = events.CityId;
            evt.AttendanceTime = events.EventFrom.Value + events.AttendanceTime.Value.TimeOfDay;
            evt.ShowOnCalender = events.ShowOnCalender;
            evt.LinkedEvent = events.LinkedEvent;
            evt.LeaveTime = events.LeaveTime.Value;
            evt.ContactName = events.ContactName;
            evt.ContactPhone = events.ContactPhone;
            evt.SendingType = events.SendingType;
            evt.ConguratulationsMsgSentOnNumber = events.ConguratulationsMsgSentOnNumber;
            evt.ReminderMessageTempName = events.ReminderMessageTempName;
            evt.ReminderMessage = events.ReminderMessage?.Replace("\r\n", "\\n").TrimEnd();
            evt.ThanksMessage = events.ThanksMessage?.Replace("\r\n", "\\n").TrimEnd();
            evt.EventFrom = events.EventFrom;
            evt.EventTo = events.EventTo;
            evt.ReminderTempId = events.ReminderTempId;
            evt.ThanksTempId = events.ThanksTempId;
            evt.DeclineTempId = events.DeclineTempId;

            Log.Information("Event {eId} edited by {uId}", evt.Id, userId);

            await _auditLogService.AddAsync(userId, events.Id, DAL.Enum.ActionEnum.UpdateEvent);
            await db.SaveChangesAsync();

            var eventOperators = await db.EventOperator.Where(e => e.EventId == evt.Id).ToListAsync();
            foreach (var eventOperator in eventOperators)
            {
                eventOperator.EventStart = evt.EventFrom;
                eventOperator.EventEnd = evt.EventTo;
            }
            db.EventOperator.UpdateRange(eventOperators);
            await db.SaveChangesAsync();
            await CreateReminderIcsFileAsync(evt);
            return RedirectToAction("ViewEvent", AppController.Admin, new { id = events.Id });
        }

        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> ViewEvent(int id)
        {
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            ViewBag.Icon = "nav-icon fas fa-pencil-square-o";
            ViewBag.CardImage = id + ".png";
            var cardInfo = await db.CardInfo.Where(p => p.EventId == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (cardInfo == null)
            {
                return RedirectToAction("QRSettings", "admin", new { id = id });
            }

            ViewBag.CardInfo = cardInfo;
            ViewBag.EventWhatsappProvider = await db.Events.Where(e => e.Id == id)
                .Select(e => e.WhatsappProviderName)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.CurrentUsedWhatsappProvider = await db.AppSettings
                .Select(e => e.WhatsappServiceProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var model = await db.VwEvents.Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            SetBreadcrum("Events", "/admin");
            return View("ViewEvent - Copy", model);
        }

        private async Task SetControls(Events entity)
        {
            ViewBag.CreatedForUserName = await db.Users
                .Where(u => u.UserId == entity.CreatedFor)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();

            ViewBag.Type = new SelectList(await db.EventCategory.ToListAsync(), "EventId", "Category");
            ViewBag.Icon = "nav-icon fas fa-pencil-square-o";
            SetBreadcrum("Events", "/admin");

            ViewBag.LinkedEvents = new SelectList(await db.Events.Where(p => p.EventTo >= DateTime.Today)
                                   .Select(x => new BasicData { Id = x.Id, Value = x.Id + "-" + x.EventTitle })
                                   .AsNoTracking()
                                   .ToListAsync(), "Id", "Value");

            ViewBag.GenderList = new SelectList(
                                            new List<SelectListItem>
                                            {
                                                new SelectListItem { Text = "Female", Value = "Female"},
                                                new SelectListItem { Text = "Male", Value = "Male"},
                                            }, "Value", "Text", entity.ParentTitleGender);


            ViewBag.MessageLanguage = new SelectList(
                                           new List<SelectListItem>
                                           {
                                                new SelectListItem { Text = "Twilio | Arabic", Value = "Twilio | Arabic"},
                                                new SelectListItem { Text = "Twilio | English", Value = "Twilio | English"},
                                                new SelectListItem { Text = "Twilio | Custom", Value = "Twilio | Custom"},
                                                new SelectListItem { Text = "Twilio | Template With Variables", Value = "Twilio | TemplateWithVariables"},

                                           }, "Value", "Text", entity.MessageLanguage);


            ViewBag.WhatsappProvider = new SelectList(
                                           new List<SelectListItem>
                                           {
                                                new SelectListItem { Text = "Default", Value = "Default"},
                                                new SelectListItem { Text = "Wati", Value = "Wati"},
                                                new SelectListItem { Text = "Twilio", Value = "Twilio"},
                                           }, "Value", "Text", entity.WhatsappProviderName);


            ViewBag.CardMessageTemplates = new SelectList(
                                           new List<SelectListItem>
                                           {
                                                new SelectListItem { Text = "Default", Value = "Default"},
                                                new SelectListItem { Text = "Twilio | Template With Variables", Value = "Twilio | TemplateWithVariables"},

                                           }, "Value", "Text", entity.CardInvitationTemplateType);


            ViewBag.ConguratulationsMsgTemplateName = new SelectList(
                                          new List<SelectListItem>
                                          {
                                                new SelectListItem { Text = "Custom", Value = "Custom"},
                                                new SelectListItem { Text = "Template With Variables", Value = "TemplateWithVariables"},
                                                new SelectListItem { Text = "Template (1)", Value = "Template (1)"},
                                                new SelectListItem { Text = "Template (2)", Value = "Template (2)"},
                                                new SelectListItem { Text = "Template (3)", Value = "Template (3)"},
                                                new SelectListItem { Text = "Template (4)", Value = "Template (4)"},
                                                new SelectListItem { Text = "Template (5)", Value = "Template (5)"},
                                                new SelectListItem { Text = "Template (6)", Value = "Template (6)"},
                                                new SelectListItem { Text = "Template (7)", Value = "Template (7)"},
                                                new SelectListItem { Text = "Template (8)", Value = "Template (8)"},
                                                new SelectListItem { Text = "Template (9)", Value = "Template (9)"},
                                                new SelectListItem { Text = "Template (10)", Value = "Template (10)"},


                                          }, "Value", "Text", entity.CardInvitationTemplateType);


            ViewBag.SendingType = new SelectList(
                                          new List<SelectListItem>
                                          {
                                                            new SelectListItem { Text = "Basic", Value = "Basic"},
                                                            new SelectListItem { Text = "With Guest Name", Value = "With Guest Name"},
                                          }, "Value", "Text", entity.SendingType);


            ViewBag.ReminderMessageTempName = new SelectList(
                              new List<SelectListItem>
                              {
                                                new SelectListItem { Text = "Custom", Value = "Custom"},
                                                new SelectListItem { Text = "Template With Variables", Value = "TemplateWithVariables"},
                                                new SelectListItem { Text = "Template (1)", Value = "Template (1)"},
                                                new SelectListItem { Text = "Template (2)", Value = "Template (2)"},
                                                new SelectListItem { Text = "Template (3)", Value = "Template (3)"},
                                                new SelectListItem { Text = "Template (1) With Calender ICS", Value = "Template (1) With Calender ICS"},
                                                new SelectListItem { Text = "Template (2) With Calender ICS", Value = "Template (2) With Calender ICS"},
                                                new SelectListItem { Text = "Template (3) With Calender ICS", Value = "Template (3) With Calender ICS"},

                              }, "Value", "Text", entity.ReminderMessageTempName);

            ViewBag.FailedSendingConfiramtionMessagesLinksLanguage = new SelectList(
                              new List<SelectListItem>
                              {

                                                new SelectListItem { Text = "Arabic", Value = "Arabic"},
                                                new SelectListItem { Text = "English", Value = "English"},

                              }, "Value", "Text", entity.FailedSendingConfiramtionMessagesLinksLanguage);

            ViewBag.ChoosenNumberWithinCountry = new SelectList(
                              new List<SelectListItem>
                              {

                                                new SelectListItem { Text = "First Number", Value = "1"},
                                                new SelectListItem { Text = "second Number", Value = "2"},

                              }, "Value", "Text", entity.ChoosenNumberWithinCountry);

            ViewBag.choosenSendingWhatsappProfile = new SelectList(await db.TwilioProfileSettings
                                .AsNoTracking()
                                .ToListAsync(),
                                 "Name", "Name");

            ViewBag.choosenSendingCountryNumber = new SelectList(
      new List<SelectListItem>
      {

                                                new SelectListItem { Text = "SAUDI", Value = "SAUDI"},
                                                new SelectListItem { Text = "KUWAIT", Value = "KUWAIT"},
                                                new SelectListItem { Text = "BAHRAIN", Value = "BAHRAIN"},

      }, "Value", "Text", entity.choosenSendingCountryNumber);



            ViewBag.EventLocations =
                new SelectList((await db.City.Include(c => c.Country).ToListAsync())
                .Select(e => new { e.Id, Location = e.CityName + "|" + e.Country.CountryName }), "Id", "Location");

            ViewBag.EventFrom = Convert.ToDateTime(entity.EventFrom).ToString("yyyy-MM-dd");
            ViewBag.EventTo = Convert.ToDateTime(entity.EventTo).ToString("yyyy-MM-dd");
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]

        public async Task<IActionResult> FetchCustomTemplates(int eventId, int typeId, [FromBody] CustomTemplates customTemplates)
        {
            var access = AccessService.AllowAccessForAdministratorAndOperatorOnly(this.HttpContext);
            if (access != null) return access;

            var evnt = await db.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            if (typeId == 1)
            {
                evnt.CustomInvitationMessageTemplateName = customTemplates.ConfirmationTemplate;
            }
            else if (typeId == 2)
            {
                evnt.CustomCardInvitationTemplateName = customTemplates.CardTemplate;
            }
            else if (typeId == 3)
            {
                evnt.ReminderTempId = customTemplates.ReminderTemplate;
            }
            else if (typeId == 4)
            {
                evnt.ThanksTempId = customTemplates.CongratulationTemplate;
            }
            db.Update(evnt);
            await db.SaveChangesAsync();
            try
            {
                var template = await _WhatsappSendingProvider
                    .SelectTwilioSendingProvider()
                    .GetTemplatesSync()
                    .GetCustomTemplateWithVariablesAsync(evnt, typeId);

                var matches = Regex.Matches(template, @"\{\{(\d+)\}\}")
                      .Cast<Match>()
                      .Select(m => int.Parse(m.Groups[1].Value))
                      .ToList();


                // Check if sequence is strictly ascending
                for (int i = 1; i < matches.Count; i++)
                {
                    if (matches[i] < matches[i - 1])
                        throw new Exception();
                }

                if (typeId == 1)
                {
                    evnt.CustomConfirmationTemplateWithVariables = template;
                }
                else if (typeId == 2)
                {
                    evnt.CustomCardTemplateWithVariables = template;
                }
                else if (typeId == 3)
                {
                    evnt.CustomReminderTemplateWithVariables = template;
                }
                else if (typeId == 4)
                {
                    evnt.CustomCongratulationTemplateWithVariables = template;
                }
                db.Update(evnt);
                await db.SaveChangesAsync();
                return Json(new { success = true, message = template, typeId = typeId });
            }
            catch
            {
                if (typeId == 1)
                {
                    evnt.CustomConfirmationTemplateWithVariables = string.Empty;
                }
                else if (typeId == 2)
                {
                    evnt.CustomCardTemplateWithVariables = string.Empty;
                }
                else if (typeId == 3)
                {
                    evnt.CustomReminderTemplateWithVariables = string.Empty;
                }
                else if (typeId == 4)
                {
                    evnt.CustomCongratulationTemplateWithVariables = string.Empty;
                }

                db.Events.Update(evnt);
                await db.SaveChangesAsync();
                return Json(new { success = false, typeId = typeId });
            }

        }
        public async Task<IActionResult> DeleteHeaderMessageImage(int eventId)
        {
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string path = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            var evnt = await db.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            var filename = evnt.MessageHeaderImage;
            var filePath = environment + path + "/" + filename;


            if (await _blobStorage.FileExistsAsync(filePath))
            {
                await _blobStorage.DeleteFileAsync(filePath, cancellationToken: default);
            }

            evnt.MessageHeaderImage = "";
            await db.SaveChangesAsync();

            return Redirect("~/admin/EditEvent/" + evnt.Id);


        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> DeleteReminderMessageImage(int eventId)
        {
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string path = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            var evnt = await db.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            var filename = evnt.ReminderMsgHeaderImg;
            var filePath = environment + path + "/" + filename;

            if (await _blobStorage.FileExistsAsync(filePath))
            {
                await _blobStorage.DeleteFileAsync(filePath, cancellationToken: default);
            }

            evnt.ReminderMsgHeaderImg = "";
            await db.SaveChangesAsync();

            return Redirect("~/admin/EditEvent/" + evnt.Id);


        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> DeleteInterestedMessageImage(int eventId)
        {
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string path = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            var evnt = await db.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            var filename = evnt.ResponseInterestedOfMarketingMsgHeaderImage;
            var filePath = environment + path + "/" + filename;

            if (await _blobStorage.FileExistsAsync(filePath))
            {
                await _blobStorage.DeleteFileAsync(filePath, cancellationToken: default);
            }

            evnt.ResponseInterestedOfMarketingMsgHeaderImage = "";
            await db.SaveChangesAsync();

            return Redirect("~/admin/EditEvent/" + evnt.Id);


        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> DeleteNotInterestedMessageImage(int eventId)
        {
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string path = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            var evnt = await db.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            var filename = evnt.ResponseNotInterestedOfMarketingMsgHeaderImage;
            var filePath = environment + path + "/" + filename;

            if (await _blobStorage.FileExistsAsync(filePath))
            {
                await _blobStorage.DeleteFileAsync(filePath, cancellationToken: default);
            }

            evnt.ResponseNotInterestedOfMarketingMsgHeaderImage = "";
            await db.SaveChangesAsync();

            return Redirect("~/admin/EditEvent/" + evnt.Id);


        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> DeleteCongratulationMessageImage(int eventId)
        {
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string path = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            var evnt = await db.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();
            var filename = evnt.CongratulationMsgHeaderImg;
            var filePath = environment + path + "/" + filename;

            if (await _blobStorage.FileExistsAsync(filePath))
            {
                await _blobStorage.DeleteFileAsync(filePath, cancellationToken: default);
            }
            evnt.CongratulationMsgHeaderImg = "";
            await db.SaveChangesAsync();

            return Redirect("~/admin/EditEvent/" + evnt.Id);


        }

        private string GenerateCalendarEventICS(Events evnt)
        {
            var location = "https://maps.app.goo.gl/" + evnt.GmapCode;

            // ??????? ????? ????? ?????? (UTC+3)
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");

            // ??? ??? ????? ????? (20:00 ?? 8 ?????)
            DateTime eventDate = evnt.EventFrom.Value.Date;
            DateTime eventStartTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 20, 0, 0);

            // ??? ????? ????? (23:00 ?? 11 ?????)
            DateTime eventEndTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 23, 0, 0);

            // ??? ??????? ??? ?????
            TimeSpan alertOffset = TimeSpan.FromDays(2);

            // ??????? ?????? UTC
            DateTime utcNow = DateTime.UtcNow;

            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("METHOD:PUBLISH");

            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{Guid.NewGuid()}");
            sb.AppendLine($"DTSTAMP:{utcNow:yyyyMMddTHHmmssZ}");

            // ??? ??????? ???????? ?????? ????? ??????
            sb.AppendLine($"DTSTART;TZID=Arab Standard Time:{eventStartTime:yyyyMMddTHHmmss}");
            sb.AppendLine($"DTEND;TZID=Arab Standard Time:{eventEndTime:yyyyMMddTHHmmss}");

            sb.AppendLine($"SUMMARY:{evnt.SystemEventTitle}");
            sb.AppendLine($"LOCATION:{evnt.EventVenue}");

            // ????? ??? ?????
            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine($"TRIGGER:-P2D"); // P2D = ??? ?????
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine("DESCRIPTION:Reminder");
            sb.AppendLine("END:VALARM");

            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }


        public async Task SaveReminderIcsFileAsync(string icsContent, string fileName)
        {

            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string ReminderIcsFolder = _configuration.GetSection("Uploads").GetSection("ReminderIcs").Value;
            string filePath = environment + ReminderIcsFolder + "/" + fileName + ".ics";

            try
            {

                if (await _blobStorage.FileExistsAsync(filePath))
                {
                    await _blobStorage.DeleteFileAsync(filePath, cancellationToken: default);
                }

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(icsContent)))
                {
                    await _blobStorage.UploadAsync(stream, "ics", filePath, cancellationToken: default);
                }
            }
            catch (IOException ioEx)
            {

            }

        }

        private async Task CreateReminderIcsFileAsync(Events evnt)
        {
            var icsContent = GenerateCalendarEventICS(evnt);

            await SaveReminderIcsFileAsync(icsContent, evnt.Id.ToString());

        }


        private bool HasOperatorRole()
        {
            var userRole = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Operator")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool HasOperatorOrSupervisorRole()
        {
            var userRole = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Operator" || userRole == "Supervisor")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool HasSupervisorRole()
        {
            var userRole = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Supervisor")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<List<EventOperator>> GetOperatorUpcomingAndInProgressEvents()
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return await db.EventOperator
                 .Include(eo => eo.Event)
                 .Where(e => e.OperatorId == userId
                     && e.Event.IsDeleted != true
                     && (e.EventEnd >= DateTime.Now))
                 .ToListAsync();
        }

        private async Task<List<EventOperator>> GetOperatorPastEvents()
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return await db.EventOperator
            .Include(eo => eo.Event)
            .Where(eo => eo.OperatorId == userId
                && eo.Event.IsDeleted != true
                && eo.EventStart < DateTime.Now.Date
                && eo.EventEnd < DateTime.Now.Date)
            .ToListAsync();
        }

        private async Task<List<EventOperator>> GetOperatorInprogressEvents()
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return await db.EventOperator
           .Include(eo => eo.Event)
           .Where(eo => eo.OperatorId == userId
               && eo.Event.IsDeleted != true
               && eo.EventStart <= DateTime.Now
               && eo.EventEnd >= DateTime.Now)
           .ToListAsync();
        }

        private async Task<List<EventOperator>> GetOperatorUpcomingEvents()
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return await db.EventOperator
            .Include(eo => eo.Event)
            .Where(eo => eo.OperatorId == userId
                && eo.Event.IsDeleted != true
                && eo.EventStart > DateTime.Now)
            .ToListAsync();
        }
        private async Task<List<EventOperator>> GetOperatorAllEvents()
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return await db.EventOperator
            .Include(eo => eo.Event)
            .Where(eo => eo.OperatorId == userId
                && eo.Event.IsDeleted != true)
            .ToListAsync();
        }
    }
}
