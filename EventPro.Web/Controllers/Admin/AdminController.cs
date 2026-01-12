using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.RabbitMQMessaging.Interface;
using EventPro.Business.Storage.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Common;
using EventPro.DAL.Common.Interfaces;
using EventPro.DAL.Extensions;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.Services.AuditLogService.Interface;
using EventPro.Services.UnitOFWorkService;
using EventPro.Services.WatiService.Interface;
using EventPro.Web.Common;
using EventPro.Web.Filters;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        private readonly IUnitOfWork _unitOfWork;
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
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminController(IConfiguration configuration, IFirbaseAPI firbaseAPI,
                                 IWebHostEnvironment env,
                                 IWatiService watiService,
                                 IAuditLogService auditLogService,
                                 IWhatsappSendingProviderService whatsappSendingProvider,
                                 IWebHookBulkMessagingQueueConsumerService webHookQueueConsumerService,
                                 IMemoryCacheStoreService MemoryCacheStoreService,
                                 IBlobStorage blobStorage,
                                 IHttpContextAccessor httpContextAccessor,
                                 IUnitOfWork unitOfWork,
                                 ICloudinaryService cloudinaryService)
        {
            SetBreadcrum("Dashboard", "/");
            _configuration = configuration;
            db = new EventProContext(configuration);
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
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

            // Initialize default values
            ViewBag.CategoryCount = 0;
            ViewBag.EventsCount = 0;
            ViewBag.UsersCount = 0;
            ViewBag.GatekeeperCount = 0;
            ViewBag.Icon = "nav-icon fas fa-tachometer-alt";
            ViewBag.UpcomingEvents = new List<VwEvents>(); // Use actual type
            ViewBag.InProgressEvents = new List<VwEvents>(); // Use actual type
            ViewBag.PastEvents = new List<VwEvents>(); // Use actual type

            try
            {
                // Get dashboard counts with null check
                var dashboardCounts = await db.VMDashboardCount.FirstOrDefaultAsync();
                if (dashboardCounts != null)
                {
                    ViewBag.CategoryCount = dashboardCounts.CategoryCount;
                    ViewBag.EventsCount = dashboardCounts.EventsCount;
                    ViewBag.UsersCount = dashboardCounts.UsersCount;
                    ViewBag.GatekeeperCount = dashboardCounts.GatekeeperCount;
                }

                // Get upcoming events with null check
                var upcomingEvents = await db.VwEvents
                    .Where(p => p.EventFrom > DateTime.Now.Date && p.IsDeleted != true)
                    .AsNoTracking()
                    .OrderBy(p => p.EventFrom)
                    .Take(5)
                    .ToListAsync();
                ViewBag.UpcomingEvents = upcomingEvents ?? new List<VwEvents>();

                // Get in-progress events with null check
                var inProgressEvents = await db.VwEvents
                    .Where(p => p.EventFrom <= DateTime.Now.Date && p.EventTo >= DateTime.Now.Date && p.IsDeleted != true)
                    .AsNoTracking()
                    .OrderBy(p => p.EventFrom)
                    .Take(5)
                    .ToListAsync();
                ViewBag.InProgressEvents = inProgressEvents ?? new List<VwEvents>();

                // Get past events with null check
                var pastEvents = await db.VwEvents
                    .Where(p => p.EventFrom < DateTime.Now.Date && p.EventTo < DateTime.Now.Date && p.IsDeleted != true)
                    .AsNoTracking()
                    .OrderBy(p => p.EventFrom)
                    .Take(5)
                    .ToListAsync();
                ViewBag.PastEvents = pastEvents ?? new List<VwEvents>();
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                // _logger.LogError(ex, "Error loading dashboard data");
            }

            return View();
        }


        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> EventCategory()
        {
            ViewBag.Icon = "nav-icon fas fa-cubes";
            SetBreadcrum("Event Category", "/");

            try
            {
                var allCategories = await _unitOfWork.EventCategory.GetAll();
                ViewBag.EventCategory = allCategories;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading categories: {ex.Message}");
                ViewBag.EventCategory = new List<VwEventCategory>();
                TempData["message"] = "Error loading categories. Please try again.";
            }

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
                    .Where(p => p.Role == RoleIds.GateKeeper)
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
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                SetBreadcrum("Event Category", "/");

                var files = Request.Form.Files;
                string filename = string.Empty;

                // Upload image to Cloudinary
                if (files != null && files.Count > 0)
                {
                    var file = files[0];
                    if (file != null && file.Length > 0)
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            filename = await _cloudinaryService.UploadImageAsync(stream, file.FileName, "categories");
                        }
                    }
                }

                if (eventCategory.EventId == 0)
                {
                    // Check duplicate category name
                    var exists = await _unitOfWork.EventCategory.Find(p => p.Category == eventCategory.Category);
                    if (exists != null)
                    {
                        TempData["message"] = "Event Category with same name already exists";
                        ViewBag.EventCategory = await _unitOfWork.EventCategory.GetAll();
                        return View();
                    }

                    // Create new category
                    EventCategory cat = new EventCategory
                    {
                        Category = eventCategory.Category,
                        CreatedBy = userId,
                        CreatedOn = DateTime.Now,
                        Status = true,
                        Icon = string.IsNullOrEmpty(filename) ? null : filename
                    };

                    await _unitOfWork.EventCategory.Add(cat);
                    await _unitOfWork.Complete();
                    TempData["successMessage"] = "Event Category created successfully";
                }
                else
                {
                    // Update category
                    var cat = await _unitOfWork.EventCategory.GetById(eventCategory.EventId);
                    if (cat != null)
                    {
                        cat.Category = eventCategory.Category;
                        if (!string.IsNullOrEmpty(filename))
                            cat.Icon = filename;
                        _unitOfWork.EventCategory.Update(cat);
                        await _unitOfWork.Complete();
                        TempData["successMessage"] = "Event Category updated successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = $"Error: {ex.Message}";
            }

            ViewBag.EventCategory = await _unitOfWork.EventCategory.GetAll();
            return View(nameof(EventCategory));
        }

        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public async Task<IActionResult> ToggleEventCategoryStatus(int eventId)
        {
            try
            {
                var category = await _unitOfWork.EventCategory.GetById(eventId);

                if (category != null)
                {
                    // Toggle the status
                    category.Status = !category.Status;
                    await _unitOfWork.Complete();

                    TempData["successMessage"] = $"Category status changed to {(category.Status == true ? "Active" : "Inactive")}";
                }
                else
                {
                    TempData["errorMessage"] = "Category not found";
                }
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = $"Error toggling status: {ex.Message}";
            }

            return RedirectToAction(nameof(EventCategory));
        }

    }
}
