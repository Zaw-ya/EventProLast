using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using EventPro.Business.Storage.Interface;
using EventPro.DAL.Common;
using EventPro.DAL.Extensions;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.Web.Common;
using EventPro.Web.Extensions;
using EventPro.Web.Filters;
using EventPro.Web.Models;
using EventPro.Web.Services;

using FirebaseAdmin;
using FirebaseAdmin.Messaging;

using Google.Apis.Auth.OAuth2;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Serilog;

namespace EventPro.Web.Controllers
{
    /// <summary>
    /// Partial controller for managing Events in the Admin panel.
    /// Contains all event-related CRUD operations, filtering, and management actions.
    /// </summary>
    public partial class AdminController : Controller
    {
        #region Event List & Display Actions

        /// <summary>
        /// Displays the main events list page with active (non-deleted) events.
        /// Accessible by: Administrator, Operator, Agent, Supervisor, Accounting
        /// </summary>
        /// <returns>Events list view</returns>
        /// important note:
        // We didnt use getguest here because we need to show all events not specific to an operator
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> Events()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");
            // Used for checking if invoice file exists in the view
            ViewBag.FilePath = _configuration.GetSection("Uploads").GetSection("Invoice").Value;

            //var model = await db.VwEvents
            //    .Where(e => e.EventTo >= DateTime.Now && e.IsDeleted != true)
            //    .OrderByDescending(e => e.Id)
            //    .AsNoTracking()
            //    .ToListAsync();

            return View("Events - Copy");
        }

        /// <summary>
        /// API endpoint to fetch active events for DataTables with server-side pagination.
        /// Supports search filtering by ID, linked event, dates, title, venue, creator name, and status.
        /// Special search values: "upcoming" (future events), "in-progress" (current events).
        /// </summary>
        /// <returns>JSON with paginated event data for DataTables</returns>
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

            // Read the VwEvents view for events that are not deleted and with filtering
            // From the database view called Vw_Events (see DAL.Models.VwEvents)
            // The filtering is done based on the search value provided in the request
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

