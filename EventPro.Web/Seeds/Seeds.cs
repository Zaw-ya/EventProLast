using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using System;
using System.Linq;

namespace EventPro.Web.Seeds
{
    public static class Seeding
    {
        public static void SeedDefaultLocation(IConfiguration config)
        {
            var db = new EventProContext(config);
            var otherEventLocation = db.EventLocations.FirstOrDefault(evl => evl.Governorate.ToLower() == "other" &&
                                                                                         evl.Country.ToLower() == "other" &&
                                                                                         evl.City.ToLower() == "other");
            if (otherEventLocation == null)
            {
                db.EventLocations.Add(new EventLocation { Governorate = "Other", Country = "Other", City = "Other" });
                db.SaveChanges();
            }
        }

        public static void SeedRoles(IConfiguration config)
        {
            var db = new EventProContext(config);

            var roles = new[]
            {
                "Administrator", // ID 1
                "Client",        // ID 2
                "GateKeeper",    // ID 3
                "Operator",      // ID 4
                "Agent",         // ID 5
                "Supervisor",    // ID 6
                "Accounting"     // ID 7
            };

            foreach (var roleName in roles)
            {
                var existingRole = db.Roles.FirstOrDefault(r => r.RoleName == roleName);
                if (existingRole == null)
                {
                    db.Roles.Add(new Roles { RoleName = roleName });
                }
            }

            db.SaveChanges();
        }

        public static void SeedAdminUser(IConfiguration config)
        {
            var db = new EventProContext(config);

            // Check if admin already exists
            var adminUser = db.Users.FirstOrDefault(u => u.UserName == "admin");
            if (adminUser == null)
            {
                // Get Administrator role
                var adminRole = db.Roles.FirstOrDefault(r => r.RoleName == "Administrator");
                if (adminRole == null)
                {
                    throw new Exception("Administrator role not found. Please run SeedRoles first.");
                }

                // Create admin user
                var admin = new Users
                {
                    UserName = "admin",
                    Password = "Admin@123", // Plain text - should be hashed in production
                    Email = "admin@eventpro.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Gender = "M",
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    Approved = true,
                    Role = adminRole.Id,
                    SendNotificationsOrEmails = true,
                    LoginAttempt = 0
                };

                db.Users.Add(admin);
                db.SaveChanges();
            }
        }

        public static void SeedAll(IConfiguration config)
        {
            SeedDefaultLocation(config);
            SeedRoles(config);
            SeedAdminUser(config);
        }
    }
}
