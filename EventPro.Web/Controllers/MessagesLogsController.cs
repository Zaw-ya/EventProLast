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
