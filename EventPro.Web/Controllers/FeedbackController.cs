using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using EventPro.Services.WatiService.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly EventProContext db;
        private readonly IWatiService _watiService;
        private readonly IWhatsappSendingProviderService _whatsappSendingProviderService;
        public FeedbackController(
            IConfiguration configuration, IWatiService watiService,
            IWhatsappSendingProviderService whatsappSendingProviderService)
        {
            db = new EventProContext(configuration);
            _watiService = watiService;
            _whatsappSendingProviderService = whatsappSendingProviderService;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            var guest = await db.Guest.FirstOrDefaultAsync(e => e.ConguratulationMsgLinkId == id);
            if (guest == null)
            {
                return NotFound();
            }
            var evnt = await db.Events.FirstOrDefaultAsync(e => e.Id == guest.EventId);
            ViewBag.evntTitle = evnt.EventTitle;
            return View(guest);
        }

        [HttpGet]
        public async Task<IActionResult> sendMessage(int id, string message)
        {
            var messageId = string.Empty;
            var guest = await db.Guest
                .FirstOrDefaultAsync(e => e.GuestId == id);

            var evnt = await db.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == guest.EventId);

            var whatsappProvider = await _whatsappSendingProviderService
                .SelectConfiguredSendingProviderAsync(evnt);
            var guests = new List<Guest>() { guest };

            try
            {
                if (guest.ConguratulationMsgCount < 1)
                {
                    return BadRequest("sent before");
                }

                if (evnt.WhatsappProviderName == "Default")
                {
                    if (db.AppSettings.FirstOrDefault().WhatsappServiceProvider == "Wati")
                    {
                        messageId = await _watiService.SendCongratulationMessageToPrideTemplate(guest, message);
                        if (messageId == null || messageId.Contains("Error"))
                        {
                            return BadRequest();
                        }
                    }
                    else if (db.AppSettings.FirstOrDefault().WhatsappServiceProvider == "Twilio")
                    {
                        await whatsappProvider.GetCongratulatioinMessageTemplates()
                             .SendCongratulationMessageToOwner(guests, evnt, message);
                    }
                }
                else if (evnt.WhatsappProviderName == "Wati")
                {
                    messageId = await _watiService.SendCongratulationMessageToPrideTemplate(guest, message);
                    if (messageId == null || messageId.Contains("Error"))
                    {
                        return BadRequest();
                    }
                }
                else if (evnt.WhatsappProviderName == "Twilio")
                {
                    await whatsappProvider.GetCongratulatioinMessageTemplates()
                         .SendCongratulationMessageToOwner(guests, evnt, message);
                }

                return Ok();
            }

            catch
            {
                return BadRequest();
            }

        }
    }
}
