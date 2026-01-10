using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.RabbitMQMessaging.Interface;
using EventPro.Business.Storage.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Common.Interfaces;
using EventPro.DAL.Extensions;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.Services.AuditLogService.Interface;
using EventPro.Services.WatiService.Interface;
using EventPro.Web.Common;
using EventPro.Web.Filters;
using EventPro.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        private readonly int WAMessageLimit;

        readonly IMemoryCache memoryCache;
        readonly IWebHostEnvironment webHostEnvironment;
        private readonly IFirbaseAPI _FirbaseAPI;
        private readonly IWatiService _watiService;

        private readonly IAuditLogService _auditLogService;
        private readonly IWhatsappSendingProviderService _WhatsappSendingProvider;
        private readonly IWebHookBulkMessagingQueueConsumerService _WebHookQueueConsumerService;
        private readonly IMemoryCacheStoreService _MemoryCacheStoreService;
        private readonly IBlobStorage _blobStorage;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminController(IConfiguration configuration, IFirbaseAPI firbaseAPI,
                                 IWebHostEnvironment env,
                                 IWatiService watiService,
                                 IAuditLogService auditLogService,
                                 IWhatsappSendingProviderService whatsappSendingProvider,
                                 IWebHookBulkMessagingQueueConsumerService webHookQueueConsumerService,
                                 IMemoryCacheStoreService MemoryCacheStoreService,
                                 IBlobStorage blobStorage,
                                 IHttpContextAccessor httpContextAccessor)
        {
            SetBreadcrum("Dashboard", "/");
            _configuration = configuration;
            db = new EventProContext(configuration);
            WAMessageLimit = Convert.ToInt32(_configuration.GetSection("InterkatSettings").GetSection("messageLimit").Value);
            this.webHostEnvironment = env;
            _FirbaseAPI = firbaseAPI;
            _watiService = watiService;
            _auditLogService = auditLogService;
            _WhatsappSendingProvider = whatsappSendingProvider;
            _WebHookQueueConsumerService = webHookQueueConsumerService;
            _MemoryCacheStoreService = MemoryCacheStoreService;
            _blobStorage = blobStorage;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetBreadcrum(string title, string link)
        {
            ViewBag.PageTitle = title;
            ViewBag.BackLink = link;
        }

        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> Index()
        {
            SetBreadcrum("Dashboard", "/");
            var dashboardCounts = await db.VMDashboardCount.FirstOrDefaultAsync();
            ViewBag.CategoryCount = dashboardCounts.CategoryCount;
            ViewBag.EventsCount = dashboardCounts.EventsCount;
            ViewBag.UsersCount = dashboardCounts.UsersCount;
            ViewBag.GatekeeperCount = dashboardCounts.GatekeeperCount;
            ViewBag.Icon = "nav-icon fas fa-tachometer-alt";
            ViewBag.UpcomingEvents = await db.VwEvents.Where(p => p.EventFrom > DateTime.Now.Date & p.IsDeleted != true)
                .AsNoTracking()
                .OrderBy(p => p.EventFrom)
                .Take(5).ToListAsync();

            ViewBag.InProgressEvents = await db.VwEvents.Where(p => p.EventFrom <= DateTime.Now.Date && p.EventTo >= DateTime.Now.Date & p.IsDeleted != true)
                .AsNoTracking()
                .OrderBy(p => p.EventFrom)
                .Take(5).ToListAsync();

            ViewBag.PastEvents = await db.VwEvents.Where(p => p.EventFrom < DateTime.Now.Date && p.EventTo < DateTime.Now.Date & p.IsDeleted != true)
                .AsNoTracking()
                .OrderBy(p => p.EventFrom)
                .Take(5).ToListAsync();
            return View();
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> EventCategory()
        {
            ViewBag.Icon = "nav-icon fas fa-cubes";

            SetBreadcrum("Event Category", "/");
            ViewBag.EventCategory = await db.VwEventCategory.ToListAsync();
            return View();
        }

        [AuthorizeRoles("Administrator")]
        public IActionResult LocalizationDelete(int id)
        {
            ViewBag.Icon = "nav-icon fas fa-globe-asia";
            var l = db.LocallizationMaster.Where(p => p.Id == id).FirstOrDefault();
            db.LocallizationMaster.Remove(l);
            db.SaveChanges();
            return RedirectToAction("Localization", AppController.Admin);
        }

        [AuthorizeRoles("Administrator")]
        public IActionResult Localization()
        {
            ViewBag.Icon = "nav-icon fas fa-globe-asia";

            SetBreadcrum("Localization", "/");
            ViewBag.Localization = db.LocallizationMaster.ToList();
            return View();
        }

        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public IActionResult Localization(LocallizationMaster locallizationMaster)
        {
            ViewBag.Icon = "nav-icon fas fa-cubes";

            SetBreadcrum("Locallization", "/");
            locallizationMaster.RegionCode = "UAE";
            db.LocallizationMaster.Add(locallizationMaster);
            db.SaveChanges();
            ViewBag.Localization = db.LocallizationMaster.ToList();
            return View();
        }

        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> ScanLogs()
        {
            ViewBag.Gatekeeper = new SelectList(
                await db.Users
                    .Where(p => p.Role == 3)
                    .OrderBy(p => p.FirstName)
                    .Select(p => new
                    {
                        p.UserId,
                        FullName = p.FirstName + " " + p.LastName
                    })
                    .ToListAsync(),
                "UserId",
                "FullName",
                new { UserId = -1, FullName = "All" }
            );

            SetBreadcrum("Scan Logs", "/");
            ViewBag.Icon = "nav-icon fas fa-cubes";
            return View("ScanLogs - Copy");
        }

        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> GetScanLogs([FromQuery] scanLogesFilterParams scanLogesFilter)
        {
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            IQueryable<VwScanLogs> scanLogs = db.VwScanLogs.Where(e => (
            string.IsNullOrEmpty(searchValue) ? true
            : (e.EventId.ToString().Contains(searchValue.ToString().Trim())) ||
              (e.SystemEventTitle.Contains(searchValue.ToString().Trim())) ||
              (e.GuestName.Contains(searchValue.ToString().Trim())) ||
              (e.ScannedBy.Contains(searchValue.ToString().Trim())) ||
              (e.ScanBy.ToString().Contains(searchValue.ToString().Trim()))
            )).OrderByDescending(e => e.ScannedOn)
            .AsNoTracking();

            if (scanLogesFilter.scanCode == null)
            {
                scanLogs = scanLogs.Where(e => e.ScannedOn > DateTime.Today.AddDays(-30));
            }

            if (scanLogesFilter.EventId != null)
            {
                scanLogs = scanLogs.Where(e => e.EventId == scanLogesFilter.EventId);
            }

            if (scanLogesFilter.scanFrom != null)
            {
                scanLogs = scanLogs.Where(e => e.ScannedOn >= scanLogesFilter.scanFrom);
            }

            if (scanLogesFilter.scanTo != null)
            {
                scanLogs = scanLogs.Where(e => e.ScannedOn <= scanLogesFilter.scanTo);
            }

            if (scanLogesFilter.GateKeeperId != null && scanLogesFilter.GateKeeperId != -1)
            {
                scanLogs = scanLogs.Where(e => e.ScanBy == scanLogesFilter.GateKeeperId);
            }

            if (!string.IsNullOrEmpty(scanLogesFilter.scanCode) && (scanLogesFilter.scanCode != "All"))
            {
                scanLogs = scanLogs.Where(e => e.ResponseCode == scanLogesFilter.scanCode);
            }

            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                IEnumerable<EventOperator> evntOperators = new List<EventOperator>();
                if (HasOperatorRole())
                {
                    evntOperators = await GetOperatorAllEvents();
                }

                scanLogs = scanLogs
                    .Where(e => evntOperators.Select(p => p.EventId).Contains(e.EventId));
            }

            var recordsTotal = await scanLogs.CountAsync();
            pageSize = pageSize == -1 ? recordsTotal : pageSize;
            var result = await scanLogs.Skip(skip).Take(pageSize).ToListAsync();

            List<ScanLogVM> scanLogsVM = new();

            foreach (var scanLog in result)
            {
                ScanLogVM scanLogVM = new(scanLog);
                scanLogsVM.Add(scanLogVM);
            }


            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = scanLogsVM
            };

            return Ok(jsonData);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "Accounting", "Operator", "Supervisor")]
        public IActionResult ScanLogs(IFormCollection collection)
        {
            var logs = db.VwScanHistoryLogs.OrderByDescending(p => p.ScannedOn).ToList();

            string fromdate = collection["fromdate"];
            string todate = collection["todate"];
            string code = collection["code"];
            string gatekeeper = collection["gatekeeper"];
            string events = collection["event"];
            string guestName = collection["guestName"];
            string eventId = collection["EventId"];  // Event ID from textbox

            ViewBag.fromDate = fromdate;
            ViewBag.todate = todate;
            ViewBag.code = code;
            ViewBag.gatekeeper = gatekeeper;
            ViewBag.eventName = events;
            ViewBag.guestName = guestName;


            if (fromdate != null && fromdate != "")
            {
                DateTime _fromDate = Convert.ToDateTime(fromdate).Date;
                logs = logs.Where(p => p.ScannedOn >= _fromDate).ToList();
            }

            if (todate != null && todate != "")
            {
                DateTime _todate = Convert.ToDateTime(todate).Date;
                logs = logs.Where(p => p.ScannedOn <= _todate).ToList();
            }
            if (events != "All")
                logs = logs.Where(p => p.SystemEventTitle == events).ToList();

            if (code != "All")
                logs = logs.Where(p => p.ResponseCode == code).ToList();

            if (gatekeeper != "All")
                logs = logs.Where(p => p.ScanBy == Convert.ToInt32(gatekeeper)).ToList();

            if (events != "All")
                logs = logs.Where(p => p.SystemEventTitle == events).ToList();

            if (!string.IsNullOrEmpty(eventId))
            {
                int eventIdInt = Convert.ToInt32(eventId);
                var eventExists = db.Events.FirstOrDefault(e => e.Id == eventIdInt);
                if (eventExists != null)
                {
                    logs = logs.Where(p => p.SystemEventTitle == eventExists.SystemEventTitle).ToList();
                }
            }

            ViewBag.Icon = "nav-icon fas fa-cubes";
            ViewBag.Events = db.Events.OrderBy(p => p.SystemEventTitle).ToList();
            ViewBag.Gatekeeper = db.VwUsers.Where(p => p.RoleName == "Gatekeeper").OrderBy(p => p.FullName).ToList();

            SetBreadcrum("Scan Logs", "/");
            ViewBag.Logs = logs;
            return View();
        }

        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public async Task<IActionResult> EventCategory(EventCategory eventCategory)
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            SetBreadcrum("Event Category", "/");
            var files = Request.Form.Files;
            string path = _configuration.GetSection("Uploads").GetSection("Card").Value;
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string filename = string.Empty;
            bool hasFile = false;
            foreach (var file in files)
            {
                string extension = file.ContentType.ToLower().Replace(@"image/", "");
                filename = Guid.NewGuid() + "." + extension;
                using var stream = file.OpenReadStream();
                await _blobStorage.UploadAsync(stream, "xlsx", environment + path + "/" + filename, cancellationToken: default);
                hasFile = true;

            }

            if (eventCategory.EventId == 0)
            {
                var dataExists = db.EventCategory.Where(p => p.Category == eventCategory.Category).FirstOrDefault();

                ViewBag.Icon = "nav-icon fas fa-cubes";
                if (dataExists != null)
                {
                    TempData["message"] = "Event with same name already exists";
                    return View();
                }

                EventCategory cat = new EventCategory
                {
                    Category = eventCategory.Category,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    Status = true
                };
                if (hasFile)
                    cat.Icon = filename;
                db.EventCategory.Add(cat);
                db.SaveChanges();
            }
            else
            {
                var _event = db.EventCategory.Where(p => p.EventId == eventCategory.EventId).FirstOrDefault();
                _event.Category = eventCategory.Category;
                if (hasFile)
                    _event.Icon = filename;
                db.SaveChanges();
            }
            ViewBag.EventCategory = db.VwEventCategory.ToList();
            return View();
        }

    }
}
