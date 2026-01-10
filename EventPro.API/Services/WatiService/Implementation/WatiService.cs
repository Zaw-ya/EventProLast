using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.API.Services.WatiService.Interface;
using EventPro.DAL.Models;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace EventPro.API.Services.WatiService.Implementation
{
    public class WatiService : IWatiService
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        public WatiService(IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
        }
        public async Task<string> SendGatekeeperCheckInMessage(GKEventHistory history, string filePathForSending)
        {
            var gkUser = await db.Users.Where(gk => gk.UserId == history.GK_Id).FirstOrDefaultAsync();
            var _event = await db.Events.Where(e => e.Id == history.Event_Id).FirstOrDefaultAsync();
            string gmapCode = "https://maps.app.goo.gl/" + _event.GmapCode;
            string location = $"https://www.google.com/maps/search/?api=1&query={history.latitude},{history.longitude}";
            string _phoneNumber_to = await db.AppSettings.Select(e => e.GateKeeperCheckNotificationsNumber).FirstOrDefaultAsync();
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + _phoneNumber_to);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"template_name\":\"checked_in5\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + filePathForSending + "\"},{\"name\":\"1\",\"value\":\"" + gkUser.FirstName + " " + gkUser.LastName + "\"},{\"name\":\"2\",\"value\":\"E000" + _event.Id.ToString() + "\"},{\"name\":\"3\",\"value\":\"" + _event.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + _event.EventVenue + "\"},{\"name\":\"5\",\"value\":\"" + gmapCode + "\"},{\"name\":\"6\",\"value\":\"" + location + "\"},{\"name\":\"7\",\"value\":\"" + DateTime.Now.ToString("tt h:mm yyyy/MM/dd", new CultureInfo("ar-EG")) + "\"},{\"name\":\"8\",\"value\":\"" + gkUser.Email.ToString() + "\"}]}", false);
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            return responseData.receivers[0].localmessageid;
        }


        public async Task<string> SendGatekeeperCheckoutMessage(GKEventHistory history)
        {
            var gkUser = await db.Users.Where(gk => gk.UserId == history.GK_Id).FirstOrDefaultAsync();
            var _event = await db.Events.Where(e => e.Id == history.Event_Id).FirstOrDefaultAsync();
            string gmapCode = "https://maps.app.goo.gl/" + _event.GmapCode;
            string location = $"https://www.google.com/maps/search/?api=1&query={history.latitude},{history.longitude}";
            string _phoneNumber_to = await db.AppSettings.Select(e => e.GateKeeperCheckNotificationsNumber).FirstOrDefaultAsync();
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + _phoneNumber_to);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"template_name\":\"checked_out6\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\" " + gkUser.FirstName + " " + gkUser.LastName + "\"},{\"name\":\"2\",\"value\":\"E000" + _event.Id.ToString() + "\"},{\"name\":\"3\",\"value\":\"" + _event.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + _event.EventVenue + "\"},{\"name\":\"5\",\"value\":\"" + gmapCode + "\"},{\"name\":\"6\",\"value\":\"" + location + "\"},{\"name\":\"7\",\"value\":\"" + DateTime.Now.ToString("tt h:mm yyyy/MM/dd", new CultureInfo("ar-EG")) + "\"},{\"name\":\"8\",\"value\":\"" + gkUser.Email.ToString() + "\"}]}", false);
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            return responseData.receivers[0].localmessageid;
        }

    }
}
