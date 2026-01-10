using Microsoft.AspNetCore.Http;
using System;

namespace EventPro.Web.Common
{
    public class AppSession
    {
        public static string GetSession(HttpContext context, string key)
        {
            if (context.Session.GetString(key) != null)
                return context.Session.GetString(key);
            else
                return string.Empty;
        }

        public static void SetSession(HttpContext context, string key, string value)
        {
            context.Session.SetString(key, value);
        }

        public static int GetCurrentUserId(HttpContext context)
        {
            if (context.Session.GetString(SesionConstant.UserId) != null)
                return Convert.ToInt32(context.Session.GetString(SesionConstant.UserId));
            else
                return 0;
        }

        public static string GetCurrentUserRoleforAgent(HttpContext context)
        {
            if (context.Session.GetString(SesionConstant.Role) != null)
                return context.Session.GetString(SesionConstant.Role);
            else
                return "UnAuth";
        }

        public static string GetCurrentUserRole(HttpContext context)
        {
            if (context.Session.GetString(SesionConstant.Role) != null)
            {
                if (context.Session.GetString(SesionConstant.Role) == "Agent")
                {
                    return "Administrator";

                }
                else
                {
                    return context.Session.GetString(SesionConstant.Role);

                }
            }
            else
                return "UnAuth";
        }
    }


    public class AppAction
    {
        public const string AdminDashboard = "index";
        public const string Index = "index";
        public const string Users = "users";
        public const string Events = "events";
        public const string AccessDenied = "AccessDenied";

    }
    public class AppController
    {
        public const string Admin = "admin";
        public const string User = "user";
        public const string Login = "account";

    }
}

