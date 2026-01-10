using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EventPro.Web.Common;
using System.Security.Claims;

namespace EventPro.Web.Services
{
    public class AccessService
    {

        public static IActionResult AccessVerification(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var operatorRole = httpContext.User.FindFirst("Operator")?.Value;

            if (string.IsNullOrEmpty(userRole) || (userRole != "Administrator" ))
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            if (operatorRole == "Operator")
            {
                return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }
         

            return null;
        }

        public static IActionResult AllowAccessForAdministratorAndOperatorOnly(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Administrator" && userRole != "Operator" && userRole != "Supervisor")
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            return null;
        }

        public static IActionResult AllowAccessForAdministratorAndSupervisorOnly(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Administrator"  && userRole != "Supervisor")
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            return null;
        }
        public static IActionResult AllowAccessForAdministratorAndAgentOnly(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Administrator" && userRole != "Agent")
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            return null;
        }
        public static IActionResult AllowAccessForAdministratorAndOperatorOrAndAgent(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Administrator" && userRole != "Operator" && userRole != "Agent" && userRole != "Supervisor")
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            return null;
        }

        public static IActionResult AllowAccessForAdministratorAndOperatorOrAndAgentOrAccounting(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Administrator" && userRole != "Operator" && userRole != "Agent" && userRole != "Supervisor" && userRole != "Accounting")
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            return null;
        }

        public static IActionResult AllowAccessForAdministratorAndAgentorAndAccounting(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Administrator" && userRole != "Agent" && userRole != "Accounting")
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            return null;
        }
        public static IActionResult AllowAccessForAdministratorAndOperatOrAccountingOrSupervisor(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Administrator" && userRole != "Operator" && userRole != "Supervisor" && userRole != "Accounting")
            {
                return new RedirectToActionResult(AppAction.Index, AppController.Login, new { });
            }

            return null;
        }


    }
}
