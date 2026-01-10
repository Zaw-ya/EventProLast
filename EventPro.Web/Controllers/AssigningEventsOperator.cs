using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.Web.Filters;
using EventPro.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public class AssigningEventsOperator : Controller
    {
        private readonly EventProContext db;
        private readonly IConfiguration _configuration;
        public AssigningEventsOperator(IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
        }

        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Icon = "nav-icon fas fa-tasks";
            SetBreadcrum("Shared Operator Events", "/admin");

            var users = await db.Users.Where(p => p.Role == 4 &&
            p.IsActive == true &&
            p.Approved == true)
              .OrderByDescending(e => e.UserId)
                               .ToListAsync();

            ViewBag.operators = new SelectList(
                             users.Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName }),
                             "UserId",
                             "FullName"
);

            var bulkOperatorEvents = new BulkOperatorEvents();
            return View(bulkOperatorEvents);
        }


        [HttpPost]
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> Assign(BulkOperatorEvents model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "??? ??? ??." });
            }
            try
            {
                if (db.BulkOperatorEvents.Where(e => e.OperatorAssignedFromId == model.OperatorAssignedFromId &&
            e.OperatorAssignedToId == model.OperatorAssignedToId).Count() > 0)
                {
                    return Json(new { success = false, message = "???? ?????? ????? ?????? ????? ??? ???? ????????? ????? ??? ???????? ??????? ????? ??? ????? ?? ?????? ???? ?????????." });
                }

                model.AssignedById = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                model.AssignedOn = DateTime.UtcNow;

                db.BulkOperatorEvents.Add(model);
                await db.SaveChangesAsync();
                int BulkID = model.Id;

                var existingEventIds = await db.EventOperator
                            .Where(e => e.OperatorId == (model.OperatorAssignedToId ?? 0))
                            .Select(e => e.EventId)
                            .ToListAsync();

                List<EventOperator> EventsOperator = await db.EventOperator
                    .Where(e => e.OperatorId == model.OperatorAssignedFromId 
                    && e.BulkOperatroEventsId == null)
                    .Select(X => new EventOperator
                    {
                        BulkOperatroEventsId = BulkID,
                        EventId = X.EventId,
                        EventStart = X.EventStart,
                        EventEnd = X.EventEnd,
                        OperatorId = model.OperatorAssignedToId ?? 0,
                    })
                    .Where(e => !existingEventIds.Contains(e.EventId))
                    .ToListAsync();

                db.EventOperator.AddRange(EventsOperator);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "??? ??? ??." });
            }


            return Json(new { success = true, message = "??? ?????? ????????? ?????" });
        }

        [HttpPost("~/api/GetBulkOperatorEvents")]
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> GetBulkOperatorEvents()
        {
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            var sortColumn = Request.Form[string.Concat("columns[", Request.Form["order[0][column]"], "][name]")];
            var sortColumnDirection = Request.Form["order[0][dir]"];


            var result = await db.BulkOperatorEvents
                .Include(e => e.OperatorAssignedFrom)
                .Include(e => e.OperatorAssignedTo)
                .Include(e => e.AssignedBy)
                .OrderByDescending(e => e.AssignedOn)
                .Skip(skip).Take(pageSize).ToListAsync();
            List<BulkOperatorEventsVM> bulkOperatorEventsVMs = new();
            int counter = 0;
            foreach (var evnt in result)
            {
                counter++;
                BulkOperatorEventsVM bulkOperatorEventsVM = new(evnt);
                bulkOperatorEventsVM.ViewNumber = counter;
                bulkOperatorEventsVMs.Add(bulkOperatorEventsVM);
            }

            var recordsTotal = await db.BulkOperatorEvents.CountAsync();
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = bulkOperatorEventsVMs
            };

            return Ok(jsonData);
        }


        [HttpGet]
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> UnAssign(int id)
        {
            try
            {
                var bulkAssigningOperatorEvents = await db.BulkOperatorEvents.FirstOrDefaultAsync(e => e.Id == id);
                List<EventOperator> eventsOperators = await db.EventOperator.Where(e => e.BulkOperatroEventsId == id)
                                                     .ToListAsync();
                db.BulkOperatorEvents.Remove(bulkAssigningOperatorEvents);
                db.EventOperator.RemoveRange(eventsOperators);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "??? ??? ??." });
            }
            return Json(new { success = true, message = "?? ????? ?????? ?????????" });
        }

        private void SetBreadcrum(string title, string link)
        {
            ViewBag.PageTitle = title;
            ViewBag.BackLink = link;
        }
    }
}
