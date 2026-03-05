using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using EventPro.DAL.Models;
using EventPro.Web.Common;
using EventPro.Web.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventPro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        private readonly ILogger<AccountController> _logger;
        private readonly IMemoryCache _cache;

        public AccountController(
            IConfiguration configuration, ILogger<AccountController> logger, IMemoryCache cache)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
            _logger = logger;
            _cache = cache;
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
            var sw = Stopwatch.StartNew();
            var cacheKey = $"login_{model.UserName}";

            var user = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

                return await db.Users
                    .AsNoTracking()
                    .Where(u => u.UserName == model.UserName)
                    .Select(u => new
                    {
                        u.UserId,
                        u.UserName,
                        u.Password,
                        u.FirstName,
                        u.LastName,
                        RoleName = u.RoleNavigation.RoleName,
                        u.IsActive
                    })
                    .FirstOrDefaultAsync();
            });

            sw.Stop();
            Console.WriteLine($"Login Query took {sw.ElapsedMilliseconds} ms");
            _logger.LogInformation($"Login Query took {sw.ElapsedMilliseconds} ms");

            //TempData["test"] = $"Ahmed From Function {user}";

            if (user != null)
            {
                var role = user?.RoleName;

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
                        return RedirectToAction(AppAction.AccessDenied, AppController.Login);
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
