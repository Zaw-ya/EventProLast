using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Dto;
using EventPro.DAL.Models;
using EventPro.Web.Filters;
using EventPro.Web.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private void SetBreadcrum(string title, string link)
        {
            ViewBag.PageTitle = title;
            ViewBag.BackLink = link;
        }
    }
}
