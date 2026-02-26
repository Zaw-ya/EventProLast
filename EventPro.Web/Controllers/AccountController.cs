using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using EventPro.Web.Common;
using EventPro.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        public AccountController(
            IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            //  await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginModel model)
        {
            List<Roles> roles = await db.Roles.ToListAsync();

            var user = await db.Users
                .Where(p => p.UserName == model.UserName && p.Password == model.Password)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            TempData["test"] = $"Ahmed From Function {user}";


            if (user != null)
            {
                var role = roles.Where(p => p.Id == user.Role && user.IsActive == true)
                                .Select(p => p.RoleName)
                                .FirstOrDefault();

                if (role == null)
                {
                    TempData["message"] = "Access Denied";
                    return View();
                }

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, role)
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                switch (role)
                {
                    case "Administrator":
                    case "Operator":
                    case "Supervisor":
                        return RedirectToAction(AppAction.AdminDashboard, AppController.Admin);
                    case "Accounting":
                        return RedirectToAction(AppAction.AdminDashboard, AppController.Admin);
                    case "Agent":
                        return RedirectToAction(AppAction.AdminDashboard, AppController.Admin);
                    case "Client":
                        return RedirectToAction(AppAction.Index, AppController.User);
                    default:
                        return RedirectToAction(AppAction.AccessDenied, AppController.Login);ء
                }
            }
            else
            {
                TempData["message"] = "Incorrect email or password";
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(AppAction.Index, AppController.Login);
        }
    }
}
