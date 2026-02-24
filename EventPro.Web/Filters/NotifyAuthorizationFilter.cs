using Hangfire.Dashboard;
using EventPro.Web.Common;
using System.Security.Claims;

namespace EventPro.Web.Filters
{
    public class NotifyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var operatorRole = httpContext.User.FindFirst("Operator")?.Value;

            if (!string.IsNullOrEmpty(userRole) && userRole == "Administrator")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
