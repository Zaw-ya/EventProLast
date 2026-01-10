using Microsoft.AspNetCore.Mvc;

namespace EventPro.API.Controllers
{
    public class GuestResponseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Post()
        {
            return View();
        }
    }
}
