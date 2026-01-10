using Microsoft.AspNetCore.Mvc;

namespace EventPro.Web.Controllers
{
    public class TermsandConditionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
