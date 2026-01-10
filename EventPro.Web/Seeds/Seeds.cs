using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
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
    }
}
