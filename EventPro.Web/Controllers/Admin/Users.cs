using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.DAL.Common;
using EventPro.Web.Common;
using EventPro.Web.Filters;
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
        #region User Management - List and Retrieval

        /// <summary>
        /// GET: Admin/Users
        /// Displays the user management page with role-based filtering
        /// Shows different role filters based on current user's role
        /// Operators and Supervisors see limited options
        /// </summary>
        /// <returns>View with user list and role filter options</returns>
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor")]
        public async Task<IActionResult> Users()
        {
            ViewBag.SelectedRole = -1;
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var operatorRole = HttpContext.User.FindFirst("Operator")?.Value;

            // Configure role filter dropdown for non-operators/supervisors
            if (operatorRole != "Operator" && operatorRole != "Supervisor")
            {
                // Define role display order
                var roleOrder = new[] { "Administrator", "Operator", "Agent", "Supervisor", "Accounting", "Client", "GateKeeper" };
                var rolesFromDb = await db.Roles.ToListAsync();
                var orderedRoles = rolesFromDb.OrderBy(r => Array.IndexOf(roleOrder, r.RoleName)).ToList();

                // Build role select list with "All" option
                var roleSelectList = new List<SelectListItem> { new SelectListItem { Text = "All", Value = "-1" } };
                roleSelectList.AddRange(orderedRoles.Select(r => new SelectListItem
                {
                    Text = r.RoleName,
                    Value = r.Id.ToString()
                }));

                ViewBag.ReminderMessageTempName = new SelectList(roleSelectList, "Value", "Text", ViewBag.SelectedRole);
            }

            ViewBag.Icon = "nav-icon fas fa-user";
            SetBreadcrum("Users", "/");
            ViewBag.Roles = await db.Roles.ToListAsync();

            return View("Users - Copy");
        }

        /// <summary>
        /// POST: Admin/GetUsers
        /// Retrieves paginated and filtered list of users for DataTables display
        /// Implements role-based access control:
        /// - Operators: See only their assigned event clients
        /// - Supervisors: See only clients
        /// - Others: See all users or filtered by role
        /// </summary>
        /// <param name="roleId">Role ID to filter users (optional)</param>
        /// <returns>JSON data for DataTables with user information</returns>
        [AuthorizeRoles("Administrator", "Agent", "Operator", "Supervisor")]
        public async Task<IActionResult> GetUsers(int roleId)
        {
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            // Parse DataTables pagination parameters
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            // Parse search value for filtering
            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            // Base query with search filtering on multiple fields
            IQueryable<Users> users = db.Users.Where(e =>
            string.IsNullOrEmpty(searchValue) ? true
            : (e.UserId.ToString().Contains(searchValue))
            || (string.Concat(e.FirstName, " ", e.LastName).Contains(searchValue))
            || (e.Email.Contains(searchValue))
            || (searchValue.Contains(e.Email))
            || (e.UserName.Contains(searchValue))
            || (searchValue.Contains(e.UserName))
             ).AsNoTracking();

            string role = userRole;

            // Apply role-based filtering
            if (HasOperatorRole())
            {
                // Operators can only see clients from their assigned events or clients they created
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var createdForIds = await db.Events
                    .Where(e => e.EventOperators.Any(p => p.OperatorId == userId))
                    .Select(e => e.CreatedFor)
                    .Distinct()
                    .ToListAsync();

                users = users.Where(p => createdForIds.Contains(p.UserId) || p.CreatedBy == userId);
                users = users.Where(p => p.Role == RoleIds.Client);
            }
            else if (HasSupervisorRole())
            {
                // Supervisors can only see clients
                users = users.Where(p => p.Role == RoleIds.Client);
            }
            else if (roleId > 0)
            {
                // Filter by specific role if provided
                users = users.Where(p => p.Role == roleId);
            }

            // Order by user ID descending
            users = users.OrderByDescending(p => p.UserId);
            var result = await users.Skip(skip).Take(pageSize).ToListAsync();

            // Build view models for users
            List<UserVM> usersVM = new();

            foreach (var user in result)
            {
                UserVM userVM = new(user);
                usersVM.Add(userVM);
            }

            // Return DataTables JSON format
            var recordsTotal = await users.CountAsync();
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = usersVM
            };

            return Ok(jsonData);
        }

        /// <summary>
        /// GET: Admin/GetUsersForSelect2
        /// Retrieves paginated users for Select2 dropdown (AJAX)
        /// Used for client selection in forms
        /// Implements same role-based access as GetUsers
        /// </summary>
        /// <param name="searchTerm">Search term for filtering users</param>
        /// <param name="page">Page number for pagination</param>
        /// <returns>JSON data formatted for Select2 plugin</returns>
        [AuthorizeRoles("Administrator", "Agent", "Operator", "Supervisor")]
        [HttpGet]
        public async Task<IActionResult> GetUsersForSelect2(string searchTerm = "", int page = 1)
        {
            // Verify access permissions
            var access = AccessService.AllowAccessForAdministratorAndOperatorOrAndAgent(HttpContext);
            if (access != null) return access;

            const int pageSize = 20;
            IQueryable<Users> users = db.Users.AsNoTracking();

            // Apply role-based filtering
            if (HasOperatorRole())
            {
                // Operators see clients from their events or created by them
                var userId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var createdForIds = await db.Events
                    .Where(e => e.EventOperators.Any(p => p.OperatorId == userId))
                    .Select(e => e.CreatedFor)
                    .Distinct()
                    .ToListAsync();

                users = users.Where(p => createdForIds.Contains(p.UserId) || p.CreatedBy == userId);
                users = users.Where(p => p.Role == RoleIds.Client);
            }
            else
            {
                // Other roles see all clients
                users = users.Where(p => p.Role == RoleIds.Client);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                users = users.Where(e =>
                    e.FirstName.Contains(searchTerm) ||
                    e.LastName.Contains(searchTerm) ||
                    e.Email.Contains(searchTerm) ||
                    e.UserName.Contains(searchTerm));
            }

            var totalCount = await users.CountAsync();

            // Get paginated results formatted for Select2
            var result = await users
                .OrderByDescending(e => e.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    id = e.UserId,
                    text = e.UserName
                })
                .ToListAsync();

            // Return Select2-formatted JSON
            return Json(new
            {
                results = result,
                pagination = new { more = totalCount > page * pageSize }
            });
        }

        #endregion

        #region User Management - View and Edit

        /// <summary>
        /// GET: Admin/_Users
        /// Displays user edit form for a specific user
        /// Implements access control for operators (can only edit accessible users)
        /// </summary>
        /// <param name="id">User ID to edit</param>
        /// <returns>View with user details for editing</returns>
        public async Task<IActionResult> _Users(int id)
        {
            var createdBy = await db.Users.Where(e => e.UserId == id).Select(e => e.CreatedBy)
                .FirstOrDefaultAsync();

            // Check operator access permissions
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                    .Include(e => e.Event)
                    .Include(e => e.Operator)
                    .Any(e => e.OperatorId == userId && (e.Event.CreatedFor == id || e.OperatorId == createdBy));

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            await SetUserDefaultValues();
            return View(await db.Users.Where(p => p.UserId == id).FirstOrDefaultAsync());
        }

        /// <summary>
        /// POST: Admin/_Users
        /// Updates user information in the database
        /// Agents can only update approval status
        /// Other roles can update full user profile
        /// Logs update action in audit trail
        /// </summary>
        /// <param name="user">User object with updated information</param>
        /// <returns>Redirect to Users list on success</returns>
        [HttpPost]
        [AuthorizeRoles("Administrator", "Agent", "Operator", "Supervisor")]
        public async Task<IActionResult> _Users(Users user)
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            ViewBag.Icon = "nav-icon fas fa-user";

            // Load existing user from database
            Users _users = await db.Users.Where(p => p.UserId == user.UserId).FirstOrDefaultAsync();

            if (_users == null)
            {
                return NotFound();
            }

            // Agents can only update approval status
            if (User.IsInRole("Agent"))
            {
                _users.Approved = user.Approved;
            }
            else
            {
                // Update all user fields
                _users.FirstName = user.FirstName;
                _users.LastName = user.LastName;

                // Only update password if provided
                if (user.Password != null && user.Password.Length > 0)
                    _users.Password = user.Password;

                _users.Address = user.Address;
                _users.Email = user.Email;
                _users.Role = user.Role;
                _users.PrimaryContactNo = user.PrimaryContactNo;
                _users.SecondaryContantNo = user.SecondaryContantNo;
                _users.Gender = user.Gender;
                _users.Approved = user.Approved;
                _users.CityId = user.CityId;
                _users.SendNotificationsOrEmails = user.SendNotificationsOrEmails;
            }

            // Track modification metadata
            _users.ModifiedBy = userId;
            _users.ModifiedOn = DateTime.UtcNow;

            await db.SaveChangesAsync();

            // Log audit trail
            await _auditLogService.AddAsync(userId, null, DAL.Enum.ActionEnum.UpdateUser);
            return RedirectToAction(AppAction.Users, AppController.Admin);
        }

        #endregion

        #region User Management - Create

        /// <summary>
        /// GET: Admin/CreateUser
        /// Displays user creation form
        /// Loads default values for dropdowns (roles, genders, locations)
        /// </summary>
        /// <param name="id">User ID (not used for creation)</param>
        /// <returns>View with user creation form</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> CreateUser(int id)
        {
            await SetUserDefaultValues();
            return View(await db.Users.Where(p => p.UserId == id).FirstOrDefaultAsync());
        }

        /// <summary>
        /// POST: Admin/CreateUser
        /// Creates a new user in the database
        /// Validates username uniqueness
        /// Sets default values (Active=true, Approved=true)
        /// Logs creation action in audit trail
        /// </summary>
        /// <param name="user">User object to create</param>
        /// <returns>Redirect to Users list on success, or view with error on failure</returns>
        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> CreateUser(Users user)
        {
            user.UserName = user.UserName.Trim();
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            SetBreadcrum("Users", "/");

            // Check if username already exists
            var uExists = await db.Users.Where(p => p.UserName == user.UserName).FirstOrDefaultAsync();
            if (uExists != null)
            {
                // Username conflict - reload form with error message
                ViewBag.UserGender = new SelectList(new string[] { "M", "F" });
                ViewBag.UserRole = new SelectList(db.Roles.ToList(), "Id", "RoleName");
                TempData["Validation"] = "Username already in use.";
                ViewBag.EventLocations =
                new SelectList((await db.City.Include(c => c.Country).ToListAsync())
               .Select(e => new { e.Id, Location = e.CityName + "|" + e.Country.CountryName }), "Id", "Location");
                return View(uExists);
            }
            else
            {
                // Create new user with default values
                user.CreatedBy = userId;
                user.CreatedOn = DateTime.UtcNow;
                user.IsActive = true;
                user.Approved = true;

                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();

                // Log audit trail
                await _auditLogService.AddAsync(userId, null, DAL.Enum.ActionEnum.CreateUser);
                return RedirectToAction(AppAction.Users, AppController.Admin);
            }
        }

        #endregion

        #region User Management - Activate/Deactivate

        /// <summary>
        /// POST: Admin/ActiveOrDeactiveUser
        /// Toggles user active/inactive status
        /// Operators can only toggle status for accessible users
        /// Logs activation/deactivation in audit trail
        /// </summary>
        /// <param name="id">User ID to toggle status for</param>
        /// <returns>JSON with success status</returns>
        [AuthorizeRoles("Administrator", "Agent", "Operator", "Supervisor")]
        public async Task<IActionResult> ActiveOrDeactiveUser(int id)
        {
            var createdBy = await db.Users.Where(e => e.UserId == id).Select(e => e.CreatedBy)
                 .FirstOrDefaultAsync();
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Check operator access permissions
            if (HasOperatorRole())
            {
                var isOperatorHasAccess = db.EventOperator
                    .Include(e => e.Event)
                    .Include(e => e.Operator)
                    .Any(e => e.OperatorId == userId && (e.Event.CreatedFor == id || e.OperatorId == createdBy));

                if (!isOperatorHasAccess)
                    return Json(new { success = false });
            }

            // Toggle user active status
            var user = await db.Users.Where(p => p.UserId == id).FirstOrDefaultAsync();
            if (user == null) return Json(new { success = false });

            user.IsActive = !user.IsActive;

            await db.SaveChangesAsync();

            // Log audit trail
            await _auditLogService.AddAsync(userId, null, DAL.Enum.ActionEnum.ActivateOrDeactivateUser);
            return Json(new { success = true });
        }

        #endregion

        #region User Management - Send Details

        /// <summary>
        /// POST: Admin/SendDetails
        /// Sends user account details via WhatsApp
        /// Uses WATI service to send account information template
        /// Operators can only send details for accessible users
        /// </summary>
        /// <param name="userId">User ID to send details for</param>
        /// <returns>JSON with success status</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendDetails(int userId)
        {
            var createdBy = await db.Users.Where(e => e.UserId == userId).Select(e => e.CreatedBy)
                               .FirstOrDefaultAsync();

            // Check operator access permissions
            if (HasOperatorRole())
            {
                var operatorId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                    .Include(e => e.Event)
                    .Include(e => e.Operator)
                    .Any(e => e.OperatorId == operatorId && (e.Event.CreatedFor == userId || e.OperatorId == createdBy));

                if (!isOperatorHasAccess)
                    return Json(new { success = false });
            }

            var uExists = await db.Users.Where(p => p.UserId == userId).FirstOrDefaultAsync();

            // Validate user exists and has contact number
            if (uExists == null || string.IsNullOrEmpty(uExists.PrimaryContactNo))
            {
                return Json(new { success = false });
            }

            // Send WhatsApp message with account details
            var msg = await _watiService.SendUserAccountDetailsTemplate(uExists);

            if (msg == "Message Processed Successfully")
            {
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sets default values for user form ViewBag properties
        /// Configures role dropdowns based on current user's role
        /// Loads gender, approval, notification, and location options
        /// </summary>
        private async Task SetUserDefaultValues()
        {
            SetBreadcrum("Users", "/");
            ViewBag.Icon = "nav-icon fas fa-pencil-square-o";
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            string role = userRole;

            // Define role display order
            var roleOrder = new[] { "Administrator", "Operator", "Agent", "Supervisor", "Accounting", "Client", "GateKeeper" };

            // Configure role dropdown based on current user
            if (role == "Operator" || role == "Supervisor")
            {
                // Operators and Supervisors can only create Clients
                ViewBag.UserRole = new SelectList(await db.Roles.Where(p => p.RoleName == "Client" || p.Id == RoleIds.Client).ToListAsync(), "Id", "RoleName");
            }
            else
            {
                // Other roles can create any role with ordered display
                var roles = await db.Roles.ToListAsync();
                var orderedRoles = roles.OrderBy(r => Array.IndexOf(roleOrder, r.RoleName)).ToList();
                ViewBag.UserRole = new SelectList(orderedRoles, "Id", "RoleName");
            }

            // Set other dropdown options
            ViewBag.UserGender = new SelectList(new string[] { "M", "F" });
            ViewBag.Approved = new SelectList(new string[] { "True", "False" });
            ViewBag.SendNotificationsOrEmails = new SelectList(new string[] { "True", "False" });

            // Load city/country location dropdown
            ViewBag.EventLocations =
               new SelectList((await db.City.Include(c => c.Country).ToListAsync())
               .Select(e => new { e.Id, Location = e.CityName + "|" + e.Country.CountryName }), "Id", "Location");
        }

        #endregion
    }
}
