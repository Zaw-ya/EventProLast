using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventPro.DAL.Models;
using EventPro.Web.Filters;
using EventPro.Web.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    /*
     * Unforunately, no authentication scheme is specified yet.
     * So I have to use the indian company method to verify logging in and roles
     * till this point.
     */
    public class LogsController : Controller
    {
        private readonly EventProContext context;

        public LogsController(EventProContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            return View(null);
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> IndexData(DateTime from, DateTime to)
        {
            ViewBag.From = from.ToString("yyyy-MM-dd");
            ViewBag.To = to.ToString("yyyy-MM-dd");
            return View("Index", await context.SeriLog
                .Where(s => s.TimeStamp >= from && s.TimeStamp <= to)
                .ToListAsync());
        }


    }
}
