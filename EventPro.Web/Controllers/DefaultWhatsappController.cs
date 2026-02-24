using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using EventPro.Business.Storage.Implementation;
using EventPro.Business.Storage.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Filters;
using EventPro.Web.Services.DefaultWhatsappService.Interface;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Serilog;

namespace EventPro.Web.Controllers
{
    //[ServiceFilter(typeof(ForwardToPrimaryFilter))]
    public class DefaultWhatsappController : Controller
    {
        private IUnitOFWorkDefaultWhatsappService _UnitOFWorkDefaultWhatsappService;
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        private readonly IBlobStorage _blobStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly bool _isPrimary;
        private readonly string _primaryVmUrl;
        private readonly System.Net.Http.HttpClient _httpClient;
        private readonly ICloudinaryService _cloudinaryService;

        public DefaultWhatsappController(IConfiguration configuration,
               IUnitOFWorkDefaultWhatsappService unitOFWorkDefaultWhatsappService,
               IBlobStorage blobStorage,
               ICloudinaryService cloudinaryService)
        {
            _UnitOFWorkDefaultWhatsappService = unitOFWorkDefaultWhatsappService;
            _configuration = configuration;
            db = new EventProContext(configuration);
            _blobStorage = blobStorage;
            _cloudinaryService = cloudinaryService;
        }
        public async Task<IActionResult> sendMessage(int id)
        {

            var guest = await db.Guest.FirstOrDefaultAsync(e => e.GuestId == id);
            var evnt = await db.Events.Where(e => e.Id == guest.EventId)
                        .Include(e => e.City.Country)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string imagePath = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value + "/" +
                               guest.EventId + "/" + "E00000" + guest.EventId + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";

            if (string.IsNullOrEmpty(evnt.FailedGuestsMessag) ||
                string.IsNullOrEmpty(evnt.FailedGuestsCardText) ||
                string.IsNullOrEmpty(evnt.FailedGuestsLocationEmbedSrc))
            {
                return Json(new { success = false });
            }

            if (!await _blobStorage.FileExistsAsync(environment + imagePath))
            {
                return Json(new { success = false });
            }

            guest = setDefaultSendingStatues(guest);

            try
            {
                var number = ChooseSendingNumber(guest, evnt);

                lock (number)
                {
                    number.SendMessage(evnt, guest);
                }

                guest.MessageId = "notnull";
                guest.TextSent = true;
                guest.TextFailed = null;
                guest.Response = "Message Processed Successfully";
                guest.WasentOn = DateTime.Now.ToString();
            }
            catch (Exception ex)
            {
                Log.Error($"error exured in DefaultWhatsappServcie :{ex.Message},inner:{ex.InnerException}");
                guest.MessageId = "notnull";
                guest.TextFailed = true;
                guest.TextSent = false;
            }

            await db.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> sendMessagesToAll(int id)
        {
            //var access = AccessService.AllowAccessForAdministratorAndOperatorOnly(this.HttpContext);
            //if (access != null) return access;

            var guests = await db.Guest.Where(p => p.EventId == id && (p.MessageId == null))
                .Take(50)
                .ToListAsync();

            var evnt = await db.Events.Where(e => e.Id == id)
                        .Include(e => e.City.Country)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(evnt.FailedGuestsMessag) ||
                string.IsNullOrEmpty(evnt.FailedGuestsCardText) ||
                string.IsNullOrEmpty(evnt.FailedGuestsLocationEmbedSrc))
            {
                return Json(new { success = false });
            }

            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            foreach (var guest in guests)
            {

                string imagePath = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value + @"/" +
                                  guest.EventId + @"/" + "E00000" + guest.EventId + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";

                if (!await _blobStorage.FileExistsAsync(environment + imagePath))
                {
                    //db.Guest.UpdateRange(guests);
                    //await db.SaveChangesAsync();
                    return Json(new { success = false });
                }

                Thread.Sleep(3000);
                setDefaultSendingStatues(guest);

                try
                {
                    var number = ChooseSendingNumber(guest, evnt);

                    lock (number)
                    {
                        number.SendMessage(evnt, guest);
                    }

                    guest.MessageId = "notnull";
                    guest.TextSent = true;
                    guest.TextFailed = null;
                    guest.Response = "Message Processed Successfully";
                    guest.WasentOn = DateTime.Now.ToString();
                }
                catch (Exception ex)
                {
                    Log.Error($"error exured in DefaultWhatsappServcie :{ex.Message},inner:{ex.InnerException}");
                    guest.MessageId = "notnull";
                    guest.TextFailed = true;
                    guest.TextSent = false;
                }
                await db.SaveChangesAsync();
            }

            //db.Guest.UpdateRange(guests);
            //await db.SaveChangesAsync();
            return Json(new { success = true });
        }