            // Operators only see events they are assigned to (direct or bulk-shared)
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var operatorEventIds = db.EventOperator
                    .Where(e => e.OperatorId == userId)
                    .Select(e => e.EventId)
                    .ToList();
                events_ = events_.Where(e => operatorEventIds.Contains(e.Id));
            }

            try
            {
                var result = await events_.Skip(skip).Take(pageSize).ToListAsync();
                // Prepare the view models to be sent to the client
                // result is the list of database models (VwEvents)
                List<EventVM> eventsVM = new(); // view models
                foreach (var evnt in result)
                {
                    EventVM eventVM = new(evnt); // from the database model to the view model
                    eventsVM.Add(eventVM);
                }

                var recordsTotal = await events_.CountAsync();
                // draw is a parameter sent by DataTables to ensure each response corresponds to the correct request
                var draw = Request.Form["draw"].FirstOrDefault();
                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal,
                    data = eventsVM
                };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                // Ghrabawy : TODO : Log the exception
                // Return a 500 Internal Server Error response with the exception details
                return StatusCode(500, new { message = ex.Message, stack = ex.StackTrace });
            }
        }

        /// <summary>
        /// Displays the archived (past) events list page.
        /// Shows events where EventTo date is in the past.
        /// Accessible by: Administrator, Operator, Agent, Supervisor, Accounting
        /// </summary>
        /// <returns>Archive events view</returns>
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public IActionResult ArchiveEvents()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Events", "/admin");

            return View("ArchiveEvents - Copy");
        }

        /// <summary>
        /// API endpoint to fetch archived (past) events for DataTables.
        /// Returns events where EventTo is less than current date and not deleted.
        /// </summary>
        /// <returns>JSON with paginated archived event data</returns>
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

        /// <summary>
        /// Displays detailed view of a specific event.
        /// Validates operator access if the user has Operator role.
        /// Redirects to QRSettings if card info doesn't exist for the event.
        /// </summary>
        /// <param name="id">Event ID to view</param>
        /// <returns>Event details view or redirect</returns>
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

            if (model == null)
            {
                Log.Warning("ViewEvent: Event {EventId} not found", id);
                return NotFound("Event not found.");
            }

            ViewBag.Icon = await db.CardInfo.Where(p => p.EventId == id).Select(p => p.BackgroundImage)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            Log.Information("ViewEvent: ID={EventId}, Title={EventTitle}", id, model.SystemEventTitle);

            SetBreadcrum("Events", "/admin");
            return View("ViewEvent - Copy", model);
        }

        /// <summary>
        /// POST action for filtering events list with additional parameters.
        /// Supports filtering by client name, event status, and date range.
        /// </summary>
        /// <param name="collection">Form collection with filter parameters</param>
        /// <returns>Filtered events view</returns>
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
            return View("Events", events_);
        }

        #endregion

        #region Event CRUD Operations

        /// <summary>
        /// Displays the create new event form.
        /// Populates dropdown lists for event type, clients, linked events, and locations.
        /// For Operators, only shows clients they have access to.
        /// </summary>
        /// <returns>Create event form view</returns>
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
                p.Role == RoleIds.Client)
                    .OrderByDescending(e => e.UserId)
                    .ToListAsync();
            }
            else
            {
                clients = await db.Users.Where(p => p.Role == RoleIds.Client)
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

        /// <summary>
        /// Creates a new event in the database.
        /// Validates date range and description, uploads media files to Blob Storage,
        /// sets default values, creates event-operator mapping, sends Firebase notification
        /// if ShowOnCalender is enabled, and generates reminder ICS file.
        /// </summary>
        /// <param name="events">Event model from form submission</param>
        /// <returns>Redirect to events list on success, or form with errors</returns>
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
            string filename = string.Empty;
            bool hasFile = false;

            foreach (var file in files)
            {
                try
                {
                    using (var stream = file.OpenReadStream())
                    {
                        if (file.ContentType.Contains("image"))
                        {
                            filename = await _blobStorage.UploadAsync(stream, file.ContentType, $"events/{file.FileName}", CancellationToken.None);
                        }
                        else if (file.ContentType.Contains("video"))
                        {
                            filename = await _blobStorage.UploadAsync(stream, file.ContentType, $"events/{file.FileName}", CancellationToken.None);
                        }
                        else if (file.ContentType.Contains("application/pdf"))
                        {
                            filename = await _blobStorage.UploadAsync(stream, file.ContentType, $"events/{file.FileName}", CancellationToken.None);
                        }

                        if (!string.IsNullOrEmpty(filename))
                        {
                            hasFile = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Create Event: File upload failed for file {FileName}", file.FileName);
                    ModelState.AddModelError(string.Empty, ex.Message);
                    // Re-populate ViewBag data
                    ViewBag.Type = new SelectList(db.EventCategory.ToList(), "EventId", "Category");
                    // Gharabawy : Here we use the "VwUsers" to get only the clients
                    ViewBag.Users = new SelectList(db.VwUsers.Where(p => p.RoleName == "Client").ToList(), "UserId", "UserName");
                    ViewBag.Icon = "nav-icon fas fa-calendar";
                    ViewBag.LinkedEvents = new SelectList(await db.Events.Where(p => p.EventTo >= DateTime.Today).Select(x => new BasicData { Id = x.Id, Value = x.Id + "-" + x.EventTitle }).AsNoTracking().ToListAsync(), "Id", "Value");
                    ViewBag.EventLocations = new SelectList((await db.City.Include(c => c.Country).AsNoTracking().ToListAsync()).Select(e => new { e.Id, Location = e.CityName + "|" + e.Country.CountryName }), "Id", "Location");
                    SetBreadcrum("Events", "/admin");
                    return View("EditEvent - Copy", events);
                }
            }
            events.SendingConfiramtionMessagesLinksLanguage = "Arabic";
            events.LinkGuestsCardText = "بطاقة الضيوف للحفلة هنا";
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
            // events.choosenSendingWhatsappProfile = settings.WhatsappDefaultTwilioProfile;
            City city = await db.City.Where(e => e.Id == events.CityId)
                .Include(e => e.Country)
                .FirstOrDefaultAsync();
            if (city.Country.CountryName.Contains("مصر") || city.Country.CountryName.Contains("Egypt"))
            {
                events.choosenSendingCountryNumber = "EGYPT";
            }
            else if (city.Country.CountryName.Contains("الكويت"))
            {
                events.choosenSendingCountryNumber = "KUWAIT";
            }
            else if (city.Country.CountryName.Contains("البحرين"))
            {
                events.choosenSendingCountryNumber = "BAHRAIN";
            }
            else
            {
                events.choosenSendingCountryNumber = "SAUDI";
            }

            Log.Information("Creating new event: Title={EventTitle}, Venue={Venue}, City={CityId}, Country={Country}, From={From}, To={To}, CreatedBy={UserId}",
                events.EventTitle, events.EventVenue, events.CityId, events.choosenSendingCountryNumber, events.EventFrom, events.EventTo, userId);

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

            // Audit Log
            await _auditLogService.AddAsync(userId, events.Id);

            #region Firebase FCM - Send push notification on event creation
            // Send FCM push notification to all devices subscribed to the event's city topic.
            // Firebase Admin SDK is initialized in Startup.cs using the JSON key from appsettings ("FireBaseJSON").
            // This block uses FirebaseMessaging.DefaultInstance which relies on that initialization.
            try
            {
                if (events.ShowOnCalender == true)
                {
                    // Build the notification payload targeting the city topic
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
                    Log.Information("Firebase FCM: Push notification sent for event {EventId} to topic {Topic}, result: {Result}", events.Id, request.Topic, result);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Firebase FCM: Failed to send push notification for event {EventId}", events.Id);
            }
            #endregion

            await CreateReminderIcsFileAsync(events);
            Log.Information("Event created successfully: ID={EventId}, Title={EventTitle}, CreatedBy={UserId}", events.Id, events.EventTitle, userId);

            // Redirect to Events Page
            return RedirectToAction(AppAction.Events, AppController.Admin);
        }

        /// <summary>
        /// Displays the edit event form for an existing event.
        /// Validates operator access if the user has Operator role.
        /// Converts stored newline characters for display in textarea fields.
        /// </summary>
        /// <param name="id">Event ID to edit</param>
        /// <returns>Edit event form view</returns>
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

        /// <summary>
        /// Updates an existing event in the database.
        /// Validates date range and description, handles file uploads to Blob Storage,
        /// updates all event properties, syncs event operator dates,
        /// and regenerates the reminder ICS file.
        /// </summary>
        /// <param name="events">Event model from form submission</param>
        /// <returns>Redirect to view event on success, or form with errors</returns>
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
                string filename = string.Empty;
                bool hasFile = false;
                foreach (var file in files)
                {
                    try
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            if (file.ContentType.Contains("image"))
                            {
                                filename = await _blobStorage.UploadAsync(stream, file.ContentType, $"events/{file.FileName}", CancellationToken.None);
                            }
                            else if (file.ContentType.Contains("video"))
                            {
                                filename = await _blobStorage.UploadAsync(stream, file.ContentType, $"events/{file.FileName}", CancellationToken.None);
                            }
                            else if (file.ContentType.Contains("application/pdf"))
                            {
                                filename = await _blobStorage.UploadAsync(stream, file.ContentType, $"events/{file.FileName}", CancellationToken.None);
                            }

                            if (!string.IsNullOrEmpty(filename))
                            {
                                hasFile = true;

                                if (file.Name.Equals("MessageHeaderImage"))
                                {
                                    evt.MessageHeaderImage = filename;
                                }
                                else if (file.Name.Equals("ReminderMsgHeaderImg"))
                                {
                                    evt.ReminderMsgHeaderImg = filename;
                                }
                                else if (file.Name.Equals("CongratulationMsgHeaderImg"))
                                {
                                    evt.CongratulationMsgHeaderImg = filename;
                                }
                                else if (file.Name.Equals("ResponseInterestedOfMarketingMsgHeaderImage"))
                                {
                                    evt.ResponseInterestedOfMarketingMsgHeaderImage = filename;
                                }
                                else if (file.Name.Equals("ResponseNotInterestedOfMarketingMsgHeaderImage"))
                                {
                                    evt.ResponseNotInterestedOfMarketingMsgHeaderImage = filename;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Edit Event: File upload failed for event {EventId}, file {FileName}", events.Id, file.FileName);
                        await SetControls(events);
                        ModelState.AddModelError(string.Empty, ex.Message);
                        return View("EditEvent", events);
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

            Log.Information("Event edited: ID={EventId}, Title={EventTitle}, Venue={Venue}, City={CityId}, Profile={Profile}, EditedBy={UserId}",
                evt.Id, evt.EventTitle, evt.EventVenue, evt.CityId, evt.choosenSendingWhatsappProfile, userId);

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
            #region Firebase FCM - Send push notification on event creation
            // Send FCM push notification to all devices subscribed to the event's city topic.
            // Firebase Admin SDK is initialized in Startup.cs using the JSON key from appsettings ("FireBaseJSON").
            // This block uses FirebaseMessaging.DefaultInstance which relies on that initialization.
            try
            {
                if (events.ShowOnCalender == true)
                {
                    // Build the notification payload targeting the city topic
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
                    Log.Information("Firebase FCM: Push notification sent for edited event {EventId} to topic {Topic}, result: {Result}", events.Id, request.Topic, result);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Firebase FCM: Failed to send push notification for event {EventId}", events.Id);
            }
            #endregion

            await CreateReminderIcsFileAsync(evt);
            return RedirectToAction("ViewEvent", AppController.Admin, new { id = events.Id });
        }

        #endregion

        #region Event Delete & Restore Actions

        /// <summary>
        /// Soft deletes an event by setting IsDeleted flag to true.
        /// Also removes all gatekeeper mappings for the event and hides from calendar.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="id">Event ID to delete</param>
        /// <returns>Success JSON response or NotFound</returns>
        [AuthorizeRoles("Administrator")]
        [HttpGet()]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await db.Events.FindAsync(id);
            if (eventItem == null)
            {
                Log.Warning("DeleteEvent: Event {EventId} not found", id);
                return NotFound(new { message = "Event not found." });
            }

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

            Log.Information("Event deleted: ID={EventId}, Title={EventTitle}, GatekeepersRemoved={GKCount}, DeletedBy={UserId}",
                id, eventItem.EventTitle, listOfEventGatekeeper.Count, userId);

            return Ok(new { success = true, message = "Event deleted successfully" });
        }

        /// <summary>
        /// Displays the deleted events list page.
        /// Shows events that have been soft-deleted (IsDeleted = true).
        /// Only accessible by Administrator.
        /// </summary>
        /// <returns>Deleted events view</returns>
        [AuthorizeRoles("Administrator")]
        public IActionResult ShowDeletedEvents()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Deleted Events", "/admin");

            return View("DeletedEvent");
        }

        /// <summary>
        /// API endpoint to fetch deleted events for DataTables.
        /// Returns events where IsDeleted is true with support for pagination and filtering.
        /// Only accessible by Administrator.
        /// </summary>
        /// <returns>JSON with paginated deleted event data</returns>
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

        /// <summary>
        /// Restores a soft-deleted event by setting IsDeleted flag to false.
        /// Clears the DeletedOn and DeletedBy fields.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="id">Event ID to restore</param>
        /// <returns>OK response or NotFound</returns>
        [AuthorizeRoles("Administrator")]
        [HttpGet()]
        public async Task<IActionResult> RestoreEvent(int id)
        {
            var eventItem = await db.Events.FindAsync(id);
            if (eventItem == null)
            {
                Log.Warning("RestoreEvent: Event {EventId} not found", id);
                return NotFound(new { message = "Event not found or not deleted." });
            }

            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            eventItem.IsDeleted = false;
            eventItem.DeletedOn = null;
            eventItem.DeletedBy = null;

            await db.SaveChangesAsync();
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.RestoreEvent);

            Log.Information("Event restored: ID={EventId}, Title={EventTitle}, RestoredBy={UserId}", id, eventItem.EventTitle, userId);

            return Ok();
        }

        #endregion

        #region Events By Operator Actions

        /// <summary>
        /// Displays events filtered by operator page.
        /// Shows a list of events with filter options by operator, date range.
        /// Only accessible by Administrator.
        /// </summary>
        /// <returns>Events by operator view</returns>
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

        /// <summary>
        /// POST action for filtering events by operator.
        /// Filters events based on operator (createdBy), date range, and event status.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="collection">Form collection with filter parameters</param>
        /// <returns>Filtered events by operator view</returns>
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

        #endregion

        #region Events By Gatekeeper Actions

        /// <summary>
        /// Displays events filtered by gatekeeper page (legacy version).
        /// Shows events with their assigned gatekeepers and filter options.
        /// Accessible by Administrator and Agent.
        /// </summary>
        /// <returns>Events by gatekeeper view</returns>
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

        /// <summary>
        /// POST action for filtering events by gatekeeper (legacy version).
        /// Filters by gatekeeper, city, event type (active/archived), date range, and assignment status.
        /// Accessible by Administrator and Agent.
        /// </summary>
        /// <param name="collection">Form collection with filter parameters</param>
        /// <returns>Filtered events by gatekeeper view</returns>
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

        /// <summary>
        /// Displays events by gatekeeper page with DataTables support.
        /// Provides filter dropdowns for gatekeeper, address, event type, and dates.
        /// Only accessible by Administrator.
        /// </summary>
        /// <returns>Events by gatekeeper view with DataTables</returns>
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

        /// <summary>
        /// API endpoint for filtering events by gatekeeper with server-side pagination.
        /// Supports filtering by event title, ID, city, type, date range, assignment status, and gatekeeper.
        /// Returns JSON data for DataTables.
        /// </summary>
        /// <returns>JSON with filtered event data for DataTables</returns>
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

        #endregion

        #region Gatekeeper Check-in/Check-out Actions

        /// <summary>
        /// Displays gatekeeper check-in/check-out history page (legacy version).
        /// Shows check-in and check-out logs for gatekeepers at events.
        /// Only accessible by Administrator.
        /// </summary>
        /// <returns>Gatekeeper checks view</returns>
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

        /// <summary>
        /// POST action for filtering gatekeeper check-in/check-out history (legacy version).
        /// Filters by gatekeeper, city, check type, and date range.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="collection">Form collection with filter parameters</param>
        /// <returns>Filtered gatekeeper checks view</returns>
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
        /// Displays gatekeeper check-in/check-out history page with DataTables support.
        /// Provides filter dropdowns for gatekeeper, address, and check type.
        /// Accessible by Administrator and Agent.
        /// </summary>
        /// <returns>Gatekeeper checks view with DataTables</returns>
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

        /// <summary>
        /// API endpoint for filtering gatekeeper check-in/check-out logs with server-side pagination.
        /// Supports filtering by event title, ID, gatekeeper, city, type, check type, and date range.
        /// Returns JSON data for DataTables with support for multiple sort columns.
        /// </summary>
        /// <returns>JSON with filtered gatekeeper check data for DataTables</returns>
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
                    Icon = gkh.Event.CardInfo.Select(c => c.BackgroundImage).FirstOrDefault(),
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

        #endregion

        #region Guest Management Actions

        /// <summary>
        /// Deletes all guests for a specific event.
        /// Removes guest records from database and deletes associated card preview files from blob storage.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="id">Event ID to delete guests from</param>
        /// <returns>OK response</returns>
        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> DeleteAllGuests(int eventId)
        {
            var guests = await db.Guest.Where(e => e.EventId == eventId)
                .AsNoTracking()
                .ToListAsync();
            db.Guest.RemoveRange(guests);
            await db.SaveChangesAsync();
            //string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            //string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            //await _blobStorage.DeleteFolderAsync(environment + cardPreview + "/" + id + "/", cancellationToken: default);

            // Delete from Blob Storage
            var prefix = $"cards/{eventId}/";

            await _blobStorage.DeleteFolderAsync(prefix, CancellationToken.None);

            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, eventId, DAL.Enum.ActionEnum.DeleteAllGuests);

            Log.Information(
               "Guest cards deletion executed. EventId={EventId}, DeletedBy={UserId}",
               eventId,
               userId);

            Log.Information("All guests deleted: EventId={EventId}, GuestsRemoved={Count}, DeletedBy={UserId}", eventId, guests.Count, userId);

            return Ok();
        }

        /// <summary>
        /// Deletes all guest invitation card files for a specific event from blob storage.
        /// Does not affect guest records in database.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="id">Event ID to delete cards from</param>
        /// <returns>OK response</returns>
        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> DeleteAllGuestsCards(int eventId)
        {
            try
            {
                // Validate event exists
                var eventExists = await db.Events
                    .AsNoTracking()
                    .AnyAsync(e => e.Id == eventId);

                if (!eventExists)
                {
                    Log.Warning("DeleteAllGuestsCards failed - Event not found. EventId={EventId}", eventId);
                    return NotFound("Event not found.");
                }

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    Log.Warning("DeleteAllGuestsCards failed - Unauthorized access. EventId={EventId}", eventId);
                    return Unauthorized();
                }

                var userId = int.Parse(userIdClaim);

                // Delete from Blob Storage
                var prefix = $"cards/{eventId}/";

                await _blobStorage.DeleteFolderAsync(prefix, CancellationToken.None);

                Log.Information(
                    "Guest cards deletion executed. EventId={EventId}, DeletedBy={UserId}",
                    eventId,
                    userId);

                // Audit log
                await _auditLogService.AddAsync(
                    userId,
                    eventId,
                    DAL.Enum.ActionEnum.DeleteAllGuestsCards);

                return Ok(new { success = true });
            }
            catch (DbUpdateException dbEx)
            {
                Log.Error(dbEx,
                    "Database error while deleting guest cards. EventId={EventId}",
                    eventId);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Database error occurred while logging the operation.");
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "Unexpected error while deleting guest cards. EventId={EventId}",
                    eventId);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Unexpected error occurred.");
            }
        }

        /// <summary>
        /// Resets all message status fields for all guests of an event.
        /// Clears confirmation, card, reminder, congratulation, and location message statuses.
        /// Allows all messages to be resent.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="id">Event ID to reset guest statuses</param>
        /// <returns>OK response</returns>
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

            Log.Information("All guest statuses reset: EventId={EventId}, GuestsAffected={Count}, ResetBy={UserId}", id, guests.Count, userId);

            return Ok();
        }

        /// <summary>
        /// Resets confirmation message status for all guests of an event.
        /// Allows confirmation messages to be resent to all guests.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>OK response</returns>
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

            Log.Information("Confirmation messages reset: EventId={EventId}, GuestsAffected={Count}, ResetBy={UserId}", id, guests.Count, userId);

            return Ok();
        }

        /// <summary>
        /// Resets invitation card message status for all guests of an event.
        /// Allows card messages to be resent to all guests.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>OK response</returns>
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

            Log.Information("Card messages reset: EventId={EventId}, GuestsAffected={Count}, ResetBy={UserId}", id, guests.Count, userId);

            return Ok();
        }

        /// <summary>
        /// Resets event location message status for all guests of an event.
        /// Allows location messages to be resent to all guests.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>OK response</returns>
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

            Log.Information("Event location messages reset: EventId={EventId}, GuestsAffected={Count}, ResetBy={UserId}", id, guests.Count, userId);

            return Ok();
        }

        /// <summary>
        /// Resets reminder message status for all guests of an event.
        /// Allows reminder messages to be resent to all guests.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>OK response</returns>
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

            Log.Information("Reminder messages reset: EventId={EventId}, GuestsAffected={Count}, ResetBy={UserId}", id, guests.Count, userId);

            return Ok();
        }

        /// <summary>
        /// Resets congratulation message status for all guests of an event.
        /// Allows congratulation messages to be resent to all guests.
        /// Only accessible by Administrator.
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>OK response</returns>
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

            Log.Information("Congratulation messages reset: EventId={EventId}, GuestsAffected={Count}, ResetBy={UserId}", id, guests.Count, userId);

            return Ok();
        }

        #endregion

        #region WhatsApp Template Management Actions

        /// <summary>
        /// Fetches and stores custom WhatsApp message templates for an event.
        /// Supports 4 template types: Confirmation (1), Card (2), Reminder (3), Congratulation (4).
        /// Validates template variable sequence and stores the template content in database.
        /// </summary>
        /// <param name="eventId">Event ID to update</param>
        /// <param name="typeId">Template type (1-4)</param>
        /// <param name="customTemplates">Template names from request body</param>
        /// <returns>JSON with success status and template content</returns>
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

        #endregion

        #region Event Image Management Actions

        /// <summary>
        /// Deletes the message header image for an event from blob storage.
        /// Clears the MessageHeaderImage field in the database.
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Redirect to edit event page</returns>
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

        /// <summary>
        /// Deletes the reminder message header image for an event from blob storage.
        /// Clears the ReminderMsgHeaderImg field in the database.
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Redirect to edit event page</returns>
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

        /// <summary>
        /// Deletes the marketing interested response image for an event from blob storage.
        /// Clears the ResponseInterestedOfMarketingMsgHeaderImage field in the database.
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Redirect to edit event page</returns>
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

        /// <summary>
        /// Deletes the marketing not interested response image for an event from blob storage.
        /// Clears the ResponseNotInterestedOfMarketingMsgHeaderImage field in the database.
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Redirect to edit event page</returns>
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

        /// <summary>
        /// Deletes the congratulation message header image for an event from blob storage.
        /// Clears the CongratulationMsgHeaderImg field in the database.
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Redirect to edit event page</returns>
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

        #endregion

        #region Calendar ICS File Generation

        /// <summary>
        /// Generates an ICS (iCalendar) file content for event reminders.
        /// Creates a calendar event with default times (20:00-23:00) in Arab Standard Time
        /// and includes a 2-day reminder alarm.
        /// </summary>
        /// <param name="evnt">Event to generate ICS for</param>
        /// <returns>ICS file content as string</returns>
        private string GenerateCalendarEventICS(Events evnt)
        {
            //var location = "https://maps.app.goo.gl/" + evnt.GmapCode;

            // Arab Standard Time zone (UTC+3)
            //TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");

            // Default party start time (20:00 or 8 PM)
            DateTime eventDate = evnt.EventFrom.Value.Date;
            DateTime eventStartTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 20, 0, 0);

            // Party end time (23:00 or 11 PM)
            DateTime eventEndTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 23, 0, 0);
            // Alert 2 days before
            //TimeSpan alertOffset = TimeSpan.FromDays(2);

            // Current UTC time
            DateTime utcNow = DateTime.UtcNow;

            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("METHOD:PUBLISH");

            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{Guid.NewGuid()}");
            sb.AppendLine($"DTSTAMP:{utcNow:yyyyMMddTHHmmssZ}");

            // Start and end times in standard calendar format
            sb.AppendLine($"DTSTART;TZID=Arab Standard Time:{eventStartTime:yyyyMMddTHHmmss}");
            sb.AppendLine($"DTEND;TZID=Arab Standard Time:{eventEndTime:yyyyMMddTHHmmss}");

            sb.AppendLine($"SUMMARY:{evnt.EventTitle}");
            sb.AppendLine($"LOCATION:{evnt.LinkGuestsLocationEmbedSrc}");

            // Reminder 2 days before
            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine($"TRIGGER:-P2D"); // P2D = 2 days before
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine("DESCRIPTION:Reminder");
            sb.AppendLine("END:VALARM");

            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        /// <summary>
        /// Saves the ICS file content to Blob Storage.
        /// Uploads the ICS file and returns the Blob Storage URL.
        /// </summary>
        /// <param name="icsContent">ICS file content</param>
        /// <param name="fileName">File name (without extension)</param>
        /// <returns>Blob Storage URL of the uploaded ICS file</returns>
        public async Task<string> SaveReminderIcsFileAsync(string icsContent, string fileName)
        {
            try
            {
                // Convert ICS content to memory stream
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(icsContent));

                // Upload to Blob Storage in 'ics' folder
                var icsUrl = await _blobStorage.UploadAsync(stream, "text/calendar", $"ics/{fileName}.ics", CancellationToken.None);

                Log.Information($"ICS file uploaded to Blob Storage: {icsUrl}");

                return icsUrl;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to upload ICS file to Blob Storage: {ex.Message}");
                return null;
            }
        }

        /*
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
         */
        /// <summary>
        /// Creates and saves a reminder ICS file for an event.
        /// Combines ICS generation and storage operations.
        /// </summary>
        /// <param name="evnt">Event to create reminder for</param>
        private async Task CreateReminderIcsFileAsync(Events evnt)
        {
            var icsContent = GenerateCalendarEventICS(evnt);

            await SaveReminderIcsFileAsync(icsContent, evnt.Id.ToString());

        }

        #endregion

        #region Helper Methods - ViewBag Setup

        /// <summary>
        /// Populates ViewBag with all dropdown lists and settings required for the Edit Event form.
        /// Sets up: event types, linked events, gender options, message languages, WhatsApp providers,
        /// template options, sending types, locations, and date formatting.
        /// </summary>
        /// <param name="entity">Event entity for pre-selecting current values</param>
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
                                                new SelectListItem { Text = "EGYPT", Value = "EGYPT"},
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

        #endregion

        #region Helper Methods - Role Checking

        /// <summary>
        /// Checks if the current user has the Operator role.
        /// </summary>
        /// <returns>True if user is an Operator, false otherwise</returns>
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

        /// <summary>
        /// Checks if the current user has either Operator or Supervisor role.
        /// </summary>
        /// <returns>True if user is an Operator or Supervisor, false otherwise</returns>
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

        /// <summary>
        /// Checks if the current user has the Supervisor role.
        /// </summary>
        /// <returns>True if user is a Supervisor, false otherwise</returns>
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

        #endregion

        #region Helper Methods - Operator Event Queries

        /// <summary>
        /// Gets all upcoming and in-progress events for the current operator.
        /// Returns events where EventEnd is in the future.
        /// </summary>
        /// <returns>List of event operators with their events</returns>
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

        /// <summary>
        /// Gets all past events for the current operator.
        /// Returns events where both EventStart and EventEnd are in the past.
        /// </summary>
        /// <returns>List of event operators with their past events</returns>
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

        /// <summary>
        /// Gets all currently in-progress events for the current operator.
        /// Returns events where current time is between EventStart and EventEnd.
        /// </summary>
        /// <returns>List of event operators with their in-progress events</returns>
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

        /// <summary>
        /// Gets all upcoming events for the current operator.
        /// Returns events where EventStart is in the future.
        /// </summary>
        /// <returns>List of event operators with their upcoming events</returns>
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

        /// <summary>
        /// Gets all events (regardless of status) for the current operator.
        /// Returns all non-deleted events assigned to the operator.
        /// </summary>
        /// <returns>List of event operators with all their events</returns>
        private async Task<List<EventOperator>> GetOperatorAllEvents()
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return await db.EventOperator
            .Include(eo => eo.Event)
            .Where(eo => eo.OperatorId == userId
                && eo.Event.IsDeleted != true)
            .ToListAsync();
        }

        #endregion
    }
}
