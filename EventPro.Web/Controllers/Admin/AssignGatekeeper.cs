using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
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
        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> AssignGatekeeper(int id)
        {
            ViewBag.EventId = id;
            var model = await db.VwEvents.Where(e => e.Id == id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            ViewBag.EventLocation = model.Glocation;

            SetBreadcrum("Manage Gatekeeper", "/");
            return View("AssignGatekeeper - Copy", model);
        }

        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> GetAvaliableGateKeepers(int eventId)
        {
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            var eventInfo = await db.Events.Where(p => p.Id == eventId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            IQueryable<Users> allGateKeepers = db.Users.Where(e => e.Role == 3 && e.IsActive.Value && e.Approved.Value)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchValue))
            {
                allGateKeepers = allGateKeepers.Where(e => string.Concat(e.FirstName, " ", e.LastName).Contains(searchValue));
            }

            IQueryable<VwEventGatekeeper> assignedGateKeepers = db.VwEventGatekeeper
                .Where(p => p.TaskId != null && p.EventId == eventId)
                .AsNoTracking();

            allGateKeepers = allGateKeepers.Where(e => assignedGateKeepers.All(c => c.UserId != e.UserId));
            int avalaibleGateKeepersCount = await allGateKeepers.CountAsync();
            var allGateKeepersResult = await allGateKeepers.Skip(skip).Take(pageSize).ToListAsync();

            IQueryable<VwGateKeeperScheduled> gateKeeperScheduled = db.VwGateKeeperScheduled
                    .Where(p => p.EventFrom >= eventInfo.EventFrom && p.EventId != eventId)
                    .AsNoTracking();

            var gateKeeperScheduledResult = await gateKeeperScheduled.ToListAsync();

            List<AvaliableGateKeepersVM> avaliableGateKeepersVM = new();

            foreach (var gateKeeper in allGateKeepersResult)
            {
                var AssignedEvents = gateKeeperScheduledResult.Where(e => e.GatekeeperId == gateKeeper.UserId)
                    .Select(e => e.EventTitle)
                    .ToList();
                AvaliableGateKeepersVM assignGateKeeperVM = new(gateKeeper, AssignedEvents);
                avaliableGateKeepersVM.Add(assignGateKeeperVM);
            }

            var recordsTotal = avalaibleGateKeepersCount;
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = avaliableGateKeepersVM
            };

            return Ok(jsonData);
        }

        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> GetAssignedGateKeepers(int eventId)
        {
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            IQueryable<VwEventGatekeeper> assignedGateKeepers = db.VwEventGatekeeper.Where(p => p.TaskId != null && p.EventId == eventId)
                                 .AsNoTracking();

            int assignedGateKeepersCount = await assignedGateKeepers.CountAsync();
            var assignedGateKeepersResult = await assignedGateKeepers.Skip(skip).Take(pageSize).ToListAsync();

            List<AssignedGateKeepersVM> assignedGateKeepersVM = new();

            foreach (var gateKeeper in assignedGateKeepersResult)
            {
                AssignedGateKeepersVM assignGateKeeperVM = new(gateKeeper);
                assignedGateKeepersVM.Add(assignGateKeeperVM);
            }

            var recordsTotal = assignedGateKeepersCount;
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = assignedGateKeepersVM
            };

            return Ok(jsonData);
        }

        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> Unassigned(int taskId)
        {
            EventGatekeeperMapping egm = await db.EventGatekeeperMapping.Where(p => p.TaskId == taskId).FirstOrDefaultAsync();
            if (egm == null) return Json(new { success = false });

            int eventId = Convert.ToInt32(egm.EventId);
            db.EventGatekeeperMapping.Remove(egm);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        [AuthorizeRoles("Administrator", "Agent")]
        public async Task<IActionResult> Assigned(int eventId, int userid)
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (await db.EventGatekeeperMapping.Where(e => e.EventId == eventId && e.GatekeeperId == userid).FirstOrDefaultAsync() != null)
            {
                return Json(new { success = false });
            }

            EventGatekeeperMapping egm = new EventGatekeeperMapping
            {
                EventId = eventId,
                GatekeeperId = userid,
                AssignedBy = userid,
                AsssignedOn = DateTime.UtcNow,
                IsActive = true
            };
            await db.EventGatekeeperMapping.AddAsync(egm);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