        public async Task<IActionResult> sendRminderMessage(int id)
        {
            //var access = AccessService.AllowAccessForAdministratorAndOperatorOnly(this.HttpContext);
            //if (access != null) return access;

            var guest = await db.Guest.FirstOrDefaultAsync(e => e.GuestId == id);
            var evnt = await db.Events.Where(e => e.Id == guest.EventId)
                        .Include(e => e.City.Country)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(evnt.FailedGuestsReminderMessage))
            {
                return Json(new { success = false });
            }
            try
            {
                var number = ChooseSendingNumber(guest, evnt);

                lock (number)
                {
                    number.SendReminderMessage(evnt, guest);
                }

                guest.ReminderMessageId = "notnull";
                guest.ReminderMessageSent = true;
                guest.ReminderMessageFailed = null;
                guest.ReminderMessageDelivered = false;
                guest.ReminderMessageWatiId = null;
                guest.ReminderMessageRead = false;
            }
            catch (Exception ex)
            {
                Log.Error($"error exured in DefaultWhatsappServcie :{ex.Message},inner:{ex.InnerException}");
                guest.ReminderMessageId = "notnull";
                guest.ReminderMessageFailed = true;
                guest.ReminderMessageSent = false;
            }

            await db.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> sendCongratularionMessage(int id)
        {
            //var access = AccessService.AllowAccessForAdministratorAndOperatorOnly(this.HttpContext);
            //if (access != null) return access;

            var guest = await db.Guest.FirstOrDefaultAsync(e => e.GuestId == id);
            var evnt = await db.Events.Where(e => e.Id == guest.EventId)
                        .Include(e => e.City.Country)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            var conguratulationId = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(evnt.FailedGuestsCongratulationMsg))
            {
                return Json(new { success = false });
            }

            try
            {
                var number = ChooseSendingNumber(guest, evnt);

                lock (number)
                {
                    number.SendCongratulationMessage(evnt, guest, conguratulationId);
                }

                guest.ConguratulationMsgId = "notnull";
                guest.ConguratulationMsgSent = true;
                guest.ConguratulationMsgFailed = null;
                guest.ConguratulationMsgRead = false;
                guest.ConguratulationMsgDelivered = false;
                guest.WatiConguratulationMsgId = null;
                guest.ConguratulationMsgCount = 1;
                guest.ConguratulationMsgLinkId = conguratulationId;
            }
            catch (Exception ex)
            {
                Log.Error($"error exured in DefaultWhatsappServcie :{ex.Message},inner:{ex.InnerException}");
                guest.ConguratulationMsgId = "notnull";
                guest.ConguratulationMsgFailed = true;
                guest.ConguratulationMsgSent = false;
            }

