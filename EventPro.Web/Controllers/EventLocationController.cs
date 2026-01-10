using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventPro.DAL.Models;
using EventPro.Web.Common;
using EventPro.Web.Filters;
using EventPro.Web.Services;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    /*
     * Unforunately, no authentication scheme is specified yet.
     * So I have to use the indian company method to verify logging in and roles
     * till this point.
     */
    public class EventLocationController : Controller
    {
        readonly EventProContext _context;

        public EventLocationController(EventProContext context)
        {
            _context = context;
        }

        // GET: EventLocationController
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> Index()
        {
            ViewBag.PageTitle = "Events Locations";
            return View(await _context.EventLocations.ToListAsync());
        }

        // GET: EventLocationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EventLocationController/Create
        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        //  [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                var _city = collection["city"].FirstOrDefault();
                var _governorate = collection["governorate"].FirstOrDefault();
                var _country = collection["country"].FirstOrDefault();
                var locations = await _context.EventLocations.FirstOrDefaultAsync(l => l.Country.ToLower() == _country.ToLower()
                                                                && l.Governorate.ToLower() == _governorate.ToLower()
                                                                && l.City.ToLower() == _city.ToLower());
                if (locations == null)
                {
                    await _context.EventLocations.AddAsync(new EventLocation
                    {
                        City = _city,
                        Governorate = _governorate,
                        Country = _country
                    });
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: EventLocationController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            return View(await _context.EventLocations.FirstOrDefaultAsync(x => x.Id == id));
        }

        // POST: EventLocationController/Edit/5
        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection collection)
        {
            try
            {
                var old = await _context.EventLocations.FirstOrDefaultAsync(x => x.Id == id);

                var _city = collection["city"].FirstOrDefault();
                var _governorate = collection["governorate"].FirstOrDefault();
                var _country = collection["country"].FirstOrDefault();
                var locations = await _context.EventLocations.FirstOrDefaultAsync(l => l.Id != old.Id
                                                                && l.Country.ToLower() == _country.ToLower()
                                                                && l.Governorate.ToLower() == _governorate.ToLower()
                                                                && l.City.ToLower() == _city.ToLower());
                if (locations == null)
                {
                    old.City = _city;
                    old.Governorate = _governorate;
                    old.Country = _country;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        /*
        // GET: EventLocationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }
        */

        // POST: EventLocationController/Delete/5
        [HttpGet]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _context.EventLocations.Where(x => x.Id == id).ExecuteDeleteAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
