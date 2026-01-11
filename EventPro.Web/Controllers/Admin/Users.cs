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
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor")]
        public async Task<IActionResult> Users()
        {
            ViewBag.SelectedRole = -1;
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var operatorRole = HttpContext.User.FindFirst("Operator")?.Value;

            if (operatorRole != "Operator" && operatorRole != "Supervisor")
            {
                // Get roles from database with proper ordering
                var roleOrder = new[] { "Administrator", "Operator", "Agent", "Supervisor", "Accounting", "Client", "GateKeeper" };
                var rolesFromDb = await db.Roles.ToListAsync();
                var orderedRoles = rolesFromDb.OrderBy(r => Array.IndexOf(roleOrder, r.RoleName)).ToList();

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

        [AuthorizeRoles("Administrator", "Agent", "Operator", "Supervisor")]
        public async Task<IActionResult> GetUsers(int roleId)
        {
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

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

            if (HasOperatorRole())
            {
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
                users = users.Where(p => p.Role == RoleIds.Client);
            }
            else if (roleId > 0)
            {
                users = users.Where(p => p.Role == roleId);
            }

            users = users.OrderByDescending(p => p.UserId);
            var result = await users.Skip(skip).Take(pageSize).ToListAsync();

            List<UserVM> usersVM = new();

            foreach (var user in result)
            {
                UserVM userVM = new(user);
                usersVM.Add(userVM);
            }

            var recordsTotal = await users.CountAsync();
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = usersVM
            };

            return Ok(jsonData);
        }

        [AuthorizeRoles("Administrator", "Agent", "Operator", "Supervisor")]
        [HttpGet]
        public async Task<IActionResult> GetUsersForSelect2(string searchTerm = "", int page = 1)
        {
            var access = AccessService.AllowAccessForAdministratorAndOperatorOrAndAgent(HttpContext);
            if (access != null) return access;

            const int pageSize = 20;
            IQueryable<Users> users = db.Users.AsNoTracking();

            if (HasOperatorRole())
            {
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
                users = users.Where(p => p.Role == RoleIds.Client);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                users = users.Where(e =>
                    e.FirstName.Contains(searchTerm) ||
                    e.LastName.Contains(searchTerm) ||
                    e.Email.Contains(searchTerm) ||
                    e.UserName.Contains(searchTerm));
            }

            var totalCount = await users.CountAsync();

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

            return Json(new
            {
                results = result,
                pagination = new { more = totalCount > page * pageSize }
            });
        }

        public async Task<IActionResult> _Users(int id)
        {
            var createdBy = await db.Users.Where(e => e.UserId == id).Select(e => e.CreatedBy)
                .FirstOrDefaultAsync();

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

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> CreateUser(int id)
        {
            await SetUserDefaultValues();
            return View(await db.Users.Where(p => p.UserId == id).FirstOrDefaultAsync());
        }

        [AuthorizeRoles("Administrator", "Agent", "Operator", "Supervisor")]
        public async Task<IActionResult> ActiveOrDeactiveUser(int id)
        {
            var createdBy = await db.Users.Where(e => e.UserId == id).Select(e => e.CreatedBy)
                 .FirstOrDefaultAsync();
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (HasOperatorRole())
            {
                var isOperatorHasAccess = db.EventOperator
                    .Include(e => e.Event)
                    .Include(e => e.Operator)
                    .Any(e => e.OperatorId == userId && (e.Event.CreatedFor == id || e.OperatorId == createdBy));


                if (!isOperatorHasAccess)
                    return Json(new { success = false });
            }

            var user = await db.Users.Where(p => p.UserId == id).FirstOrDefaultAsync();
            if (user == null) return Json(new { success = false });
            user.IsActive = !user.IsActive;

            await db.SaveChangesAsync();
            await _auditLogService.AddAsync(userId, null, DAL.Enum.ActionEnum.ActivateOrDeactivateUser);
            return Json(new { success = true });
        }


        [HttpPost]
        [AuthorizeRoles("Administrator", "Agent", "Operator", "Supervisor")]
        public async Task<IActionResult> _Users(Users user)
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            ViewBag.Icon = "nav-icon fas fa-user";
            Users _users = await db.Users.Where(p => p.UserId == user.UserId).FirstOrDefaultAsync();

            if (_users == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Agent"))
            {
                _users.Approved = user.Approved;
            }
            else
            {
                _users.FirstName = user.FirstName;
                _users.LastName = user.LastName;
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

            _users.ModifiedBy = userId;
            _users.ModifiedOn = DateTime.UtcNow;

            await db.SaveChangesAsync();

            await _auditLogService.AddAsync(userId, null, DAL.Enum.ActionEnum.UpdateUser);
            return RedirectToAction(AppAction.Users, AppController.Admin);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> CreateUser(Users user)
        {
            user.UserName = user.UserName.Trim();
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            SetBreadcrum("Users", "/");
            var uExists = await db.Users.Where(p => p.UserName == user.UserName).FirstOrDefaultAsync();
            if (uExists != null)
            {
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
                user.CreatedBy = userId;
                user.CreatedOn = DateTime.UtcNow;
                user.IsActive = true;
                user.Approved = true;
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
                await _auditLogService.AddAsync(userId, null, DAL.Enum.ActionEnum.CreateUser);
                return RedirectToAction(AppAction.Users, AppController.Admin);
            }
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendDetails(int userId)
        {
            var createdBy = await db.Users.Where(e => e.UserId == userId).Select(e => e.CreatedBy)
                               .FirstOrDefaultAsync();

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

            if (uExists == null || string.IsNullOrEmpty(uExists.PrimaryContactNo))
            {
                return Json(new { success = false });
            }

            var msg = await _watiService.SendUserAccountDetailsTemplate(uExists);

            if (msg == "Message Processed Successfully")
            {
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        private async Task SetUserDefaultValues()
        {
            SetBreadcrum("Users", "/");
            ViewBag.Icon = "nav-icon fas fa-pencil-square-o";
            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            string role = userRole;

            // Define role order
            var roleOrder = new[] { "Administrator", "Operator", "Agent", "Supervisor", "Accounting", "Client", "GateKeeper" };

            if (role == "Operator" || role == "Supervisor")
            {
                ViewBag.UserRole = new SelectList(await db.Roles.Where(p => p.RoleName == "Client" || p.Id == RoleIds.Client).ToListAsync(), "Id", "RoleName");
            }
            else
            {
                var roles = await db.Roles.ToListAsync();
                var orderedRoles = roles.OrderBy(r => Array.IndexOf(roleOrder, r.RoleName)).ToList();
                ViewBag.UserRole = new SelectList(orderedRoles, "Id", "RoleName");
            }
            ViewBag.UserGender = new SelectList(new string[] { "M", "F" });
            ViewBag.Approved = new SelectList(new string[] { "True", "False" });
            ViewBag.SendNotificationsOrEmails = new SelectList(new string[] { "True", "False" });
            ViewBag.EventLocations =
               new SelectList((await db.City.Include(c => c.Country).ToListAsync())
               .Select(e => new { e.Id, Location = e.CityName + "|" + e.Country.CountryName }), "Id", "Location");
        }
    }
}
