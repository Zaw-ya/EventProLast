using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPro.DAL.Models;
using EventPro.Web.Common;
using EventPro.Web.Filters;
using EventPro.Web.Services;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public class CityController : Controller
    {
        readonly EventProContext _context;
        public CityController(EventProContext context)
        {
            _context = context;
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> Index()
        {
            ViewBag.PageTitle = "Cities";
            return View(await _context.City.Include(x => x.Country).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.countries =
               new SelectList((await _context.Country.ToListAsync())
                     .Select(e => new { CountryId = e.Id, CountryName = e.CountryName }), "CountryId", "CountryName");
            return View();
        }

        [HttpPost]
        [AuthorizeRoles("Administrator")]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(City city)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.countries =
              new SelectList((await _context.Country.ToListAsync())
                    .Select(e => new { CountryId = e.Id, CountryName = e.CountryName }), "CountryId", "CountryName");
                return View(city);
            }
            try
            {
                bool exist = _context.City.Any(c => c.CountryId == city.CountryId
                                                 && c.CityName.ToLower() == city.CityName.Trim().ToLower());
                if (exist)
                {
                    ViewBag.countries =
              new SelectList((await _context.Country.ToListAsync())
                    .Select(e => new { CountryId = e.Id, CountryName = e.CountryName }), "CountryId", "CountryName");
                    ModelState.AddModelError(string.Empty, "Error! This city already exist");
                    return View(city);
                }
                _context.Add(city);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(city);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.countries =
             new SelectList((await _context.Country.ToListAsync())
                   .Select(e => new { CountryId = e.Id, CountryName = e.CountryName }), "CountryId", "CountryName");

            return View(await _context.City.FirstOrDefaultAsync(x => x.Id == id));
        }

        [HttpPost]
        [AuthorizeRoles("Administrator")]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(City city)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.countries =
           new SelectList((await _context.Country.ToListAsync())
                 .Select(e => new { CountryId = e.Id, CountryName = e.CountryName }), "CountryId", "CountryName");
                return View(city);
            }
            try
            {
                bool exist = _context.City.Any(c => c.Id != city.Id
                                                  && c.CountryId == city.CountryId
                                                  && c.CityName.ToLower() == city.CityName.Trim().ToLower());
                if (exist)
                {
                    ModelState.AddModelError(string.Empty, "Error! This country already exist");
                    ViewBag.countries =
           new SelectList((await _context.Country.ToListAsync())
                 .Select(e => new { CountryId = e.Id, CountryName = e.CountryName }), "CountryId", "CountryName");
                    return View(city);
                }
                _context.Entry(city).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(city);
            }
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var city = _context.City.Find(id);
                if (city == null)
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return Json(new { success = false, responseText = "This country does not exist" });
                }
                if (_context.Users.Where(c => c.CityId == city.Id).Any())
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return Json(new { success = false, responseText = $"can not delete {city.CityName} because it has users" });
                }
                if (_context.Events.Where(c => c.CityId == city.Id).Any())
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return Json(new { success = false, responseText = $"can not delete {city.CityName} because it has Events" });
                }
                _context.Remove(city);
                var affectedRows = await _context.SaveChangesAsync();

                if (affectedRows > 0)
                    return Ok();
            }
            catch
            {
                return BadRequest();
            }
            return BadRequest();
        }
    }
}
