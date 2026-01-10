using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.DataProtector;
using EventPro.DAL.Models;
using EventPro.Web.Services;
using System;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public class ResponsesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        private readonly UrlProtector _urlProtector;

        public ResponsesController(IConfiguration configuration, UrlProtector urlProtector)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
            _urlProtector = urlProtector;
        }

        public async Task<IActionResult> EventLocation(string id)
        {

            try
            {
                var idUnprotected = UrlEncryptionHelper.Decrypt(id);
                var guest = await db.Guest.FirstOrDefaultAsync(e => string.Concat("noButton", e.EventId.ToString(), e.GuestId.ToString()) == idUnprotected.ToString());
                var evnt = await db.Events.FirstOrDefaultAsync(e => e.Id == guest.EventId);
                ViewBag.evntTitle = evnt.EventTitle;
                ViewBag.Location = evnt.LinkGuestsLocationEmbedSrc;
                ViewBag.language = evnt.SendingConfiramtionMessagesLinksLanguage;

                guest.waMessageEventLocationForSendingToAll = "not null";
                guest.EventLocationSent = true;
                guest.EventLocationRead = true;
                guest.TextRead = true;
                await db.SaveChangesAsync();
                return View(guest);
            }
            catch (Exception ex)
            {
                return Redirect("https://EventPro.sa/en");
            }
        }

        public async Task<IActionResult> AcceptOrDecline(string id)
        {

            try
            {
                var idUnprotected = UrlEncryptionHelper.Decrypt(id);
                var guest = await db.Guest.FirstOrDefaultAsync(e => string.Concat("yesButton", e.EventId.ToString(), e.GuestId.ToString()) == idUnprotected.ToString());
                var evnt = await db.Events.FirstOrDefaultAsync(e => e.Id == guest.EventId);
                ViewBag.evntTitle = evnt.EventTitle;
                ViewBag.language = evnt.SendingConfiramtionMessagesLinksLanguage;
                ViewBag.Id = id;
                if (!string.IsNullOrEmpty(guest.Response) && (guest.Response.Equals("_yesText_Ar") || guest.Response.Equals("???? ??????")))
                {
                    return RedirectToAction("Confirm", new { id = id });
                }
                else if (!string.IsNullOrEmpty(guest.Response) && (guest.Response.Equals("Decline") || guest.Response.Equals("???????? ?? ??????")))
                {
                    return RedirectToAction("Decline", new { id = id });
                }
                else
                {
                    return View(guest);
                }

            }
            catch (Exception ex)
            {
                return Redirect("https://EventPro.sa/en");
            }

        }

        public async Task<IActionResult> Confirm(string id)
        {

            try
            {
                var idUnprotected = UrlEncryptionHelper.Decrypt(id);
                var guest = await db.Guest.FirstOrDefaultAsync(e => string.Concat("yesButton", e.EventId.ToString(), e.GuestId.ToString()) == idUnprotected.ToString());
                var evnt = await db.Events.FirstOrDefaultAsync(e => e.Id == guest.EventId);
                ViewBag.evntTitle = evnt.EventTitle;
                guest.TextRead = true;
                ViewBag.showPhoto = true;
                ViewBag.language = evnt.SendingConfiramtionMessagesLinksLanguage;
                ViewBag.cardMessage = evnt.LinkGuestsCardText;

                if (!string.IsNullOrEmpty(guest.Response) && (guest.Response.Equals("Decline") || guest.Response.Equals("???????? ?? ??????")))
                {
                    if (evnt.SendingConfiramtionMessagesLinksLanguage == "Arabic")
                    {
                        ViewBag.status = "???????? ?? ??????";
                    }
                    else if (evnt.SendingConfiramtionMessagesLinksLanguage == "English")
                    {
                        ViewBag.status = "Declined the invitation";
                    }

                    return View("DuplicateAnswer", guest);
                }
                else
                {
                    if (evnt.SendInvitation != true && guest.ImgRead != true)
                    {
                        ViewBag.photo = null;
                        ViewBag.showPhoto = false;
                    }
                    else
                    {
                        ViewBag.photo = evnt.Id + "/E00000" + evnt.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                        guest.ImgSentMsgId = "not null";
                        guest.ImgSent = true;
                        guest.ImgRead = true;
                    }
                    guest.Response = "???? ??????";
                    guest.WaresponseTime = DateTime.Now;

                    await db.SaveChangesAsync();
                    return View("Confirm", guest);
                }
            }
            catch (Exception ex)
            {
                return Redirect("https://EventPro.sa/en");
            }

        }

        public async Task<IActionResult> Decline(string id)
        {
            try
            {
                var idUnprotected = UrlEncryptionHelper.Decrypt(id);
                var guest = await db.Guest.FirstOrDefaultAsync(e => string.Concat("yesButton", e.EventId.ToString(), e.GuestId.ToString()) == idUnprotected.ToString());
                var evnt = await db.Events.FirstOrDefaultAsync(e => e.Id == guest.EventId);
                ViewBag.evntTitle = evnt.EventTitle;
                guest.TextRead = true;
                ViewBag.language = evnt.SendingConfiramtionMessagesLinksLanguage;

                if (!string.IsNullOrEmpty(guest.Response) && (guest.Response.Equals("_yesText_Ar") || guest.Response.Equals("???? ??????")))
                {
                    if (evnt.SendingConfiramtionMessagesLinksLanguage == "Arabic")
                    {
                        ViewBag.status = "???? ??????";
                    }
                    else if (evnt.SendingConfiramtionMessagesLinksLanguage == "English")
                    {
                        ViewBag.status = "Accepted the invitation";
                    }

                    return View("DuplicateAnswer", guest);
                }
                else
                {
                    guest.Response = "???????? ?? ??????";
                    guest.WaresponseTime = DateTime.Now;
                    await db.SaveChangesAsync();
                    return View("Decline", guest);
                }
            }
            catch (Exception ex)
            {
                return Redirect("https://EventPro.sa/en");
            }

        }
    }
}