            await db.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> sendImagesToAll(int id)
        {
            //var access = AccessService.AllowAccessForAdministratorAndOperatorOnly(this.HttpContext);
            //if (access != null) return access;

            var guests = await db.Guest.Where(p => p.EventId == id && (p.ImgSentMsgId == null))
                .Take(50)
                .ToListAsync();

            var evnt = await db.Events.Where(e => e.Id == id)
                         .Include(e => e.City.Country)
                         .AsNoTracking()
                         .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(evnt.FailedGuestsCardText))
            {
                return Json(new { success = false });
            }
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            foreach (var guest in guests)
            {
                string imagePath = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value + @"/" +
                                  guest.EventId + @"/" + "E00000" + guest.EventId + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";

                if (!await _blobStorage.FileExistsAsync(environment + imagePath))
                {
                    //db.Guest.UpdateRange(guests);
                    //await db.SaveChangesAsync();
                    return Json(new { success = false });
                }

                Thread.Sleep(3000);
                try
                {

                    var number = ChooseSendingNumber(guest, evnt);

                    lock (number)
                    {
                        number.SendImage(evnt, guest);
                    }

                    guest.ImgSentMsgId = "notnull";
                    guest.ImgSent = true;
                    guest.ImgFailed = null;
                    guest.ImgDelivered = false;
                    guest.ImgRead = false;
                    guest.whatsappMessageImgId = null;
                }
                catch (Exception ex)
                {
                    Log.Error($"error exured in DefaultWhatsappServcie :{ex.Message},inner:{ex.InnerException}");
                    guest.ImgSentMsgId = "notnull";
                    guest.ImgFailed = true;
                    guest.ImgSent = false;
                }
                await db.SaveChangesAsync();
            }

            //db.Guest.UpdateRange(guests);
            //await db.SaveChangesAsync();
            return Json(new { success = true });
        }

        /*[HttpGet]*/ // أو [HttpPost] حسب استخدامك
        public async Task<IActionResult> sendImage(int id, int type)
        {
            Log.Information("Starting sendImage request - GuestId: {GuestId}, Type: {Type}", id, type);

            // 1. جيب الـ guest
            var guest = await db.Guest
                .FirstOrDefaultAsync(e => e.GuestId == id);

            if (guest == null)
            {
                Log.Warning("Guest not found - GuestId: {GuestId}", id);
                return Json(new { success = false, message = "Guest not found" });
            }

            Log.Debug("Guest found - GuestId: {GuestId}, EventId: {EventId}, Mobile: {Mobile}",
                guest.GuestId, guest.EventId, guest.PrimaryContactNo);

            // 2. جيب الـ event
            var evnt = await db.Events
                .Where(e => e.Id == guest.EventId)
                .Include(e => e.City.Country)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (evnt == null)
            {
                Log.Warning("Event not found - EventId: {EventId} for GuestId: {GuestId}", guest.EventId, id);
                return Json(new { success = false, message = "Event not found" });
            }

            Log.Information("Event loaded - EventId: {EventId}, Title: {Title}, Country: {Country}",
                evnt.Id, evnt.EventTitle, evnt.City?.Country?.CountryName);

            // 3. تحقق من النص الأساسي
            if (string.IsNullOrEmpty(evnt.FailedGuestsCardText))
            {
                Log.Warning("Card text is not configured - EventId: {EventId}", evnt.Id);
                return Json(new { success = false, message = "Card text is not configured" });
            }

            // 4. بناء publicId وتحقق من الصورة في Cloudinary
            string publicId = $"cards/{evnt.Id}/E00000{evnt.Id}_{guest.GuestId}_{guest.NoOfMembers}";
            Log.Information("Checking Cloudinary for card image - PublicId: {PublicId}", publicId);

            string imageUrl;
            try
            {
                imageUrl = await _cloudinaryService.GetLatestVersionUrlAsync(publicId, "image");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get image URL from Cloudinary - PublicId: {PublicId}", publicId);
                return Json(new { success = false, message = "Failed to retrieve image from storage" });
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                Log.Warning("Card image not found in Cloudinary - PublicId: {PublicId}, GuestId: {GuestId}", publicId, guest.GuestId);
                return Json(new { success = false, message = "Card image not found in storage" });
            }

            Log.Information("Card image found in Cloudinary - URL: {ImageUrl}, PublicId: {PublicId}", imageUrl, publicId);

            // 5. اختيار خدمة WhatsApp حسب البلد
            var whatsappService = ChooseSendingNumber(guest, evnt);
            Log.Debug("Selected WhatsApp service for country: {Country}", evnt.City?.Country?.CountryName);

            // 6. محاولة إرسال الصورة
            try
            {
                Log.Information("Attempting to send card image via WhatsApp fallback - GuestId: {GuestId}, EventId: {EventId}",
                    guest.GuestId, evnt.Id);

                //lock (whatsappService)
                //{
                    await whatsappService.SendImage(evnt, guest);
                //}

                Log.Information("Card image sent successfully via WhatsApp - GuestId: {GuestId}", guest.GuestId);

                // 7. تحديث حالة الـ guest
                guest.ImgSentMsgId = "notnull";
                guest.ImgSent = true;
                guest.ImgFailed = null;
                guest.ImgDelivered = false;
                guest.ImgRead = false;
                guest.whatsappMessageImgId = null;
                guest.WasentOn = DateTime.Now.ToString();

                // ملاحظة عن type == 1 (نحتفظ بيه مؤقتًا لحد ما تؤكد إزاي نستخدمه)
                if (type == 1)
                {
                    Log.Warning("type=1 detected in sendImage - Updating text status (legacy behavior?) - GuestId: {GuestId}", guest.GuestId);
                    guest.Response = "تم الإرسال بنجاح";
                    guest.MessageId = "notnull";
                    guest.TextSent = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send card image via WhatsApp fallback - GuestId: {GuestId}, EventId: {EventId}",
                    guest.GuestId, evnt.Id);

                guest.ImgSentMsgId = "notnull";
                guest.ImgFailed = true;
                guest.ImgSent = false;
            }

            // 8. حفظ التغييرات
            try
            {
                await db.SaveChangesAsync();
                Log.Information("Guest state updated and saved - GuestId: {GuestId}, ImgSent: {ImgSent}, ImgFailed: {ImgFailed}",
                    guest.GuestId, guest.ImgSent, guest.ImgFailed);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save guest changes after image send attempt - GuestId: {GuestId}", guest.GuestId);
            }

            return Json(new { success = guest.ImgSent });
        }

