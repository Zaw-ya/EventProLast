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
    public class CountryController : Controller
    {
        readonly EventProContext _context;
        public CountryController(EventProContext context)
        {
            _context = context;
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> Index()
        {
            ViewBag.PageTitle = "Countries";
            return View(await _context.Country.ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [AuthorizeRoles("Administrator")]
        //  [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Country country)
        {
            if (!ModelState.IsValid)
            {
                return View(country);
            }
            try
            {
                bool exist = _context.Country.Any(c => c.CountryName.ToLower() == country.CountryName.Trim().ToLower());
                if (exist)
                {
                    ModelState.AddModelError(string.Empty, "Error! This country already exist");
                    return View(country);
                }
                _context.Add(country);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(country);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            return View(await _context.Country.FirstOrDefaultAsync(x => x.Id == id));
        }

        [HttpPost]
        [AuthorizeRoles("Administrator")]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Country country)
        {
            if (!ModelState.IsValid)
            {
                return View(country);
            }
            try
            {
                bool exist = _context.Country.Any(c => c.Id != country.Id
                                                  && c.CountryName.ToLower() == country.CountryName.Trim().ToLower());
                if (exist)
                {
                    ModelState.AddModelError(string.Empty, "Error! This country already exist");
                    return View(country);
                }
                _context.Entry(country).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(country);
            }
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var country = _context.Country.Find(id);
                if (country == null)
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return Json(new { success = false, responseText = "This country does not exist" });
                }
                if (_context.City.Where(c => c.CountryId == country.Id).Any())
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return Json(new { success = false, responseText = $"can not delete {country.CountryName} because it has cities" });
                }
                _context.Remove(country);
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
