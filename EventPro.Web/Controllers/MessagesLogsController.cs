using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Dto;
using EventPro.DAL.Models;
using EventPro.Web.Filters;
using EventPro.Web.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;

namespace EventPro.Web.Controllers
{
    public class MessagesLogsController : Controller
    {
        private readonly EventProContext _context;
        private readonly IWhatsappSendingProviderService _whatsappSendingProvider;
        public MessagesLogsController(EventProContext db,IWhatsappSendingProviderService sendingProviderService)
        {
            _context = db;
            _whatsappSendingProvider = sendingProviderService;
        }

        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> Index()
        {
            ViewBag.choosenSendingWhatsappProfile = new SelectList(await _context.TwilioProfileSettings
                    .AsNoTracking()
                    .ToListAsync(),
                     "Name", "Name");

            ViewBag.Icon = "nav-icon fa fa-scroll";
            SetBreadcrum("messages logs", "/");
            return View();
        }

        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> GetGuestMessages(string phoneNumber, string profileName)
        {
            List<MessageLog> messages = new List<MessageLog>();

            try
            {
                var whatsAppProvider = _whatsappSendingProvider.SelectTwilioSendingProvider();
                messages = await whatsAppProvider.GetGuestMessagesAsync(phoneNumber, profileName);
            }
            catch
            {
                return Json(new { success = false, messages = messages });
            }

            return Json(new { success = true, messages = messages });
        }
/*4. Media proxy endpoint (MessagesLogsController.cs)
GET /MessagesLogs/Media?messageSid=XX&mediaSid=YY&profileName=ZZ
What it does:

Looks up the Twilio profile credentials (AccountSid + AuthToken) from the database
Constructs the Twilio API media URL
Makes an HTTP request to Twilio with Basic authentication
Streams the actual media content (image/video/audio bytes) back to the browser with the correct content type
Why: Twilio media URLs require authentication. The browser cannot access them directly because it doesn't have the Twilio credentials. 
This proxy acts as a middleman — the browser requests from your server,
your server fetches from Twilio with auth, and passes the content through.

*/
        [AuthorizeRoles("Administrator", "Supervisor")]
        public async Task<IActionResult> Media(string messageSid, string mediaSid, string profileName)
        {
            try
            {
                var twilioProfile = await _context.TwilioProfileSettings
                    .Where(e => e.Name == profileName)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (twilioProfile == null)
                    return NotFound();

                var apiUrl = $"https://api.twilio.com/2010-04-01/Accounts/{twilioProfile.AccountSid}/Messages/{messageSid}/Media/{mediaSid}";

                using var httpClient = new HttpClient();
                var credentials = Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($"{twilioProfile.AccountSid}:{twilioProfile.AuthToken}"));
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);

                var response = await httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                var stream = await response.Content.ReadAsStreamAsync();

                return File(stream, contentType);
            }
            catch
            {
                return NotFound();
            }
        }

        private void SetBreadcrum(string title, string link)
        {
            ViewBag.PageTitle = title;
            ViewBag.BackLink = link;
        }
    }
}