        public async Task<IActionResult> AcceptOrDecline(string id)
        {
            try
            {
                var guest = await db.Guest.FirstOrDefaultAsync(e => string.Concat(e.GuestId.ToString(), e.EventId.ToString()) == id.ToString());
                var evnt = await db.Events.FirstOrDefaultAsync(e => e.Id == guest.EventId);
                ViewBag.evntTitle = evnt.EventTitle;
                ViewBag.language = evnt.FailedSendingConfiramtionMessagesLinksLanguage;
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
                    return View("AcceptOrDecline", guest);
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
                var guest = await db.Guest.FirstOrDefaultAsync(e => string.Concat(e.GuestId.ToString(), e.EventId.ToString()) == id.ToString());
                var evnt = await db.Events.FirstOrDefaultAsync(e => e.Id == guest.EventId);
                ViewBag.evntTitle = evnt.EventTitle;
                guest.TextRead = true;
                ViewBag.showPhoto = true;
                ViewBag.language = evnt.FailedSendingConfiramtionMessagesLinksLanguage;
                ViewBag.cardMessage = evnt.FailedGuestsCardText;

                if (!string.IsNullOrEmpty(guest.Response) && (guest.Response.Equals("Decline") || guest.Response.Equals("???????? ?? ??????")))
                {
                    if (evnt.FailedSendingConfiramtionMessagesLinksLanguage == "Arabic")
                    {
                        ViewBag.status = "???????? ?? ??????";
                    }
                    else if (evnt.FailedSendingConfiramtionMessagesLinksLanguage == "English")
                    {
                        ViewBag.status = "Declined the invitation";
                    }

                    return View("DefaultWhatsappDuplicateAnswer", guest);
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
                    return View("DefaultWhatsappConfirm", guest);
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
                var guest = await db.Guest.FirstOrDefaultAsync(e => string.Concat(e.GuestId.ToString(), e.EventId.ToString()) == id.ToString());
                var evnt = await db.Events.FirstOrDefaultAsync(e => e.Id == guest.EventId);
                ViewBag.evntTitle = evnt.EventTitle;
                guest.TextRead = true;
                ViewBag.language = evnt.FailedSendingConfiramtionMessagesLinksLanguage;

                if (!string.IsNullOrEmpty(guest.Response) && (guest.Response.Equals("_yesText_Ar") || guest.Response.Equals("???? ??????")))
                {
                    if (evnt.FailedSendingConfiramtionMessagesLinksLanguage == "Arabic")
                    {
                        ViewBag.status = "???? ??????";
                    }
                    else if (evnt.FailedSendingConfiramtionMessagesLinksLanguage == "English")
                    {
                        ViewBag.status = "Accepted the invitation";
                    }

                    return View("DefaultWhatsappDuplicateAnswer", guest);
                }
                else
                {
                    guest.Response = "???????? ?? ??????";
                    guest.WaresponseTime = DateTime.Now;
                    await db.SaveChangesAsync();
                    return View("DefaultWhatsappDecline", guest);
                }
            }
            catch (Exception ex)
            {
                return Redirect("https://EventPro.sa/en");
            }

        }

        public async Task<IActionResult> EventLocation(string id)
        {
            try
            {
                var guest = await db.Guest.FirstOrDefaultAsync(e => string.Concat(e.GuestId.ToString(), e.EventId.ToString()) == id.ToString());
                var evnt = await db.Events.FirstOrDefaultAsync(e => e.Id == guest.EventId);
                ViewBag.evntTitle = evnt.EventTitle;
                ViewBag.Location = evnt.FailedGuestsLocationEmbedSrc;
                ViewBag.language = evnt.FailedSendingConfiramtionMessagesLinksLanguage;

                guest.waMessageEventLocationForSendingToAll = "not null";
                guest.EventLocationSent = true;
                guest.EventLocationRead = true;
                guest.TextRead = true;
                await db.SaveChangesAsync();
                return View("DefaultWhatsappEventLocation", guest);
            }
            catch (Exception ex)
            {
                return Redirect("https://EventPro.sa/en");
            }
        }

        private IDefaultWhatsappService ChooseSendingNumber(Guest guest, Events evnt)
        {
            if (evnt.City.Country.CountryName.Contains("Egypt"))
            {
                return _UnitOFWorkDefaultWhatsappService.defaultWhatsappServcieEgypt;
            }
            else if (evnt.City.Country.CountryName.Contains("Egypt"))
            {
                return _UnitOFWorkDefaultWhatsappService.defaultWhatsappServcieEgypt;
            }
            else
            {
                return _UnitOFWorkDefaultWhatsappService.defaultWhatsappServcieEgypt;
            }
        }

        private Guest setDefaultSendingStatues(Guest guest)
        {
            guest.TextDelivered = false;
            guest.ImgSentMsgId = null;
            guest.ImgRead = false;
            guest.ImgDelivered = false;
            guest.ImgSent = false;
            guest.ImgFailed = null;
            guest.TextRead = false;
            guest.TextFailed = null;
            guest.waMessageEventLocationForSendingToAll = null;
            guest.EventLocationSent = false;
            guest.EventLocationRead = false;
            guest.EventLocationFailed = null;
            guest.ReminderMessageFailed = null;
            guest.ConguratulationMsgFailed = null;
            guest.Response = string.Empty;
            guest.ReminderMessageWatiId = null;
            guest.WaresponseTime = null;
            guest.whatsappMessageId = null;

            return guest;
        }
    }
}
