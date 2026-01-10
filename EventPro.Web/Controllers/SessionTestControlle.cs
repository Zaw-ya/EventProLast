using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventPro.Web.Controllers
{
    [Route("session-test")]
    public class SessionTestController : Controller
    {
        public IActionResult Index()
        {
            var count = HttpContext.Session.GetInt32("counter") ?? 0;
            count++;
            HttpContext.Session.SetInt32("counter", count);
            return Content($"Session counter: {count}");
        }
    }
}