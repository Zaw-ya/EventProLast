using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.DAL.Common;
using EventPro.Web.Filters;
using EventPro.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text.log;

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        #region Operator Assignment Management

        /// <summary>
        /// GET: Admin/GetEventAssignedOperator
        /// Displays the operator assignment page for a specific event
        /// Shows available and assigned operators for the event
        /// </summary>
        /// <param name="id">Event ID to manage operators for</param>
        /// <returns>View with event details and operator assignment interface</returns>
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> GetEventAssignedOperator(int id)
        {
            // Set event ID for view reference
            ViewBag.EventId = id;

            // Load event details from view
            var model = await db.VwEvents.Where(e => e.Id == id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

            // Set event location for display
            ViewBag.EventLocation = model.Glocation;

            SetBreadcrum("Manage Operator", "/");
            return View("AssignOperator", model);
        }

        /// <summary>
        /// POST: Admin/GetAvaliableOperators
        /// Retrieves paginated list of available operators for DataTables display
        /// Excludes operators already assigned to the event
        /// Shows operators with active and approved status
        /// </summary>
        /// <param name="eventId">Event ID to check operator availability for</param>
        /// <returns>JSON data for DataTables with available operators</returns>
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> GetAvaliableOperators(int eventId)
        {
            // Parse DataTables pagination parameters
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            // Parse search value for filtering
            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            // Get event information
            var eventInfo = await db.Events.Where(p => p.Id == eventId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Query all active and approved operators
            IQueryable<Users> allOperators = db.Users.Where(e => e.Role == RoleIds.Operator && e.IsActive.Value && e.Approved.Value)
                .AsNoTracking();

            // Apply search filter on operator name or username
            if (!string.IsNullOrEmpty(searchValue))
            {
                allOperators = allOperators
                    .Where(e => string.Concat(e.FirstName, " ", e.LastName).Contains(searchValue.ToString().Trim()) ||
                    e.UserName.Contains(searchValue.ToString().Trim()));
            }

            // Get operators already assigned to this event
            IQueryable<EventOperator> assignedOperator = db.EventOperator
                .Where(p => p.EventId == eventId)
                .AsNoTracking();

            // Exclude already assigned operators from available list
            allOperators = allOperators.Where(e => assignedOperator.All(c => c.OperatorId != e.UserId));
            int avalaibleGateKeepersCount = await allOperators.CountAsync();
            var allOperatorResult = await allOperators.Skip(skip).Take(pageSize).ToListAsync();

            // Build view models for available operators
            List<EventOperatorsVM> AvaliableOperatorsVM = new();

            foreach (var Operator in allOperatorResult)
            {
                EventOperatorsVM assignOperatorVM = new(Operator);
                AvaliableOperatorsVM.Add(assignOperatorVM);
            }

            // Return DataTables JSON format
            var recordsTotal = avalaibleGateKeepersCount;
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = AvaliableOperatorsVM
            };

            return Ok(jsonData);
        }

        /// <summary>
        /// POST: Admin/GetAssignedOperators
        /// Retrieves paginated list of operators already assigned to an event
        /// Used for displaying current operator assignments in DataTables
        /// </summary>
        /// <param name="eventId">Event ID to get assigned operators for</param>
        /// <returns>JSON data for DataTables with assigned operator details</returns>
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> GetAssignedOperators(int eventId)
        {
            // Parse DataTables pagination parameters
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            // Parse search value (currently not used in filtering)
            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            // Query operators assigned to this event through EventOperators relationship
            IQueryable<Users> assignedOperators = db.Users.Include(e => e.EventOperators)
                .Where(p => p.EventOperators.Any(e => e.EventId == eventId))
                                 .AsNoTracking();

            // Get total count and paginated results
            int assignedOperatorCount = await assignedOperators.CountAsync();
            var assignedOperatorsResult = await assignedOperators.Skip(skip).Take(pageSize).ToListAsync();

            // Build view models for assigned operators
            List<EventOperatorsVM> assignedOperatorsVM = new();

            foreach (var Operator in assignedOperatorsResult)
            {
                EventOperatorsVM assignOperatorVM = new(Operator);
                assignedOperatorsVM.Add(assignOperatorVM);
            }

            // Return DataTables JSON format
            var recordsTotal = assignedOperatorCount;
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = assignedOperatorsVM
            };

            return Ok(jsonData);
        }

        /// <summary>
        /// POST: Admin/UnassignOperator
        /// Removes an operator assignment from an event
        /// Deletes the EventOperator record for the specified event and operator
        /// </summary>
        /// <param name="eventId">Event ID to remove operator from</param>
        /// <param name="userid">User ID of the operator to unassign</param>
        /// <returns>JSON with success status</returns>
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> UnassignOperator(int eventId, int userid)
        {
            try
            {
                // Find the operator assignment by event ID and operator ID
                var eventOperator = db.EventOperator.Where(e => e.EventId == eventId &&
                e.OperatorId == userid)
                    .FirstOrDefault();

                // Remove the operator assignment
                db.EventOperator.Remove(eventOperator);
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false , message = ex});
            }
        }

        /// <summary>
        /// POST: Admin/AssignOperator
        /// Assigns an operator to an event
        /// Creates a new EventOperator record with event start/end dates
        /// </summary>
        /// <param name="eventId">Event ID to assign operator to</param>
        /// <param name="userid">User ID of the operator to assign</param>
        /// <returns>JSON with success status</returns>
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> AssignOperator(int eventId, int userid)
        {
            // Get event details to capture start and end dates
            var events = await db.Events.Where(e => e.Id == eventId).FirstOrDefaultAsync();

            try
            {
                // Create new operator assignment with event timing
                var eventOperator = new EventOperator()
                {
                    OperatorId = userid,
                    EventId = eventId,
                    EventStart = events.EventFrom,
                    EventEnd = events.EventTo,
                    BulkOperatroEventsId = -1 // Not part of bulk assignment
                };

                await db.EventOperator.AddAsync(eventOperator);
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}
