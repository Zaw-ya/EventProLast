using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPro.DAL.Enum;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.DAL.Common;
using EventPro.Web.Extensions;
using EventPro.Web.Filters;
using EventPro.Web.Models;
using EventPro.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        #region WhatsApp Reports

        /// <summary>
        /// GET: Admin/WaTextReport
        /// Displays WhatsApp text confirmation report
        /// Shows aggregated message statistics from view
        /// </summary>
        /// <returns>View with WhatsApp text report data</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public IActionResult WaTextReport()
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            SetBreadcrum("Confirmaton Report", "/");
            ViewBag.CurrentUser = userId;

            // Load aggregated WhatsApp message statistics
            ViewBag.Data = db.VwMiagregationReport.ToList();
            return View();
        }

        /// <summary>
        /// GET: Admin/WaQRReport
        /// Displays WhatsApp QR code report
        /// Shows aggregated QR code scan statistics from view
        /// </summary>
        /// <returns>View with WhatsApp QR report data</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public IActionResult WaQRReport()
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            SetBreadcrum("QR Report", "/");
            ViewBag.CurrentUser = userId;

            // Load aggregated QR code statistics
            ViewBag.Data = db.VwMiagregationReport.ToList();
            return View();
        }

        /// <summary>
        /// GET: Admin/WaRespReport
        /// Displays WhatsApp confirmation response report page
        /// Shows detailed confirmation statistics for events
        /// </summary>
        /// <returns>View with WhatsApp confirmation report interface</returns>
        [AuthorizeRoles("Administrator", "Accounting", "Operator", "Supervisor")]
        public IActionResult WaRespReport()
        {
            SetBreadcrum("Whatsapp Confirmation Report", "/");
            return View("WaRespReport - Copy");
        }

        /// <summary>
        /// POST: Admin/getWaRespReport
        /// Retrieves paginated WhatsApp confirmation report data for DataTables
        /// Filters by operator's assigned events if user has operator role
        /// Excludes deleted events from results
        /// </summary>
        /// <returns>JSON data for DataTables with confirmation report statistics</returns>
        public async Task<IActionResult> getWaRespReport()
        {
            // Parse DataTables pagination parameters
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            // Parse search value for filtering
            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            // Query confirmation reports excluding deleted events
            IQueryable<vw_ConfirmationReport> whatsappConfiramtionReports = db.vw_ConfirmationReport
                .Where(e => e.IsDeleted == false)
                .Where(e => string.IsNullOrEmpty(searchValue) ? true
                : (searchValue.ToString().Contains(e.Id.ToString()))
                || (e.SystemEventTitle.Contains(searchValue))
                || (searchValue.ToString().Contains(e.SystemEventTitle)))
                .AsNoTracking()
                .OrderByDescending(e => e.Id);

            // Apply operator role filtering
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Get operator's assigned events
                IEnumerable<EventOperator> evntOperators = new List<EventOperator>();
                if (HasOperatorRole())
                {
                    evntOperators = await GetOperatorAllEvents();
                }

                // Filter reports to only operator's events
                whatsappConfiramtionReports = whatsappConfiramtionReports
                    .Where(e => evntOperators.Select(p => p.EventId).Contains(e.Id));
            }

            // Get paginated results
            var result = await whatsappConfiramtionReports.Skip(skip).Take(pageSize).ToListAsync();

            // Build view models
            List<WhatsappConfirmationReportVM> WhatsappConfirmationReportsVM = new();

            foreach (var report in result)
            {
                WhatsappConfirmationReportVM eventVM = new(report);
                WhatsappConfirmationReportsVM.Add(eventVM);
            }

            // Return DataTables JSON format
            var recordsTotal = await whatsappConfiramtionReports.CountAsync();
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = WhatsappConfirmationReportsVM
            };

            return Ok(jsonData);
        }

        #endregion

        #region Scan Reports

        /// <summary>
        /// GET: Admin/ScanSummary
        /// Displays scan summary report showing QR code scan statistics
        /// Executes stored procedure ProcScanSummary for data
        /// Filters by operator's assigned events if applicable
        /// </summary>
        /// <returns>View with scan summary data</returns>
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> ScanSummary()
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            SetBreadcrum("Scan Summary", "/");
            ViewBag.CurrentUser = userId;

            // Execute stored procedure to get scan summary statistics
            List<ScanSummary> scanSummary = await db.Set<ScanSummary>()
                           .FromSqlInterpolated($"exec ProcScanSummary")
                           .AsAsyncEnumerable().ToListAsync();

            // Apply operator role filtering
            if (HasOperatorRole())
            {
                // Get operator's assigned events
                IEnumerable<EventOperator> evntOperators = new List<EventOperator>();
                if (HasOperatorRole())
                {
                    evntOperators = await GetOperatorAllEvents();
                }

                // Filter summary to only operator's events
                scanSummary = scanSummary
                    .Where(e => evntOperators.Select(p => p.EventId).Contains(e.Id))
                    .ToList();
            }

            // Order results by event ID
            scanSummary = scanSummary.OrderBy(e => e.Id)
                .ToList();

            ViewBag.Data = scanSummary;

            return View();
        }

        #endregion

        #region Audit Log Reports

        /// <summary>
        /// GET: Admin/AuditLogReport
        /// Displays audit log report page with filter options
        /// Shows recent 500 audit log entries by default
        /// Provides dropdowns for filtering by action type and user
        /// </summary>
        /// <returns>View with audit log report data and filter options</returns>
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> AuditLogReport()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Audit Log", "/admin");

            // Populate action type dropdown with all enum values
            ViewBag.Action = new SelectList(Enum.GetValues(typeof(ActionEnum)).Cast<ActionEnum>()
                         .Select(x => new SelectListItem { Text = x.ToDescription(), Value = ((int)x).ToString() }).ToList()
            , "Value", "Text");

            // Populate user dropdown with active administrators and operators
            var users = await db.Users.Where(p => (p.Role == RoleIds.Administrator || p.Role == RoleIds.Operator) &&
                                       p.IsActive == true &&
                                       p.Approved == true)
                                       .OrderByDescending(e => e.UserId)
                                       .ToListAsync();
            ViewBag.Users = new SelectList(
                users.Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName }),
                "UserId",
                "FullName"
            );

            // Load recent 500 audit log entries
            ViewBag.AuditLogs = await db.AuditLog.Include(e => e.Event)
                .Include(e => e.User)
                .Where(e => e.Event != null && e.Event.IsDeleted == false)
                .OrderByDescending(x => x.Id).Take(500)
                 .Select(e => new AuditLogModel
                 {
                     Id = e.Id,
                     EventId = e.EventId,
                     EventTitle = e.Event.SystemEventTitle,
                     CreatedOn = e.CreatedOn,
                     EventFrom = e.Event.EventFrom,
                     EventTo = e.Event.EventTo,
                     linkedTo = e.Event.LinkedEvent,
                     UserName = e.User.UserName,
                     RelatedId = e.RelatedId,
                     Desc = e.Notes,
                     Action = ((ActionEnum)e.Action).ToDescription()
                 })
                 .ToListAsync();
            return View();
        }

        /// <summary>
        /// POST: Admin/AuditLogReport
        /// Filters audit log report based on submitted criteria
        /// Supports filtering by event title, event ID, action type, user, and date range
        /// Returns up to 10,000 filtered audit log entries
        /// </summary>
        /// <param name="collection">Form collection with filter parameters</param>
        /// <returns>View with filtered audit log data</returns>
        [HttpPost]
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> AuditLogReport(IFormCollection collection)
        {
            ViewBag.Icon = "nav-icon fas fa-calendar";
            SetBreadcrum("Audit Log", "/admin");

            // Extract filter parameters from form
            string eventTitle = collection["EventTitle"];
            string eventId = collection["EventId"];
            string actionId = collection["Action"];
            string userId = collection["User"];
            string dateFrom = collection["startFrom"];
            string dateTo = collection["startTo"];

            // Store filter values in ViewBag for form persistence
            ViewBag.eventTitle = eventTitle;
            ViewBag.eventId = eventId;
            ViewBag.actionId = actionId;
            ViewBag.userId = userId;
            ViewBag.startFrom = dateFrom;
            ViewBag.startTo = dateTo;

            // Repopulate dropdown lists
            var users = await db.Users.Where(p => (p.Role == RoleIds.Administrator || p.Role == RoleIds.Operator) &&
                             p.IsActive == true &&
                             p.Approved == true)
                             .OrderByDescending(e => e.UserId)
                             .ToListAsync();
            ViewBag.Users = new SelectList(
                users.Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName }),
                "UserId",
                "FullName"
            );

            ViewBag.Action = new SelectList(Enum.GetValues(typeof(ActionEnum)).Cast<ActionEnum>()
                           .Select(x => new SelectListItem { Text = x.ToDescription(), Value = ((int)x).ToString() }).ToList()
                           , "Value", "Text");

            // Query audit logs with filters applied
            ViewBag.AuditLogs = await db.AuditLog.Include(e => e.Event)
                .Include(e => e.User)
                .Where(x =>
                (string.IsNullOrEmpty(eventTitle) || (x.Event.SystemEventTitle.Contains(eventTitle)))
                && (string.IsNullOrEmpty(userId) || x.UserId == Int32.Parse(userId))
                && (string.IsNullOrEmpty(eventId) || (x.Event.Id == int.Parse(eventId)))
                && (string.IsNullOrEmpty(actionId) || x.Action == (ActionEnum)(int.Parse(actionId)))
                && (string.IsNullOrEmpty(dateFrom) || x.Event.EventFrom >= Convert.ToDateTime(dateFrom))
                && (string.IsNullOrEmpty(dateTo) || x.Event.EventFrom <= Convert.ToDateTime(dateTo)))
                .OrderByDescending(x => x.Id).Take(10000)
                 .Select(e => new AuditLogModel
                 {
                     Id = e.Id,
                     EventId = e.EventId,
                     EventTitle = e.Event.SystemEventTitle,
                     CreatedOn = e.CreatedOn,
                     EventFrom = e.Event.EventFrom,
                     EventTo = e.Event.EventTo,
                     linkedTo = e.Event.LinkedEvent,
                     UserName = e.User.UserName,
                     RelatedId = e.RelatedId,
                     Desc = e.Notes,
                     Action = ((ActionEnum)e.Action).ToDescription()
                 })
                 .ToListAsync();
            return View();
        }

        #endregion

        #region Guest Reports

        /// <summary>
        /// GET: Admin/TotalGuests
        /// Displays total guests report page with filter options
        /// Provides location dropdown for filtering
        /// </summary>
        /// <returns>View with total guests report interface</returns>
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        [HttpGet]
        public async Task<IActionResult> TotalGuests()
        {
            // Populate location dropdown with city/country combinations
            ViewBag.Address = new SelectList(await db.City.Include(c => c.Country)
               .Select(u => new BasicData { Id = u.Id, Value = u.Country.CountryName + "/" + u.CityName })
                .OrderBy(u => u.Value)
                 .ToListAsync(), "Id", "Value");
            return View();
        }

        /// <summary>
        /// POST: Admin/FilterTotalGuests
        /// Retrieves filtered and paginated total guests data for DataTables
        /// Supports filtering by event title, ID, city, type (active/archived), and date range
        /// Operators see only their assigned events
        /// Calculates total guests by summing NoOfMembers for each event
        /// </summary>
        /// <returns>JSON data for DataTables with total guest counts per event</returns>
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> FilterTotalGuests()
        {
            // Parse DataTables and filter parameters
            var filterDto = Request.Form.GetFilters();
            string eventTitle = Request.Form["EventTitle"];
            string eventId = Request.Form["EventId"];
            string cityId = Request.Form["Address"];
            string type = Request.Form["type"];
            string dateFrom = Request.Form["startFrom"];
            string dateTo = Request.Form["startTo"];

            // Base query with required relationships
            IQueryable<Events> events = db.Events.Where(e => e.IsDeleted == false).Include(e => e.Guest)
                 .Include(e => e.TypeNavigation).Include(e => e.City).ThenInclude(c => c.Country);

            // Apply operator role filtering
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                events = events.Include(e => e.EventOperators)
                    .Where(e => e.EventOperators.Any(p => p.OperatorId == userId));
            }

            // Apply DataTables search filter
            if (!string.IsNullOrEmpty(filterDto.SearchValue))
                events = events.Where(b => b.SystemEventTitle.Contains(filterDto.SearchValue!) ||
                                     b.Id.ToString().Contains(filterDto.SearchValue!)
                              );

            // Apply form filters
            events = events.Where(x =>
                (string.IsNullOrEmpty(eventTitle) || (x.SystemEventTitle.Contains(eventTitle)))
              && (string.IsNullOrEmpty(eventId) || (x.Id == int.Parse(eventId)))
                        && (string.IsNullOrEmpty(type) || (type == "active" && x.EventTo >= DateTime.Today)
                                                      || (type == "archived" && x.EventTo < DateTime.Today))
                        && (string.IsNullOrEmpty(cityId) || x.CityId == int.Parse(cityId))
                         && (string.IsNullOrEmpty(dateFrom) || x.EventFrom >= Convert.ToDateTime(dateFrom))
                        && (string.IsNullOrEmpty(dateTo) || x.EventFrom <= Convert.ToDateTime(dateTo))
                        );

            // Get total count before pagination
            var recordsTotal = events.Count();

            // Apply sorting and pagination
            events = events
                 .OrderBy($"{filterDto.SortColumn} {filterDto.SortColumnDirection}")
                 .Skip(filterDto.Skip)
                 .Take(filterDto.PageSize);

            // Build result with guest count aggregation
            var eventsList = await events.Select(e => new TotalGuestModel
            {
                Id = e.Id,
                EventTitle = e.SystemEventTitle,
                Icon = e.TypeNavigation.Icon,
                EventFrom = e.EventFrom,
                EventTo = e.EventTo,
                EventVenue = e.EventVenue,
                linkedTo = e.LinkedEvent,
                Location = !string.IsNullOrEmpty(e.City.Country.CountryName) ? e.City.Country.CountryName + "/" + e.City.CityName : e.City.Country.CountryName,
                TotalGuests = e.Guest.Sum(c => c.NoOfMembers) // Sum all members across guests

            }).ToListAsync();

            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal, data = eventsList };

            return Ok(jsonData);
        }

        #endregion

        #region Deleted Events Reports

        /// <summary>
        /// GET: Admin/ReportDeletedEventsByGk
        /// Displays deleted events by gatekeeper report page
        /// Shows events that were unassigned/deleted by gatekeepers
        /// Provides filter options for gatekeeper and location
        /// </summary>
        /// <returns>View with deleted events report interface</returns>
        [HttpGet]
        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> ReportDeletedEventsByGk()
        {
            ViewBag.Icon = "nav-icon fas fa-calendar-times";
            SetBreadcrum("Deleted Events By Gatekeeper", "/admin");

            // Populate gatekeeper dropdown with approved gatekeepers
            ViewBag.Users = new SelectList(await db.Users
                .Where(u => u.RoleNavigation.RoleName == "Gatekeeper" && u.Approved.Value)
                .Select(u => new BasicData
                {
                    Id = u.UserId,
                    Value = u.City.CityName + "/" + u.FirstName + " " + u.LastName
                }).ToListAsync(), "Id", "Value");

            // Populate location dropdown
            ViewBag.Address = new SelectList(await db.City.Include(c => c.Country)
                .Select(u => new BasicData
                {
                    Id = u.Id,
                    Value = u.Country.CountryName + "/" + u.CityName
                }).ToListAsync(), "Id", "Value");

            return View();
        }

        /// <summary>
        /// POST: Admin/FilterDeletedEventsByGk
        /// Retrieves filtered and paginated deleted events data for DataTables
        /// Supports filtering by gatekeeper ID, event ID, location, and date range
        /// Shows who deleted each event and current assigned gatekeepers
        /// Implements manual DataTables parameter extraction and sorting
        /// </summary>
        /// <returns>JSON data for DataTables with deleted event details</returns>
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> FilterDeletedEventsByGk()
        {
            // Extract DataTables parameters manually
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            // Parse pagination parameters with defaults
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            sortColumn = sortColumn ?? "Id";
            sortColumnDirection = sortColumnDirection ?? "desc";

            // Extract filter parameters
            string gkId = Request.Form["GKId"];
            string eventId = Request.Form["EventId"];
            string cityId = Request.Form["Address"];
            string dateFrom = Request.Form["startFrom"];
            string dateTo = Request.Form["startTo"];

            // Build query with event and deletion details
            var query = from r in db.ReportDeletedEventsByGk
                        join e in db.Events.Include(ev => ev.City).ThenInclude(c => c.Country)
                            on r.EventId equals e.Id
                        select new ReportDeletedEventsByGkViewModel
                        {
                            Id = r.Id,
                            EventId = e.Id,
                            SystemEventTitle = e.SystemEventTitle,
                            LinkedTo = e.LinkedEvent,
                            EventFrom = e.EventFrom,
                            EventTo = e.EventTo,
                            EventVenue = e.EventVenue,
                            Location = e.City.Country.CountryName + "/" + e.City.CityName,
                            DeletedOn = r.UnassignedOn.ToLocalTime(),
                            DeletedById = r.UnassignedById,
                            DeletedByName = r.UnassignedByName,
                            // Get current assigned gatekeepers for this event
                            AssignedNames = db.EventGatekeeperMapping
                                .Where(x => x.EventId == e.Id)
                                .Select(x => x.Gatekeeper.FirstName + " " + x.Gatekeeper.LastName)
                                .ToList()
                        };

            // Apply DataTables search
            if (!string.IsNullOrEmpty(searchValue))
                query = query.Where(q => q.SystemEventTitle.Contains(searchValue) ||
                                   q.EventId.ToString().Contains(searchValue) ||
                                   q.DeletedByName.Contains(searchValue));

            // Apply filters
            if (!string.IsNullOrEmpty(gkId))
                query = query.Where(q => q.DeletedById.ToString() == gkId);

            if (!string.IsNullOrEmpty(eventId))
                query = query.Where(q => q.EventId.ToString().Contains(eventId));

            if (!string.IsNullOrEmpty(cityId))
            {
                // Lookup city name for filtering
                var cityName = await db.City.Include(c => c.Country)
                    .Where(c => c.Id.ToString() == cityId)
                    .Select(c => c.Country.CountryName + "/" + c.CityName)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(cityName))
                    query = query.Where(q => q.Location == cityName);
            }

            if (DateTime.TryParse(dateFrom, out var fromDate))
                query = query.Where(q => q.EventFrom >= fromDate);

            if (DateTime.TryParse(dateTo, out var toDate))
                query = query.Where(q => q.EventFrom <= toDate);

            var recordsTotal = query.Count();

            // Apply sorting manually based on column name
            switch (sortColumn.ToLower())
            {
                case "id":
                    query = sortColumnDirection == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id);
                    break;
                case "eventid":
                    query = sortColumnDirection == "asc" ? query.OrderBy(x => x.EventId) : query.OrderByDescending(x => x.EventId);
                    break;
                case "systemeventtitle":
                    query = sortColumnDirection == "asc" ? query.OrderBy(x => x.SystemEventTitle) : query.OrderByDescending(x => x.SystemEventTitle);
                    break;
                case "eventfrom":
                    query = sortColumnDirection == "asc" ? query.OrderBy(x => x.EventFrom) : query.OrderByDescending(x => x.EventFrom);
                    break;
                case "deletedon":
                    query = sortColumnDirection == "asc" ? query.OrderBy(x => x.DeletedOn) : query.OrderByDescending(x => x.DeletedOn);
                    break;
                case "deletedbyname":
                    query = sortColumnDirection == "asc" ? query.OrderBy(x => x.DeletedByName) : query.OrderByDescending(x => x.DeletedByName);
                    break;
                case "location":
                    query = sortColumnDirection == "asc" ? query.OrderBy(x => x.Location) : query.OrderByDescending(x => x.Location);
                    break;
                default:
                    query = query.OrderByDescending(x => x.Id);
                    break;
            }

            // Apply paging
            query = query.Skip(skip).Take(pageSize);

            var data = await query.ToListAsync();

            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal, data = data };
            return Ok(jsonData);
        }

        #endregion
    }
}
