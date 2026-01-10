using Microsoft.AspNetCore.Mvc;

namespace EventPro.Web.Controllers
{
    public class PrivacyPolicyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
