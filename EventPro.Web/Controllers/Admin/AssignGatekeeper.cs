using EventPro.DAL.Common;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        #region Gatekeeper Assignment Management

        /// <summary>
        /// GET: Admin/AssignGatekeeper
        /// Displays the gatekeeper assignment page for a specific event
        /// Shows available and assigned gatekeepers for the event
        /// </summary>
        /// <param name="id">Event ID to manage gatekeepers for</param>
        /// <returns>View with event details and gatekeeper assignment interface</returns>
        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> AssignGatekeeper(int id)
        {
            // Set event ID for view reference
            ViewBag.EventId = id;

            // Load event details from view
            var model = await db.VwEvents.Where(e => e.Id == id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();


            ViewBag.Icon = await db.CardInfo.Where(p => p.EventId == id).Select(p => p.BackgroundImage)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Set event location for display
            ViewBag.EventLocation = model.Glocation;

            SetBreadcrum("Manage Gatekeeper", "/");
            return View("AssignGatekeeper - Copy", model);
        }

        /// <summary>
        /// POST: Admin/GetAvaliableGateKeepers
        /// Retrieves paginated list of available gatekeepers for DataTables display
        /// Excludes gatekeepers already assigned to the event
        /// Shows gatekeepers with active and approved status
        /// Displays other events each gatekeeper is scheduled for (conflict detection)
        /// </summary>
        /// <param name="eventId">Event ID to check gatekeeper availability for</param>
        /// <returns>JSON data for DataTables with available gatekeepers and their scheduled events</returns>
        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> GetAvaliableGateKeepers(int eventId)
        {
            // Parse DataTables pagination parameters
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            // Parse search value for filtering
            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            // Get event information for conflict checking
            var eventInfo = await db.Events.Where(p => p.Id == eventId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Query all active and approved gatekeepers
            IQueryable<Users> allGateKeepers = db.Users.Where(e => e.Role == RoleIds.GateKeeper && e.IsActive.Value && e.Approved.Value)
                .AsNoTracking();

            // Apply search filter on gatekeeper name
            if (!string.IsNullOrEmpty(searchValue))
            {
                allGateKeepers = allGateKeepers.Where(e => string.Concat(e.FirstName, " ", e.LastName).Contains(searchValue));
            }

            // Get gatekeepers already assigned to this event
            IQueryable<VwEventGatekeeper> assignedGateKeepers = db.VwEventGatekeeper
                .Where(p => p.TaskId != null && p.EventId == eventId)
                .AsNoTracking();

            // Exclude already assigned gatekeepers from available list
            allGateKeepers = allGateKeepers.Where(e => assignedGateKeepers.All(c => c.UserId != e.UserId));
            int avalaibleGateKeepersCount = await allGateKeepers.CountAsync();
            var allGateKeepersResult = await allGateKeepers.Skip(skip).Take(pageSize).ToListAsync();

            // Get gatekeepers scheduled for other events during the same timeframe (conflict detection)
            IQueryable<VwGateKeeperScheduled> gateKeeperScheduled = db.VwGateKeeperScheduled
                    .Where(p => p.EventFrom >= eventInfo.EventFrom && p.EventId != eventId)
                    .AsNoTracking();

            var gateKeeperScheduledResult = await gateKeeperScheduled.ToListAsync();

            // Build view models with gatekeeper details and their other assigned events
            List<AvaliableGateKeepersVM> avaliableGateKeepersVM = new();

            foreach (var gateKeeper in allGateKeepersResult)
            {
                // Get list of other events this gatekeeper is assigned to
                var AssignedEvents = gateKeeperScheduledResult.Where(e => e.GatekeeperId == gateKeeper.UserId)
                    .Select(e => e.EventTitle)
                    .ToList();
                AvaliableGateKeepersVM assignGateKeeperVM = new(gateKeeper, AssignedEvents);
                avaliableGateKeepersVM.Add(assignGateKeeperVM);
            }

            // Return DataTables JSON format
            var recordsTotal = avalaibleGateKeepersCount;
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = avaliableGateKeepersVM
            };

            return Ok(jsonData);
        }

        /// <summary>
        /// POST: Admin/GetAssignedGateKeepers
        /// Retrieves paginated list of gatekeepers already assigned to an event
        /// Used for displaying current gatekeeper assignments in DataTables
        /// </summary>
        /// <param name="eventId">Event ID to get assigned gatekeepers for</param>
        /// <returns>JSON data for DataTables with assigned gatekeeper details</returns>
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> GetAssignedGateKeepers(int eventId)
        {
            // Parse DataTables pagination parameters
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            // Parse search value (currently not used in filtering)
            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            // Query gatekeepers assigned to this event (TaskId not null means assigned)
            IQueryable<VwEventGatekeeper> assignedGateKeepers = db.VwEventGatekeeper.Where(p => p.TaskId != null && p.EventId == eventId)
                                 .AsNoTracking();

            // Get total count and paginated results
            int assignedGateKeepersCount = await assignedGateKeepers.CountAsync();
            var assignedGateKeepersResult = await assignedGateKeepers.Skip(skip).Take(pageSize).ToListAsync();

            // Build view models for assigned gatekeepers
            List<AssignedGateKeepersVM> assignedGateKeepersVM = new();

            foreach (var gateKeeper in assignedGateKeepersResult)
            {
                AssignedGateKeepersVM assignGateKeeperVM = new(gateKeeper);
                assignedGateKeepersVM.Add(assignGateKeeperVM);
            }

            // Return DataTables JSON format
            var recordsTotal = assignedGateKeepersCount;
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = assignedGateKeepersVM
            };

            return Ok(jsonData);
        }

        /// <summary>
        /// POST: Admin/Unassigned
        /// Removes a gatekeeper assignment from an event
        /// Deletes the EventGatekeeperMapping record for the specified task
        /// </summary>
        /// <param name="taskId">Task ID of the gatekeeper assignment to remove</param>
        /// <returns>JSON with success status</returns>
        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> Unassigned(int taskId)
        {
            // Find the gatekeeper mapping by task ID
            EventGatekeeperMapping egm = await db.EventGatekeeperMapping.Where(p => p.TaskId == taskId).FirstOrDefaultAsync();

            // Return failure if mapping not found
            if (egm == null) return Json(new { success = false });

            // Get event ID before removing (for potential logging/auditing)
            int eventId = Convert.ToInt32(egm.EventId);

            // Remove the gatekeeper assignment
            db.EventGatekeeperMapping.Remove(egm);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        /// <summary>
        /// POST: Admin/Assigned
        /// Assigns a gatekeeper to an event
        /// Creates a new EventGatekeeperMapping record
        /// Prevents duplicate assignments (same gatekeeper + event)
        /// </summary>
        /// <param name="eventId">Event ID to assign gatekeeper to</param>
        /// <param name="userid">User ID of the gatekeeper to assign</param>
        /// <returns>JSON with success status</returns>
        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> Assigned(int eventId, int userid)
        {
            // Get current user ID for tracking who made the assignment
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Check if this gatekeeper is already assigned to this event
            if (await db.EventGatekeeperMapping.Where(e => e.EventId == eventId && e.GatekeeperId == userid).FirstOrDefaultAsync() != null)
            {
                return Json(new { success = false });
            }

            // Create new gatekeeper assignment mapping
            EventGatekeeperMapping egm = new EventGatekeeperMapping
            {
                EventId = eventId,
                GatekeeperId = userid,
                AssignedBy = userId, // Track who assigned this gatekeeper
                AsssignedOn = DateTime.UtcNow, // Track when assignment was made
                IsActive = true
            };

            await db.EventGatekeeperMapping.AddAsync(egm);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        #endregion
    }
}
