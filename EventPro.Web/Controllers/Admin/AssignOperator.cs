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

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> GetEventAssignedOperator(int id)
        {
            ViewBag.EventId = id;
            var model = await db.VwEvents.Where(e => e.Id == id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            ViewBag.EventLocation = model.Glocation;

            SetBreadcrum("Manage Operator", "/");
            return View("AssignOperator", model);
        }

        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> GetAvaliableOperators(int eventId)
        {
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            var eventInfo = await db.Events.Where(p => p.Id == eventId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            IQueryable<Users> allOperators = db.Users.Where(e => e.Role == RoleIds.Operator && e.IsActive.Value && e.Approved.Value)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchValue))
            {
                allOperators = allOperators
                    .Where(e => string.Concat(e.FirstName, " ", e.LastName).Contains(searchValue.ToString().Trim()) ||
                    e.UserName.Contains(searchValue.ToString().Trim()));
            }

            IQueryable<EventOperator> assignedOperator = db.EventOperator
                .Where(p => p.EventId == eventId)
                .AsNoTracking();

            allOperators = allOperators.Where(e => assignedOperator.All(c => c.OperatorId != e.UserId));
            int avalaibleGateKeepersCount = await allOperators.CountAsync();
            var allOperatorResult = await allOperators.Skip(skip).Take(pageSize).ToListAsync();

            List<EventOperatorsVM> AvaliableOperatorsVM = new();

            foreach (var Operator in allOperatorResult)
            {
                EventOperatorsVM assignOperatorVM = new(Operator);
                AvaliableOperatorsVM.Add(assignOperatorVM);
            }

            var recordsTotal = avalaibleGateKeepersCount;
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = AvaliableOperatorsVM
            };

            return Ok(jsonData);
        }

        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> GetAssignedOperators(int eventId)
        {
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            IQueryable<Users> assignedOperators = db.Users.Include(e => e.EventOperators)
                .Where(p => p.EventOperators.Any(e => e.EventId == eventId))
                                 .AsNoTracking();

            int assignedOperatorCount = await assignedOperators.CountAsync();
            var assignedOperatorsResult = await assignedOperators.Skip(skip).Take(pageSize).ToListAsync();

            List<EventOperatorsVM> assignedOperatorsVM = new();

            foreach (var Operator in assignedOperatorsResult)
            {
                EventOperatorsVM assignOperatorVM = new(Operator);
                assignedOperatorsVM.Add(assignOperatorVM);
            }

            var recordsTotal = assignedOperatorCount;
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = assignedOperatorsVM
            };

            return Ok(jsonData);
        }

        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> UnassignOperator(int eventId, int userid)
        {
            try
            {

                var eventOperator = db.EventOperator.Where(e => e.EventId == eventId &&
                e.OperatorId == userid)
                    .FirstOrDefault();
                db.EventOperator.Remove(eventOperator);
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }

        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> AssignOperator(int eventId, int userid)
        {
            var events = await db.Events.Where(e=>e.Id == eventId).FirstOrDefaultAsync();
            try
            {

                var eventOperator = new EventOperator()
                {
                    OperatorId = userid,
                    EventId = eventId,
                    EventStart = events.EventFrom,
                    EventEnd = events.EventTo,
                    BulkOperatroEventsId = -1
                };

                await db.EventOperator.AddAsync(eventOperator);
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });

            }
        }

    }
}
