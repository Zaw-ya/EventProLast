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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        #region Properties and Dependencies

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

        /// <summary>
        /// Constructor - Initializes AdminController with all required dependencies
        /// Sets up database context, services, and configuration values
        /// Sets default breadcrumb navigation to Dashboard
        /// </summary>
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

        #endregion

        #region Dashboard

        /// <summary>

        /// <summary>
        /// GET: Admin/Index
        /// Displays the main admin dashboard with real-time statistics and data visualization.
        /// </summary>
        /// <returns>Dashboard view with statistics, charts data, and event lists</returns>
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> Index()
        {
            SetBreadcrum("Dashboard", "/");

            // Initialize default values
            ViewBag.CategoryCount = 0;
            ViewBag.EventsCount = 0;
            ViewBag.UsersCount = 0;
            ViewBag.GatekeeperCount = 0;
            ViewBag.ClientCount = 0;
            ViewBag.Icon = "nav-icon fas fa-tachometer-alt";
            
            // Charts Data
            ViewBag.ChartLabels = new List<string>();
            ViewBag.ChartData = new List<int>();

            ViewBag.UpcomingEvents = new List<VwEvents>();
            ViewBag.InProgressEvents = new List<VwEvents>();
            ViewBag.PastEvents = new List<VwEvents>();

            try
            {
                // 1. Real-time Counts
                ViewBag.CategoryCount = await db.EventCategory.CountAsync();
                ViewBag.EventsCount = await db.Events.CountAsync(e => e.IsDeleted != true);
                ViewBag.UsersCount = await db.Users.CountAsync(u => u.Approved == true); // Assuming 'Approved' means active user
                ViewBag.GatekeeperCount = await db.Users.CountAsync(u => u.Role == RoleIds.GateKeeper && u.Approved == true);
                ViewBag.ClientCount = await db.Users.CountAsync(u => u.Role == RoleIds.Client && u.Approved == true);

                // 2. Chart Data: Events per Month (Current Year)
                var currentYear = DateTime.Now.Year;
                var monthlyStats = await db.Events
                    .Where(e => e.EventFrom.HasValue && e.EventFrom.Value.Year == currentYear && e.IsDeleted != true)
                    .GroupBy(e => e.EventFrom.Value.Month)
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .ToListAsync();
                
                var months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).ToList();
                var dataPoints = new int[12];
                foreach(var stat in monthlyStats)
                {
                    if(stat.Month >= 1 && stat.Month <= 12)
                        dataPoints[stat.Month - 1] = stat.Count;
                }

                ViewBag.ChartLabels = months;
                ViewBag.ChartData = dataPoints.ToList();


                // 3. Activity Feed Lists
                // Load upcoming events (future events only, ordered by date, top 5)
                ViewBag.UpcomingEvents = await _unitOfWork.Events.GetQueryable()
                    .Include(e => e.CardInfo)
                    .Include(e => e.CreatedByNavigation)
                    .Include(e => e.TypeNavigation)
                    .Where(p => p.EventFrom > DateTime.Now.Date && p.IsDeleted != true)
                    .OrderBy(p => p.EventFrom)
                    .Take(5)
                    .ToListAsync();
                    
               // Load in-progress events (happening today, ordered by date, top 5)
                ViewBag.InProgressEvents = await _unitOfWork.Events.GetQueryable()
                    .Include(e => e.CardInfo)
                    .Include(e => e.CreatedByNavigation)
                    .Include(e => e.TypeNavigation)
                    .Where(p => p.EventFrom <= DateTime.Now.Date && p.EventTo >= DateTime.Now.Date && p.IsDeleted != true)
                    .OrderBy(p => p.EventFrom)
                    .Take(5)
                    .ToListAsync();

                // Load past events (already completed, ordered by date, top 5)
                ViewBag.PastEvents = await _unitOfWork.Events.GetQueryable()
                    .Include(e => e.CardInfo)
                    .Include(e => e.CreatedByNavigation)
                    .Include(e => e.TypeNavigation)
                    .Where(p => p.EventFrom < DateTime.Now.Date && p.EventTo < DateTime.Now.Date && p.IsDeleted != true)
                    .OrderByDescending(p => p.EventFrom)
                    .Take(5)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log error silently - dashboard will display with default empty values
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }

            return View();
        }

        #endregion

        #region Event Category Management

        /// <summary>
        /// GET: Admin/EventCategory
        /// Displays list of all event categories in the system
        /// Shows category names, icons, and active/inactive status
        /// Loads categories using Unit of Work pattern
        /// </summary>
        /// <returns>View with list of event categories</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> EventCategory()
        {
            ViewBag.Icon = "nav-icon fas fa-cubes";
            SetBreadcrum("Event Category", "/");

            try
            {
                // Load all categories from database via unit of work
                var allCategories = await _unitOfWork.EventCategory.GetAll();
                ViewBag.EventCategory = allCategories;
            }
            catch (Exception ex)
            {
                // Handle error and show empty list with error message
                Console.WriteLine($"Error loading categories: {ex.Message}");
                ViewBag.EventCategory = new List<VwEventCategory>();
                TempData["message"] = "Error loading categories. Please try again.";
            }

            return View();
        }

        /// <summary>
        /// POST: Admin/EventCategory
        /// Creates a new event category or updates an existing one
        /// Handles category icon upload to Cloudinary storage
        /// Validates for duplicate category names before creation
        /// </summary>
        /// <param name="eventCategory">Event category model with Id, name, and other properties</param>
        /// <returns>Redirects to category list with success/error message</returns>
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

                // Handle category icon upload to Cloudinary
                if (files != null && files.Count > 0)
                {
                    var file = files[0];
                    if (file != null && file.Length > 0)
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            // Upload to Cloudinary in 'categories' folder
                            filename = await _cloudinaryService.UploadImageAsync(stream, file.FileName, "categories");
                        }
                    }
                }

                if (eventCategory.EventId == 0)
                {
                    // CREATE NEW CATEGORY

                    // Check if category with same name already exists
                    var exists = await _unitOfWork.EventCategory.Find(p => p.Category == eventCategory.Category);
                    if (exists != null)
                    {
                        TempData["message"] = "Event Category with same name already exists";
                        ViewBag.EventCategory = await _unitOfWork.EventCategory.GetAll();
                        return View();
                    }

                    // Create new category entity
                    EventCategory cat = new EventCategory
                    {
                        Category = eventCategory.Category,
                        CreatedBy = userId,
                        CreatedOn = DateTime.Now,
                        Status = true, // Active by default
                        Icon = string.IsNullOrEmpty(filename) ? null : filename
                    };

                    await _unitOfWork.EventCategory.Add(cat);
                    await _unitOfWork.Complete();
                    TempData["successMessage"] = "Event Category created successfully";
                }
                else
                {
                    // UPDATE EXISTING CATEGORY

                    var cat = await _unitOfWork.EventCategory.GetById(eventCategory.EventId);
                    if (cat != null)
                    {
                        cat.Category = eventCategory.Category;
                        // Only update icon if new one was uploaded
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

        /// <summary>
        /// POST: Admin/ToggleEventCategoryStatus
        /// Toggles the active/inactive status of an event category
        /// Changes status from true to false or false to true
        /// </summary>
        /// <param name="eventId">ID of the category to toggle</param>
        /// <returns>Redirects to category list with success message</returns>
        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public async Task<IActionResult> ToggleEventCategoryStatus(int eventId)
        {
            try
            {
                var category = await _unitOfWork.EventCategory.GetById(eventId);

                if (category != null)
                {
                    // Toggle status between active (true) and inactive (false)
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

        #endregion

        #region Localization Management

        /// <summary>
        /// GET: Admin/Localization
        /// Displays all localization entries for multi-language support
        /// Shows translation keys and their corresponding values for different regions
        /// </summary>
        /// <returns>View with list of localization entries</returns>
        [AuthorizeRoles("Administrator")]
        public IActionResult Localization()
        {
            ViewBag.Icon = "nav-icon fas fa-globe-asia";
            SetBreadcrum("Localization", "/");

            ViewBag.Localization = db.LocallizationMaster.ToList();

            return View();
        }

        /// <summary>
        /// POST: Admin/Localization
        /// Creates a new localization entry for translation support
        /// Automatically sets region code to UAE
        /// </summary>
        /// <param name="locallizationMaster">Localization model with key, value, and region</param>
        /// <returns>View with updated localization list</returns>
        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public IActionResult Localization(LocallizationMaster locallizationMaster)
        {
            ViewBag.Icon = "nav-icon fas fa-globe-asia";
            SetBreadcrum("Localization", "/");

            // Set default region code
            locallizationMaster.RegionCode = "UAE";
            db.LocallizationMaster.Add(locallizationMaster);
            db.SaveChanges();

            // Reload list with new entry
            ViewBag.Localization = db.LocallizationMaster.ToList();

            return View();
        }

        /// <summary>
        /// GET: Admin/LocalizationDelete
        /// Deletes a localization entry by ID
        /// Removes translation key-value pair from the system
        /// </summary>
        /// <param name="id">ID of localization entry to delete</param>
        /// <returns>Redirects to localization list</returns>
        [AuthorizeRoles("Administrator")]
        public IActionResult LocalizationDelete(int id)
        {
            ViewBag.Icon = "nav-icon fas fa-globe-asia";

            var localization = db.LocallizationMaster.Where(p => p.Id == id).FirstOrDefault();
            if (localization != null)
            {
                db.LocallizationMaster.Remove(localization);
                db.SaveChanges();
            }

            return RedirectToAction("Localization", AppController.Admin);
        }

        #endregion

        #region Scan Logs Management

        /// <summary>
        /// GET: Admin/ScanLogs
        /// Displays the scan logs page with filter options
        /// Shows QR code scanning history for all events
        /// Populates gatekeeper dropdown for filtering by scanner
        /// </summary>
        /// <returns>Scan logs view with gatekeeper filter dropdown</returns>
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> ScanLogs()
        {
            // Populate gatekeeper dropdown for filtering scan logs
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

        /// <summary>
        /// POST: Admin/GetScanLogs
        /// Retrieves filtered and paginated scan logs for DataTables display
        /// Supports filtering by: event, date range, gatekeeper, scan code (response status)
        /// Supports full-text search across event ID, title, guest name, and scanner
        /// Restricts operators to view only their assigned events
        /// Returns JSON data for DataTables rendering
        /// </summary>
        /// <param name="scanLogesFilter">Filter parameters from query string</param>
        /// <returns>JSON with paginated scan log data for DataTables</returns>
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> GetScanLogs([FromQuery] scanLogesFilterParams scanLogesFilter)
        {
            // Get pagination parameters from DataTables
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            // Get search value from DataTables
            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            // Build base query with search filter across multiple fields
            IQueryable<VwScanLogs> scanLogs = db.VwScanLogs.Where(e => (
            string.IsNullOrEmpty(searchValue) ? true
            : (e.EventId.ToString().Contains(searchValue.ToString().Trim())) ||
              (e.SystemEventTitle.Contains(searchValue.ToString().Trim())) ||
              (e.GuestName.Contains(searchValue.ToString().Trim())) ||
              (e.ScannedBy.Contains(searchValue.ToString().Trim())) ||
              (e.ScanBy.ToString().Contains(searchValue.ToString().Trim()))
            )).OrderByDescending(e => e.ScannedOn)
            .AsNoTracking();

            // Default filter: show only last 30 days if no scan code filter applied
            if (scanLogesFilter.scanCode == null)
            {
                scanLogs = scanLogs.Where(e => e.ScannedOn > DateTime.Today.AddDays(-30));
            }

            // Apply event ID filter
            if (scanLogesFilter.EventId != null)
            {
                scanLogs = scanLogs.Where(e => e.EventId == scanLogesFilter.EventId);
            }

            // Apply date range filters
            if (scanLogesFilter.scanFrom != null)
            {
                scanLogs = scanLogs.Where(e => e.ScannedOn >= scanLogesFilter.scanFrom);
            }

            if (scanLogesFilter.scanTo != null)
            {
                scanLogs = scanLogs.Where(e => e.ScannedOn <= scanLogesFilter.scanTo);
            }

            // Apply gatekeeper filter (who scanned the QR code)
            if (scanLogesFilter.GateKeeperId != null && scanLogesFilter.GateKeeperId != -1)
            {
                scanLogs = scanLogs.Where(e => e.ScanBy == scanLogesFilter.GateKeeperId);
            }

            // Apply scan response code filter (success, duplicate, invalid, etc.)
            if (!string.IsNullOrEmpty(scanLogesFilter.scanCode) && (scanLogesFilter.scanCode != "All"))
            {
                scanLogs = scanLogs.Where(e => e.ResponseCode == scanLogesFilter.scanCode);
            }

            // Restrict operators to see only their assigned events
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

            // Calculate total count before pagination
            var recordsTotal = await scanLogs.CountAsync();

            // Apply pagination (-1 means show all)
            pageSize = pageSize == -1 ? recordsTotal : pageSize;
            var result = await scanLogs.Skip(skip).Take(pageSize).ToListAsync();

            // Convert to view model for client
            List<ScanLogVM> scanLogsVM = new();
            foreach (var scanLog in result)
            {
                ScanLogVM scanLogVM = new(scanLog);
                scanLogsVM.Add(scanLogVM);
            }

            // Return JSON formatted for DataTables
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = scanLogsVM
            };

            return Ok(jsonData);
        }

        /// <summary>
        /// POST: Admin/ScanLogs
        /// Legacy method for filtering scan logs via form post (older UI)
        /// Supports filtering by: date range, event name, gatekeeper, scan code, guest name, event ID
        /// Maintains filter values in ViewBag to preserve form state after post
        /// </summary>
        /// <param name="collection">Form collection with filter parameters</param>
        /// <returns>View with filtered scan log list</returns>
        [HttpPost]
        [AuthorizeRoles("Administrator", "Accounting", "Operator", "Supervisor")]
        public IActionResult ScanLogs(IFormCollection collection)
        {
            // Load all scan logs ordered by most recent first
            var logs = db.VwScanHistoryLogs.OrderByDescending(p => p.ScannedOn).ToList();

            // Extract filter parameters from form collection
            string fromdate = collection["fromdate"];
            string todate = collection["todate"];
            string code = collection["code"];
            string gatekeeper = collection["gatekeeper"];
            string events = collection["event"];
            string guestName = collection["guestName"];
            string eventId = collection["EventId"];

            // Store filter values in ViewBag to maintain form state
            ViewBag.fromDate = fromdate;
            ViewBag.todate = todate;
            ViewBag.code = code;
            ViewBag.gatekeeper = gatekeeper;
            ViewBag.eventName = events;
            ViewBag.guestName = guestName;

            // Apply from date filter
            if (fromdate != null && fromdate != "")
            {
                DateTime _fromDate = Convert.ToDateTime(fromdate).Date;
                logs = logs.Where(p => p.ScannedOn >= _fromDate).ToList();
            }

            // Apply to date filter
            if (todate != null && todate != "")
            {
                DateTime _todate = Convert.ToDateTime(todate).Date;
                logs = logs.Where(p => p.ScannedOn <= _todate).ToList();
            }

            // Apply event name filter
            if (events != "All")
                logs = logs.Where(p => p.SystemEventTitle == events).ToList();

            // Apply scan response code filter
            if (code != "All")
                logs = logs.Where(p => p.ResponseCode == code).ToList();

            // Apply gatekeeper filter
            if (gatekeeper != "All")
                logs = logs.Where(p => p.ScanBy == Convert.ToInt32(gatekeeper)).ToList();

            // Apply event ID filter from textbox input
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

            // Populate dropdowns for filters
            ViewBag.Events = db.Events.OrderBy(p => p.SystemEventTitle).ToList();
            ViewBag.Gatekeeper = db.VwUsers.Where(p => p.RoleName == "Gatekeeper").OrderBy(p => p.FullName).ToList();

            SetBreadcrum("Scan Logs", "/");
            ViewBag.Logs = logs;

            return View();
        }

        #endregion

        

        #region Helper Methods

        /// <summary>
        /// Sets breadcrumb navigation data in ViewBag for consistent page navigation
        /// Used across all admin pages to display page title and back link
        /// </summary>
        /// <param name="title">Page title to display in breadcrumb and header</param>
        /// <param name="link">URL for back navigation link</param>
        private void SetBreadcrum(string title, string link)
        {
            ViewBag.PageTitle = title;
            ViewBag.BackLink = link;
        }

        #endregion
    }
}
