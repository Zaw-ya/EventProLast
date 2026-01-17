using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using EventPro.DAL;
using EventPro.DAL.Enum;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.Web.Common;
using EventPro.Web.Filters;

using ExcelDataReader;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QRCoder;

using Serilog;

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> Guests(int id)
        {
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            ViewBag.EventId = id;
            ViewBag.GmapCode = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
                .Select(p => p.GmapCode)
                .FirstOrDefaultAsync();

            ViewBag.sendingLimit = await db.AppSettings
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            if (User.IsInRole("Administrator") && !User.IsInRole("Operator"))
            {
                ViewBag.IsAdmin = 1;
            }
            else
            {
                ViewBag.IsAdmin = 0;
            }

            SetBreadcrum("Guest", "/");
            return View("Guests - Copy");
        }

        // Get Guests list + Guests FullTable 
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> GetGuests(int id)
        {
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            string searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            var sortColumn = Request.Form[string.Concat("columns[", Request.Form["order[0][column]"], "][name]")];
            var sortColumnDirection = Request.Form["order[0][dir]"];

            IQueryable<vwGuestInfo> guests = db.vwGuestInfo.Where(e => (e.EventId == id && e.GuestArchieved == false) && (
            string.IsNullOrEmpty(searchValue) ? true
            : (e.GuestId.ToString().Contains(searchValue))
            || (e.CreatedOn.ToString().Contains(searchValue))
            || (e.EventId.ToString().Contains(searchValue))
            || (string.Concat(e.FirstName, " ", e.LastName).Contains(searchValue))
            || (string.Concat("+", e.SecondaryContactNo, e.PrimaryContactNo).Contains(searchValue))
            )).AsNoTracking();

            if (searchValue?.Length > 3)
            {
                if (searchValue.Contains("لم ترسل الرسالة") || searchValue.Contains("failed") || searchValue.Contains("فشلت") || searchValue.Contains("لم تتم الرسالة") || searchValue.Contains("رسالة فشلت ولم ترسل من الواتس اب رسالة بسبب مشكلة تقنية"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && ((
                    ((p.ConguratulationMsgRead != true)
                && (p.ConguratulationMsgDelivered != true)
                && ((p.ConguratulationMsgSent == true && p.WatiConguratulationMsgId != null && p.TextDelivered != true && p.TextRead != true) || p.ConguratulationMsgFailed == true))) ||
                    ((p.ReminderMessageRead != true)
                && (p.ReminderMessageDelivered != true)
                && ((p.ReminderMessageSent == true && p.ReminderMessageWatiId != null && p.TextDelivered != true && p.TextRead != true) || p.ReminderMessageFailed == true)) ||
                    ((p.EventLocationRead != true)
                && (p.EventLocationDelivered != true)
                && ((p.EventLocationSent == true && p.whatsappWatiEventLocationId != null && p.TextDelivered != true && p.TextRead != true) || p.EventLocationFailed == true)) ||
                    ((p.ImgRead != true)
                && (p.ImgDelivered != true)
                && ((p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true) || p.ImgFailed == true)) ||
                ((p.TextRead != true)
                && (p.TextDelivered != true)
                && ((p.TextSent == true && p.whatsappMessageId != null) || p.TextFailed == true)))
                    ).AsNoTracking();
                }

                if (searchValue.Contains("قرأت الرسالة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                    p.ImgRead == true))
                        .AsNoTracking();
                }
                if (searchValue.Contains("WA ERROR") || searchValue.Contains("wa error") || searchValue.Contains("error") || searchValue.Contains("wa") || searchValue.Contains("WA Error"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                    p.Response.Contains("WA Error")))
                        .AsNoTracking();
                }

                if (searchValue.Contains("وصلت الرسالة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                    p.ImgRead != true &&
                         p.ImgDelivered == true))
                        .AsNoTracking();
                }

                if (searchValue.Contains("معلقة الرسالة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.TextFailed != true &&
                  p.TextDelivered != true &&
                  p.TextRead != true &&
                  p.TextSent == true &&
                  !(p.TextSent == true && p.whatsappMessageId != null)))
                        .AsNoTracking();

                }

                if (searchValue.Contains("قرأت الصورة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.ImgRead == true))
                        .AsNoTracking();
                }

                if (searchValue.Contains("قرأت الصورة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.ImgRead != true &&
                         p.ImgDelivered == true))
                        .AsNoTracking();
                }

                if (searchValue.Contains("معلقة الصورة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.ImgFailed != true &&
                  p.ImgDelivered != true &&
                  p.ImgRead != true &&
                  p.ImgSent == true &&
                  !(p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true)))
                        .AsNoTracking();
                }

                if (searchValue.Contains("لم تتم الصورة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     (p.ImgRead != true)
                && (p.ImgDelivered != true)
                && ((p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true) || p.ImgFailed == true)))
                        .AsNoTracking();

                }


                if (searchValue.Contains("تمت قراءة الموقع"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.EventLocationRead == true))
                        .AsNoTracking();
                }


                if (searchValue.Contains("تمت قراءة الموقع"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                    p.EventLocationRead != true &&
                         p.EventLocationDelivered == true))
                        .AsNoTracking();
                }

                if (searchValue.Contains("معلقة رسالة الموقع"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.EventLocationFailed != true &&
                  p.EventLocationDelivered != true &&
                  p.EventLocationRead != true &&
                  p.EventLocationSent == true &&
                  !(p.EventLocationSent == true && p.whatsappWatiEventLocationId != null && p.TextDelivered != true && p.TextRead != true)))
                        .AsNoTracking();
                }

                if (searchValue.Contains("قرأت رسالة التذكير"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                      p.ReminderMessageRead == true))
                        .AsNoTracking();
                }

                if (searchValue.Contains("وصلت رسالة التذكير"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.ReminderMessageRead != true &&
                         p.ReminderMessageDelivered == true))
                        .AsNoTracking();
                }

                if (searchValue.Contains("معلقة رسالة التذكير"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                      p.ReminderMessageFailed != true &&
                  p.ReminderMessageDelivered != true &&
                  p.ReminderMessageRead != true &&
                  p.ReminderMessageSent == true &&
                  !(p.ReminderMessageSent == true && p.ReminderMessageWatiId != null && p.TextDelivered != true && p.TextRead != true)))
                        .AsNoTracking();
                }

                if (searchValue.Contains("لم ترسل رسالة التذكير"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     (p.ReminderMessageRead != true)
                && (p.ReminderMessageDelivered != true)
                && ((p.ReminderMessageSent == true && p.ReminderMessageWatiId != null && p.TextDelivered != true && p.TextRead != true) || p.ReminderMessageFailed == true)))
                        .AsNoTracking();
                }

                if (searchValue.Contains("قرأت رسالة التهنئة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                      p.ConguratulationMsgRead == true))
                        .AsNoTracking();
                }

                if (searchValue.Contains("وصلت رسالة التهنئة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.ConguratulationMsgRead != true &&
                         p.ConguratulationMsgDelivered == true))
                        .AsNoTracking();
                }

                if (searchValue.Contains("معلقة رسالة التهنئة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.ConguratulationMsgFailed != true &&
                  p.ConguratulationMsgDelivered != true &&
                  p.ConguratulationMsgRead != true &&
                  p.ConguratulationMsgSent == true &&
                  !(p.ConguratulationMsgSent == true && p.WatiConguratulationMsgId != null && p.TextDelivered != true && p.TextRead != true)))
                        .AsNoTracking();
                }

                if (searchValue.Contains("لم ترسل رسالة التهنئة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     (p.ConguratulationMsgRead != true)
                && (p.ConguratulationMsgDelivered != true)
                && ((p.ConguratulationMsgSent == true && p.WatiConguratulationMsgId != null && p.TextDelivered != true && p.TextRead != true) || p.ConguratulationMsgFailed == true)))
                        .AsNoTracking();
                }


                if (searchValue.Contains("رفضوا") || searchValue.Contains("لم يتمكن بحضور الحفلة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     (p.Response.Equals("Decline") ||
                        (p.Response.Equals("اعتذار عن الحضور")))))
                        .AsNoTracking();
                }

                if (searchValue.Contains("حضور"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     (p.Response.Equals("Confirm") ||
                        (p.Response.Equals("تأكيد الحضور")))))
                        .AsNoTracking();

                }

                if (searchValue.Contains("ربما"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) &&
                        (p.Response.Equals("ربما")))
                        .AsNoTracking();
                }

                if (searchValue.Contains("تحت الانتظار"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) &&
                        (p.Response.Equals("تحت الانتظار")))
                        .AsNoTracking();
                }

                if (searchValue.Contains("مرسلة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                    (p.Response.Equals("Message Processed Successfully"))
                && ((p.TextFailed != true)
                && ((p.TextDelivered == true)
                || (p.TextRead == true)
                || !(p.TextSent == true && p.whatsappMessageId != null)))))
                        .AsNoTracking();
                }
            }
            int recordsTotal = 0;
            guests = guests.OrderByDescending(e => e.GuestId);
            recordsTotal = await guests.CountAsync();

            var result = new List<vwGuestInfo>();
            try
            {
                pageSize = pageSize == -1 ? recordsTotal : pageSize;

                result = await guests
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                var error = new
                {
                    Message = ex.Message,
                    Inner = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source
                };

                return StatusCode(500, error);
            }

            List<GuestVM> guestsVM = new();

            foreach (var guest in result)
            {
                GuestVM guestVM = new(guest);
                guestsVM.Add(guestVM);
            }


            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = guestsVM
            };

            return Ok(jsonData);
        }

        // Add new Guest
        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> Guest(Guest guest)
        {
            var AddedOrModified = 0;

            try
            {
                AddedOrModified = await AddOrModifyGuest(guest);
                return Json(new { success = true, addedOrModified = AddedOrModified });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }

        // Add or modify guest (Upsert)
        public async Task<int> AddOrModifyGuest(Guest guest)
        {
            int guestId = 0;
            int eventId = Convert.ToInt32(guest.EventId);
            var addedOrModified = 0;


            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            string guestcode = _configuration.GetSection("Uploads").GetSection("Guestcode").Value;
            string path = _configuration.GetSection("Uploads").GetSection("Card").Value;
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // If we send the GuestId it means we update
            if (guest.GuestId > 0)
            {
                guestId = guest.GuestId;
                Guest gst = await db.Guest.Where(p => p.GuestId == guest.GuestId).FirstOrDefaultAsync();
                gst.FirstName = guest.FirstName;
                gst.NoOfMembers = guest.NoOfMembers;
                gst.AdditionalText = guest.AdditionalText;
                gst.PrimaryContactNo = guest.PrimaryContactNo;
                gst.SecondaryContactNo = guest.SecondaryContactNo;
                gst.IsPhoneNumberValid = true;
                gst.GuestArchieved = false;
                eventId = Convert.ToInt32(gst.EventId);
                guest.Cypertext = EventProCrypto.EncryptString(_configuration.GetSection("SecurityKey").Value, Convert.ToString(guest.GuestId));
                await _auditLogService.AddAsync(userId, eventId, ActionEnum.UpdateGuest, guestId, gst.FirstName);
                await db.SaveChangesAsync();
                addedOrModified = 2;
            }
            else
            {
                // It means we create
                guest.CreatedBy = userId;
                guest.CreatedOn = DateTime.Now;
                guest.IsPhoneNumberValid = true;
                guest.GuestArchieved = false;
                guest.Source = "Entry";
                // Gharabawy needed to understand this cypher text
                guest.Cypertext = EventProCrypto.EncryptString(_configuration.GetSection("SecurityKey").Value, Convert.ToString(guest.GuestId));
                var newGuest = await db.Guest.AddAsync(guest);
                await db.SaveChangesAsync();
                addedOrModified = 1;
                guest = newGuest.Entity;
                guestId = newGuest.Entity.GuestId;
                await _auditLogService.AddAsync(userId, eventId, ActionEnum.AddGuest, guestId, guest.FirstName);
            }

            Log.Information("Event {eId} guest {gId} added/modified by {uId}", eventId, guestId, userId);
            // Get the event card info 
            var cardinfo = await db.CardInfo.Where(p => p.EventId == eventId).FirstOrDefaultAsync();
            
            await RefreshQRCode(guest, cardinfo);
            await RefreshCard(guest, eventId, cardinfo, cardPreview, guestcode, path);
            await db.SaveChangesAsync();

            return addedOrModified;
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> DeleteGuest(int id)
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var guest = await db.Guest.Where(p => p.GuestId == id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            int eventId = Convert.ToInt32(guest.EventId);
            db.Guest.Remove(guest);
            await db.SaveChangesAsync();
            await _auditLogService.AddAsync(userId, eventId, ActionEnum.DeleteGuest, id, guest.FirstName);
            Log.Information("Event {eId} guest {gId} removed by {uId}", eventId, guest.GuestId, userId);
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            string environment = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            //await _cloudinaryService.DeleteAsync();

            TempData["error"] = "Guest information deleted successfully!";

            return RedirectToAction("Guests", "admin", new { id = eventId });
        }


        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> InviteOnWhatsapp(int id)
        {
            Guest guest = await db.Guest.Where(p => p.GuestId == id)
                .FirstOrDefaultAsync();

            if (guest == null)
            {
                return BadRequest();
            }
            var guests = new List<Guest>() { guest };

            var _event = await db.Events.Where(p => p.Id == guest.EventId)
                .FirstOrDefaultAsync();

            if (_event.ConfirmationButtonsType == "Links")
            {
                if (string.IsNullOrEmpty(_event.LinkGuestsLocationEmbedSrc))
                    return Json(new { success = false, message = "رابط الموقع غير موجود" });

                if (string.IsNullOrEmpty(_event.LinkGuestsCardText))
                    return Json(new { success = false, message = "نص الرسالة غير موجود" });
            }

            if (!CheckEventLocationExists(_event))
                return Json(new { success = false, message = "رابط الموقع غير موجود" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "رقم الجوال غير موجود" });

            if (!await CheckGuestsCardsExistAsync(guests, _event))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            try
            {
                var sendingProvider = await _WhatsappSendingProvider
                    .SelectConfiguredSendingProviderAsync(_event);
                await sendingProvider.SendConfirmationMessagesAsync(guests, _event);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }

            return Json(new { success = true });
        }

        public async Task<string> sendSelectedTemplate(Guest guest, Events evt)
        {
            if (evt.MessageLanguage == "English")
            {
                if (evt.MessageHeaderImage == string.Empty)
                {
                    if (evt.MessageHeaderText == null)
                    {

                        return await _watiService.SendEnglishInvitaionTemplate(guest, evt);
                    }
                    else
                    {

                        return await _watiService.SendEnglishInvitaionTemplateWithHeaderText(guest, evt);
                    }

                }
                else
                {
                    if (evt.MessageHeaderText == null)
                    {

                        return await _watiService.SendEnglishInvitaionTemplateWithHeaderImage(guest, evt);

                    }
                    else
                    {

                        return await _watiService.SendEnglishInvitaionTemplateWihtHeaderTextAndHeaderImage(guest, evt);
                    }
                }

            }
            else if (evt.MessageLanguage == "Work Invitaion")
            {
                if (evt.MessageHeaderImage == string.Empty)
                {
                    if (evt.MessageHeaderText == null)
                    {

                        return await _watiService.SendWorkInvitationTemplate(guest, evt);
                    }
                    else
                    {

                        return await _watiService.SendWorkInvitationTemplateWithHeaderText(guest, evt);
                    }

                }
                else
                {
                    if (evt.MessageHeaderText == null)
                    {

                        return await _watiService.SendWorkInvitationTemplateWithHeaderImage(guest, evt);

                    }
                    else
                    {

                        return await _watiService.SendWorkInvitationTemplateWithHeaderTextAndHeaderImage(guest, evt);
                    }
                }
            }

            else if (evt.MessageLanguage == "Custom-Invitation")
            {
                return await _watiService.SendCustomInvitaionTemplate(guest, evt);
            }
            else if (evt.MessageLanguage == "Custom-Invitation with client name")
            {
                return await _watiService.SendCustomInvitaionWithClientNameTemplate(guest, evt);
            }
            else if (evt.MessageLanguage == "QR-Invitation")
            {
                return await _watiService.SendQRInvitaionTemplate(guest, evt);
            }
            else if (evt.MessageLanguage == "Arabic Male")
            {
                if (evt.MessageHeaderImage == string.Empty)
                {
                    if (evt.MessageHeaderText == null)
                    {
                        return await _watiService.SendArabicMaleInvitaionTemplate(guest, evt);
                    }
                    else
                    {
                        return await _watiService.SendArabicMaleInvitaionTemplateWithHeaderText(guest, evt);
                    }

                }
                else
                {
                    if (evt.MessageHeaderText == null)
                    {
                        return await _watiService.SendArabicMaleInvitaionTemplateWithHeaderImage(guest, evt);

                    }
                    else
                    {
                        return await _watiService.SendArabicMaleInvitaionTemplateWithHeaderImageAndHeaderText(guest, evt);
                    }
                }


            }

            else if (evt.MessageLanguage == "Arabic Female")
            {
                if (evt.MessageHeaderImage == string.Empty)
                {
                    if (evt.MessageHeaderText == null)
                    {
                        return await _watiService.SendArabicFemaleInvitaionTemplate(guest, evt);
                    }
                    else
                    {
                        return await _watiService.SendArabicFemaleInvitaionTemplateWithHeaderText(guest, evt);
                    }

                }
                else
                {
                    if (evt.MessageHeaderText == null)
                    {
                        return await _watiService.SendArabicFemaleInvitaionTemplateWithHeaderImage(guest, evt);

                    }
                    else
                    {
                        return await _watiService.SendArabicFemaleInvitaionTemplateWithHeaderImageAndHeaderText(guest, evt);
                    }
                }
            }
            else if (evt.MessageLanguage == "English_without_parentTitle")
            {
                if (evt.MessageHeaderImage == string.Empty)
                {
                    if (evt.MessageHeaderText == null)
                    {

                        return await _watiService.SendEnglishInvitaionTemplate(guest, evt);
                    }
                    else
                    {

                        return await _watiService.SendEnglishInvitaionTemplateWithHeaderText(guest, evt);
                    }

                }
                else
                {
                    if (evt.MessageHeaderText == null)
                    {

                        return await _watiService.SendEnglishInvitaionTemplateWithHeaderImage(guest, evt);

                    }
                    else
                    {

                        return await _watiService.SendEnglishInvitaionTemplateWihtHeaderTextAndHeaderImage(guest, evt);
                    }
                }
            }
            else if (evt.MessageLanguage == "Arabic")
            {
                if (evt.ParentTitleGender == "Male")
                {
                    if (evt.MessageHeaderImage == string.Empty)
                    {
                        if (evt.MessageHeaderText == null)
                        {
                            return await _watiService.SendArabicMaleInvitaionTemplate(guest, evt);
                        }
                        else
                        {
                            return await _watiService.SendArabicMaleInvitaionTemplateWithHeaderText(guest, evt);
                        }

                    }
                    else
                    {
                        if (evt.MessageHeaderText == null)
                        {
                            return await _watiService.SendArabicMaleInvitaionTemplateWithHeaderImage(guest, evt);

                        }
                        else
                        {
                            return await _watiService.SendArabicMaleInvitaionTemplateWithHeaderImageAndHeaderText(guest, evt);
                        }
                    }
                }
                else
                {
                    if (evt.MessageHeaderImage == string.Empty)
                    {
                        if (evt.MessageHeaderText == null)
                        {
                            return await _watiService.SendArabicFemaleInvitaionTemplate(guest, evt);
                        }
                        else
                        {
                            return await _watiService.SendArabicFemaleInvitaionTemplateWithHeaderText(guest, evt);
                        }

                    }
                    else
                    {
                        if (evt.MessageHeaderText == null)
                        {
                            return await _watiService.SendArabicFemaleInvitaionTemplateWithHeaderImage(guest, evt);

                        }
                        else
                        {
                            return await _watiService.SendArabicFemaleInvitaionTemplateWithHeaderImageAndHeaderText(guest, evt);
                        }
                    }

                }
            }

            return "No Matching Template Found";
        }

        public async Task<IActionResult> GetCardSendingData(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {

                var bulkSendingLimit = await db.AppSettings
                                      .AsNoTracking()
                                      .Select(e => e.BulkSendingLimit)
                                      .FirstOrDefaultAsync();

                var remainingMessagesCount = await db.Guest.Where(e => e.EventId == id && e.ImgSentMsgId == null).CountAsync();
                remainingMessagesCount = remainingMessagesCount >= bulkSendingLimit ? bulkSendingLimit : remainingMessagesCount;
                return Json(new { sentMessages = 0, remainingMessages = remainingMessagesCount });
            }
            else
            {
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());
                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendCardToAll(int id)
        {
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            var guests = await db.Guest
                .Where(p => p.EventId == id && (p.ImgSentMsgId == null))
                .Take(sendingBulkLimit)
                .ToListAsync();

            var _event = await db.Events
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            if (!await CheckGuestsCardsExistAsync(guests, _event))
                return Json(new { success = false, message = "صورة البطاقة غير موجودة" });

            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                _WebHookQueueConsumerService.Pause();
                var sendingProvider = await _WhatsappSendingProvider
                    .SelectConfiguredSendingProviderAsync(_event);
                await sendingProvider.SendCardMessagesAsync(guests, _event);
            }
            catch
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }
            finally
            {
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });
        }

        public async Task<IActionResult> GetEventLocationSendingData(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                var bulkSendingLimit = await db.AppSettings
                      .AsNoTracking()
                      .Select(e => e.BulkSendingLimit)
                      .FirstOrDefaultAsync();

                var remainingMessagesCount = await db.Guest.Where(e => e.EventId == id && e.waMessageEventLocationForSendingToAll == null).CountAsync();
                remainingMessagesCount = remainingMessagesCount >= bulkSendingLimit ? bulkSendingLimit : remainingMessagesCount;
                return Json(new { sentMessages = 0, remainingMessages = remainingMessagesCount });
            }
            else
            {
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendEventLocationToAll(int id)
        {
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            var guests = await db.Guest.Where(p => p.EventId == id &&
            (p.waMessageEventLocationForSendingToAll == null))
                .Take(sendingBulkLimit)
                .ToListAsync();

            var _event = await db.Events
                .Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            if (!CheckEventLocationExists(_event))
                return Json(new { success = false, message = "رابط الموقع غير موجود" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                _WebHookQueueConsumerService.Pause();
                var whatsappProvider = await _WhatsappSendingProvider.SelectConfiguredSendingProviderAsync(_event);
                await whatsappProvider.SendEventLocationAsync(guests, _event);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }
            finally
            {
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });
        }

        public async Task<IActionResult> GetReminderMessagesSendingDataToAll(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                var bulkSendingLimit = await db.AppSettings
                    .AsNoTracking()
                    .Select(e => e.BulkSendingLimit)
                    .FirstOrDefaultAsync();

                var remainingMessagesCount = await db.Guest.Where(e => e.EventId == id && e.ReminderMessageId == null).CountAsync();
                remainingMessagesCount = remainingMessagesCount >= bulkSendingLimit ? bulkSendingLimit : remainingMessagesCount;
                return Json(new { sentMessages = 0, remainingMessages = remainingMessagesCount });
            }
            else
            {
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendReminderMessageToAll(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (HasOperatorRole())
            {
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            var guests = await db.Guest.Where(p => p.EventId == id &&
            (p.ReminderMessageId == null) &&
            (!p.Response.Equals("Decline")) &&
            (!p.Response.Equals("اعتذار عن الحضور")))
                .Take(sendingBulkLimit).ToListAsync();

            var _event = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                _WebHookQueueConsumerService.Pause();
                var whatsappProvider = await _WhatsappSendingProvider
                    .SelectConfiguredSendingProviderAsync(_event);
                await whatsappProvider.SendReminderMessageAsync(guests, _event);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }
            finally
            {
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }


            //await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.SendReminderToAll);
            return Json(new { success = true });
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendReminderMessageToOnlyReceived(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (HasOperatorRole())
            {

                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            var _event = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
               .FirstOrDefaultAsync();

            var guests = new List<Guest>();
            if (_event.WhatsappConfirmation == true)
            {
                guests = await db.Guest.Where(p => p.EventId == id && p.MessageId != null && p.TextFailed != true &&
                           (p.ReminderMessageId == null) &&
                           (!p.Response.Equals("Decline")) &&
                           (!p.Response.Equals("اعتذار عن الحضور")))
                            .Take(sendingBulkLimit).ToListAsync();
            }
            else if (_event.WhatsappPush == true)
            {
                guests = await db.Guest.Where(p => p.EventId == id && p.ImgSentMsgId != null && p.ImgFailed != true &&
                            (p.ReminderMessageId == null) &&
                            (!p.Response.Equals("Decline")) &&
                            (!p.Response.Equals("اعتذار عن الحضور")))
                             .Take(sendingBulkLimit).ToListAsync();
            }

            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                _WebHookQueueConsumerService.Pause();
                var whatsappProvider = await _WhatsappSendingProvider
                    .SelectConfiguredSendingProviderAsync(_event);
                await whatsappProvider.SendReminderMessageAsync(guests, _event);
            }
            catch
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }
            finally
            {
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }


            //await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.SendReminderToOnlyReceived);
            return Json(new { success = true });
        }
        
        public async Task<IActionResult> GetReminderMessagesSendingDataToOnlyAccepted(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                var bulkSendingLimit = await db.AppSettings
                                       .AsNoTracking()
                                       .Select(e => e.BulkSendingLimit)
                                       .FirstOrDefaultAsync();

                var remainingMessagesCount = await db.Guest.Where(e => e.EventId == id && e.ReminderMessageId == null &&
                        ((e.Response.Equals("Confirm")) ||
                        (e.Response.Equals("تأكيد الحضور")))).CountAsync();
                remainingMessagesCount = remainingMessagesCount >= bulkSendingLimit ? bulkSendingLimit : remainingMessagesCount;
                return Json(new { sentMessages = 0, remainingMessages = remainingMessagesCount });
            }
            else
            {
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendReminderMessageToOnlyAccepted(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (HasOperatorRole())
            {

                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }
            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            var guests = await db.Guest.Where(p => p.EventId == id && (p.ReminderMessageId == null) &&
                        ((p.Response.Equals("Confirm")) ||
                        (p.Response.Equals("تأكيد الحضور"))))
                        .Take(sendingBulkLimit).ToListAsync();

            var _event = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                _WebHookQueueConsumerService.Pause();
                var whatsappProvider = await _WhatsappSendingProvider
                    .SelectConfiguredSendingProviderAsync(_event);
                await whatsappProvider.SendReminderMessageAsync(guests, _event);
            }
            catch
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }
            finally
            {
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            //await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.SendReminderToOnlyAccepted);
            return Json(new { success = true });
        }

        public async Task<IActionResult> GetReminderMessagesSendingDataToNoAnswer(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                var bulkSendingLimit = await db.AppSettings
                                       .AsNoTracking()
                                       .Select(e => e.BulkSendingLimit)
                                       .FirstOrDefaultAsync();

                var remainingMessagesCount = await db.Guest.Where(e => e.EventId == id && e.ReminderMessageId == null &&
                        (e.Response.Equals("Message Processed Successfully"))).CountAsync();
                remainingMessagesCount = remainingMessagesCount >= bulkSendingLimit ? bulkSendingLimit : remainingMessagesCount;
                return Json(new { sentMessages = 0, remainingMessages = remainingMessagesCount });
            }
            else
            {
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendReminderMessageToOnlyNoAnswer(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (HasOperatorRole())
            {

                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            var guests = await db.Guest.Where(p => p.EventId == id && (p.ReminderMessageId == null) &&
                        (p.Response.Equals("Message Processed Successfully")))
                        .Take(sendingBulkLimit).ToListAsync();

            var _event = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                _WebHookQueueConsumerService.Pause();
                var whatsappProvider = await _WhatsappSendingProvider
                    .SelectConfiguredSendingProviderAsync(_event);
                await whatsappProvider.SendReminderMessageAsync(guests, _event);
            }
            catch
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }
            finally
            {
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            //await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.SendReminderToOnlyNoAnswer);
            return Json(new { success = true });
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendReminderMessage(int id)
        {
            var guest = await db.Guest
                .FirstOrDefaultAsync(p => p.GuestId == id);

            var _event = await db.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == guest.EventId);
            var guests = new List<Guest> { guest };

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "رقم الجوال غير موجود" });

            try
            {
                var whatsappProvider = await _WhatsappSendingProvider
                    .SelectConfiguredSendingProviderAsync(_event);
                await whatsappProvider.SendReminderMessageAsync(guests, _event);
            }
            catch
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }
            return Json(new { success = true });
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendToAll(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (HasOperatorRole())
            {

                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            var guests = await db.Guest
                .Where(p => p.EventId == id && (p.MessageId == null))
                .Take(sendingBulkLimit)
                .ToListAsync();

            var _event = await db.Events.Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            if (_event.ConfirmationButtonsType == "Links")
            {
                if (string.IsNullOrEmpty(_event.LinkGuestsLocationEmbedSrc))
                    return Json(new { success = false, message = "رابط الموقع غير موجود" });

                if (string.IsNullOrEmpty(_event.LinkGuestsCardText))
                    return Json(new { success = false, message = "نص الرسالة غير موجود" });
            }

            if (!CheckEventLocationExists(_event))
                return Json(new { success = false, message = "رابط الموقع غير موجود" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            if (!await CheckGuestsCardsExistAsync(guests, _event))
                return Json(new { success = false, message = "صورة البطاقة غير موجودة" });

            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                _WebHookQueueConsumerService.Pause();
                var whatsappProvider = await _WhatsappSendingProvider
                    .SelectConfiguredSendingProviderAsync(_event);
                await whatsappProvider.SendConfirmationMessagesAsync(guests, _event);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }
            finally
            {
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            //await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.SendConfirmation);
            return Json(new { success = true });

        }

        public async Task<IActionResult> GetConfirmationSendingData(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {

                var bulkSendingLimit = await db.AppSettings
                                       .AsNoTracking()
                                       .Select(e => e.BulkSendingLimit)
                                       .FirstOrDefaultAsync();

                var remainingMessagesCount = await db.Guest.Where(e => e.EventId == id && e.MessageId == null).CountAsync();
                remainingMessagesCount = remainingMessagesCount >= bulkSendingLimit ? bulkSendingLimit : remainingMessagesCount;
                return Json(new { sentMessages = 0, remainingMessages = remainingMessagesCount });
            }
            else
            {
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        public async Task<IActionResult> GetCongratulationSendingData(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                var bulkSendingLimit = await db.AppSettings
                                         .AsNoTracking()
                                         .Select(e => e.BulkSendingLimit)
                                         .FirstOrDefaultAsync();

                var remainingMessagesCount = await db.Guest.Where(e => e.EventId == id &&
               (e.ScanHistory.Where(p => p.ResponseCode == "Allowed").Count() > 0) &&
               (e.ConguratulationMsgId == null)).CountAsync();
                remainingMessagesCount = remainingMessagesCount >= bulkSendingLimit ? bulkSendingLimit : remainingMessagesCount;
                return Json(new { sentMessages = 0, remainingMessages = remainingMessagesCount });
            }
            else
            {
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendCongratulationMessageToAll(int id)
        {
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            var guests = await db.Guest
                .Where(p => p.EventId == id &&
                (p.ScanHistory.Where(e => e.ResponseCode == "Allowed").Count() > 0)
                && (p.ConguratulationMsgId == null))
                .Take(sendingBulkLimit)
                .ToListAsync();

            var _event = await db.Events
                .Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            if (_event.ConguratulationsMsgSentOnNumber == null)
                return Json(new { success = false, message = "نسبة الضيف لارسل رقم الجوال الذي سيتم إرسال رسالة تأكيد الحضور منه غير محدد" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                _WebHookQueueConsumerService.Pause();
                var whatsappProvider = await _WhatsappSendingProvider.SelectConfiguredSendingProviderAsync(_event);
                await whatsappProvider.SendCongratulationMessageAsync(guests, _event);
            }
            catch
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }
            finally
            {
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });

        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendQRCode(int id)
        {
            Guest guest = await db.Guest.Where(p => p.GuestId == id)
                .FirstOrDefaultAsync();
            var guests = new List<Guest>() { guest };

            var _event = await db.Events.Where(p => p.Id == guest.EventId)
                .FirstOrDefaultAsync();

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "رقم الجوال غير موجود" });

            if (!await CheckGuestsCardsExistAsync(guests, _event))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            try
            {
                var sendingProvider = await _WhatsappSendingProvider
                    .SelectConfiguredSendingProviderAsync(_event);
                await sendingProvider.SendCardMessagesAsync(guests, _event);
            }
            catch
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }

            //var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            //await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.SendCards);
            return Json(new { success = true });
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> DeletePastEventsCards()
        {
            if (_configuration.GetSection("Database")["ConnectionString"].ToLower().Contains("EventProuat"))
            {
                return Json(new { success = false });
            }

            try
            {
                var AllCardsDirectory = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
                var environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
                List<string> evntsFolder = await _blobStorage.GetFoldersInsideAFolderAsync(environment + AllCardsDirectory, cancellationToken: default);
                var upcomingAndCurrentEvnts = await db.Events.Where(e => e.EventTo >= DateTime.Now)
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var evntFolder in evntsFolder)
                {
                    if (!upcomingAndCurrentEvnts.Exists(e => e.Id.ToString() == evntFolder))
                    {
                        await _blobStorage.DeleteFolderAsync(environment + AllCardsDirectory + "/" + evntFolder, cancellationToken: default);
                    }
                }

                return Json(new { success = true });
            }

            catch
            {
                return Json(new { success = false });
            }
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> DeleteSerilogData()
        {
            //var access = AccessService.AccessVerification(this.HttpContext);
            //if (access != null) return access;

            try
            {
                await db.Database.ExecuteSqlAsync(
                    $"TRUNCATE TABLE SeriLog"
                    );
                await db.Database.ExecuteSqlAsync(
                    $"TRUNCATE TABLE SeriLogAPI"
                    );

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> SetDeclineResponse(int id)
        {
            string _noText_Eng = _configuration.GetSection("PinacleSettings").GetSection("TextNo_Eng").Value;
            var guest = await db.Guest.FirstOrDefaultAsync(p => p.GuestId == id);
            guest.Response = _noText_Eng;
            db.Guest.Update(guest);
            await db.SaveChangesAsync();
            return Ok();
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SetAcceptResponse(int id)
        {
            string _yesText_Eng = _configuration.GetSection("PinacleSettings").GetSection("TextYes_Eng").Value;
            var guest = await db.Guest.FirstOrDefaultAsync(p => p.GuestId == id);
            guest.Response = _yesText_Eng;
            if (string.IsNullOrEmpty(guest.MessageId))
            {
                guest.MessageId = "not null";
            }
            db.Guest.Update(guest);
            await db.SaveChangesAsync();
            return Ok();
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendEventLocation(int id)
        {
            var guest = await db.Guest
                .FirstOrDefaultAsync(p => p.GuestId == id);

            var _event = await db.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == guest.EventId);

            var guests = new List<Guest>();
            guests.Add(guest);

            if (!CheckEventLocationExists(_event))
                return Json(new { success = false, message = "رابط الموقع غير موجود" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "رقم الجوال غير موجود?" });

            try
            {
                var whatsappProvider = await _WhatsappSendingProvider.SelectConfiguredSendingProviderAsync(_event);
                await whatsappProvider.SendEventLocationAsync(guests, _event);
            }
            catch
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }

            //var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            //await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.SendEventLocation);
            return Json(new { success = true });
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> sendConguratilationMessage(int id)
        {
            var defaultWhatsAppProvider = db.AppSettings
               .Select(e => e.WhatsappServiceProvider)
               .AsNoTracking()
               .FirstOrDefault();

            Guest guest = await db.Guest.Where(p => p.GuestId == id)
                .FirstOrDefaultAsync();

            var _event = await db.Events.Where(p => p.Id == guest.EventId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var guests = new List<Guest>() { guest };

            if (_event.ConguratulationsMsgSentOnNumber == null)
                return Json(new { success = false, message = "نسبة الضيف لارسل رقم الجوال الذي سيتم إرسال رسالة تأكيد الحضور منه غير محدد" });

            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "رقم الجوال غير موجود" });

            try
            {
                var whatsappProvider = await _WhatsappSendingProvider.SelectConfiguredSendingProviderAsync(_event);
                await whatsappProvider.SendCongratulationMessageAsync(guests, _event);
            }
            catch
            {
                return Json(new { success = false, message = "??? ??? ??" });
            }

            //var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            //await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.SendThanks);
            return Json(new { success = true });
        }

        public IActionResult DownloadGuestImage([FromQuery] string url)
        {

            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(url);
                client.Dispose();

                return File(stream, "image/jpeg", "EventProCardInvitation.jpg");
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public IActionResult Guests(Guest guest)
        {
            var files = Request.Form.Files;
            string path = _configuration.GetSection("Uploads").GetSection("Excel").Value;
            string excelConnection = _configuration.GetSection("Database").GetSection("ExcelConnection").Value;
            string filename = string.Empty;
            bool hasFile = false;
            foreach (var file in files)
            {
                string extension = file.ContentType.ToLower().Replace(@"image/", "");
                filename = Guid.NewGuid() + ".xlsx";
                using (var fileStream = new FileStream(path + @"\" + filename, FileMode.Create))
                {
                    hasFile = true;
                    file.CopyTo(fileStream);
                }
            }
            if (hasFile)
            {
                var excelData = ImportFromExcel.ImportDataFromExcel(path + @"\" + filename, excelConnection);
            }
            return RedirectToAction("Guests", "admin", new { id = guest.EventId });
        }

        // For the bulk upload of guests via excel file
        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> Upload(int eventId, IFormCollection form)
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (HasOperatorRole())
            {
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == eventId);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var totalGuests = 0;
            var totalMembers = 0;
            try
            {
                List<Guest> newGuests = new List<Guest>();
                var files = Request.Form.Files;

                foreach (var file in files)
                {
                    string path = _configuration.GetSection("Uploads").GetSection("Excel").Value;
                    string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
                    string filename = Guid.NewGuid().ToString() + "_" + file.FileName.Replace(" ", "_");
                    if (filename.ToLower().EndsWith(".xlsx") || filename.ToLower().EndsWith(".xls"))
                    {
                        using var stream = file.OpenReadStream();
                        await _blobStorage.UploadAsync(stream, "xlsx", environment + path + "/" + filename, cancellationToken: default);
                        DataSet data;

                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            data = reader.AsDataSet(new ExcelDataSetConfiguration
                            {
                                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                                {
                                    UseHeaderRow = true // Treat first row as column names
                                }
                            });
                        }

                        foreach (DataRow row in data.Tables[0].Rows)
                        {
                            if (row["Members"].ToString().Length > 0 &&
                                row["GuestName"] != null)

                            {
                                Guest guest = new Guest();
                                guest.FirstName = Convert.ToString(row["GuestName"]);
                                guest.AdditionalText = Convert.ToString(row["AdditionalText"]);
                                guest.NoOfMembers = Convert.ToInt32(row["Members"]);
                                guest.SecondaryContactNo = Convert.ToString(int.Parse(row["CountryCode"].ToString())).Trim();
                                guest.PrimaryContactNo = Convert.ToString(double.Parse(row["ContactNo"].ToString())).Trim();
                                guest.CreatedBy = userId;
                                guest.CreatedOn = DateTime.UtcNow;
                                guest.EventId = eventId;
                                guest.Source = "Upload";
                                guest.IsPhoneNumberValid = true;
                                guest.GuestArchieved = false;
                                guest.Cypertext = EventProCrypto.EncryptString(_configuration.GetSection("SecurityKey").Value, Convert.ToString(guest.GuestId));
                                newGuests.Add(guest);
                            }

                        }
                        await _auditLogService.AddAsync(userId, eventId, ActionEnum.UploadGuest);
                        await db.Guest.AddRangeAsync(newGuests);
                        await db.SaveChangesAsync();

                    }

                    else
                    {
                        return Json(new { success = false });
                    }

                }
                Log.Information("Event {eId} guests uploaded by {uId}", eventId, userId);
                totalGuests = newGuests.Count();
                totalMembers = newGuests.Select(e => e.NoOfMembers)?.Sum() ?? 0;
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true, totalGuests = totalGuests, totalMembers = totalMembers });
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> StatusRefreshIndividually(int id, int eventid)
        {
            var guest = await db.Guest
                .FirstOrDefaultAsync(x => x.GuestId == id);

            var events = await db.Events.Where(e => e.Id == eventid)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (guest == null)
                BadRequest();

            var guests = new List<Guest>() { guest };
            var whatsAppProvider = _WhatsappSendingProvider.SelectTwilioSendingProvider();
            await whatsAppProvider.UpdateMessagesStatus(guests, events);
            return Ok();
        }

        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> StatusRefreshForAllGuests(int id)
        {
            var guests = await db.Guest.Where(e => e.EventId == id)
                .ToListAsync();

            var events = await db.Events.Where(e => e.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (guests.Count == 0)
                return RedirectToAction("Guests", "admin", new { id = id });

            try
            {
                var whatsAppProvider = _WhatsappSendingProvider.SelectTwilioSendingProvider();
                await whatsAppProvider.UpdateMessagesStatus(guests, events);
            }
            catch
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }

        public async Task<IActionResult> GetRefreshGuestsStatusData(int id, int remaningStatus)
        {
            if (remaningStatus == 0)
            {
                var allStatus = await db.Guest.Where(e => e.EventId == id).CountAsync();
                return Json(new { remaningStatus = 0, allStatus = allStatus });
            }
            else
            {
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { remaningStatus = sentMessagesCount, allStatus = remaningStatus });
            }
        }

        [HttpGet]
        public IActionResult AddToCalender(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("Invalid calendar file URL.");
            }

            try
            {
                //// Download the .ics file from the provided URL
                //using (WebClient client = new WebClient())
                //{
                //    byte[] fileData = client.DownloadData(url); // Download file data

                //    // Define a default or dynamic file name (you can extract from URL if needed)
                //    string fileName = "event.ics";

                //    // Return the .ics file with the correct MIME type and file name
                //    return File(fileData, "text/calendar", fileName);
                //}
                return Redirect(url);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public IActionResult ForceStartConsumer()
        {
            _WebHookQueueConsumerService.ForceStart();
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult ForceStopConsumer()
        {
            _WebHookQueueConsumerService.ForceStop();
            return Json(new { success = true });
        }
        public DataSet ReadExcel(string file, string tableName)
        {

            string strconnection = _configuration.GetSection("Database").GetSection("ExcelConnection").Value; //GetConfigValue("xlsxconnection");
            strconnection = strconnection + "Data Source=" + file.ToString() + " ;Excel 12.0;HDR=Yes;IMEX=1";
            OleDbConnection olecn = new OleDbConnection(strconnection);
            OleDbCommand olecmd = new OleDbCommand("SELECT * from [" + tableName + "$]", olecn);
            OleDbDataAdapter olead = new OleDbDataAdapter(olecmd);
            DataSet ds = new DataSet();
            olead.Fill(ds);
            return ds;
        }

        private async Task<bool> CheckGuestsCardsExistAsync(List<Guest> guests, Events _event)
        {
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            if (!await _blobStorage.FolderExistsAsync(environment + cardPreview + "/" + _event.Id))
            {
                return false;
            }

            foreach (var guest in guests)
            {
                string imagePath = cardPreview + "/" + guest.EventId + "/" + "E00000" + guest.EventId + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                if (!await _blobStorage.FileExistsAsync(environment + imagePath))
                {
                    return false;
                }
            }

            return true;
        }
        private bool CheckGuestsNumbersExist(List<Guest> guests)
        {
            if (guests.Any(e => string.IsNullOrEmpty(e.PrimaryContactNo) ||
            string.IsNullOrEmpty(e.SecondaryContactNo)))
            {
                return false;
            }

            return true;
        }
        private bool CheckEventLocationExists(Events _event)
        {
            if (_event.GmapCode == null || _event.GmapCode.Length == 0)
            {
                return false;
            }

            return true;
        }

        private async Task RefreshCard(Guest guest, int eventId, CardInfo cardInfo, string cardPreview, string guestcode, string path)
        {
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            int guestId = guest.GuestId;
            int nos = Convert.ToInt32(guest.NoOfMembers);

            // Load template image from local storage (generated in CardPreview)
            string templatePath = Path.Combine(webHostEnvironment.WebRootPath, "upload", "cardpreview", $"{eventId}.png");

            Image img;
            if (System.IO.File.Exists(templatePath))
            {
                // Use local template
                img = Image.FromFile(templatePath);
            }
            else
            {
                // Fallback: Load background from Cloudinary if template doesn't exist
                using HttpClient client = new HttpClient();
                var imageUrl = cardInfo.BackgroundImage;
                byte[] imageData = await client.GetByteArrayAsync(imageUrl);
                using MemoryStream fs = new MemoryStream(imageData);
                img = Image.FromStream(fs);
            }

            double zoomRatio = 1;
            if (img.Width > 900)
            {
                zoomRatio = Convert.ToDouble(img.Width) / Convert.ToDouble(900);
            }

            // Load guest QR code from Cloudinary
            using HttpClient clientQR = new HttpClient();
            string cloudName = _configuration.GetSection("CloudinarySettings").GetSection("CloudName").Value;

            // var barcodeUrl = $"https://res.cloudinary.com/{cloudName}/image/upload/QR/{eventId}/{guestId}.png";
            // We refresh qr code before we refresh the card so we have to get the latest version of qr code here
            // We have to get the latest version of the qr code in case it was regenerated
            var qrPublicId = $"QR/{eventId}/{guestId}";
            var barcodeUrl = await _cloudinaryService.GetLatestVersionUrlAsync(qrPublicId);
            byte[] barcodeData = await clientQR.GetByteArrayAsync(barcodeUrl);
            
            using MemoryStream fsBarcode = new MemoryStream(barcodeData);
            Image barcode = Image.FromStream(fsBarcode);
            Bitmap myBitmap = new Bitmap(img);
            Graphics grap = Graphics.FromImage(myBitmap);

            // Draw guest QR code
            if (cardInfo.BarcodeXaxis != null && cardInfo.BarcodeYaxis != null)
            {
                grap.DrawImage(barcode, (int)(cardInfo.BarcodeXaxis * zoomRatio), (int)(cardInfo.BarcodeYaxis * zoomRatio), (int)(cardInfo.BarcodeWidth * zoomRatio), (int)(cardInfo.BarcodeWidth * zoomRatio));
            }
            else
            {
                grap.DrawImage(barcode, (int)(1 * zoomRatio), (int)(1 * zoomRatio), (int)(cardInfo.BarcodeWidth * zoomRatio), (int)(cardInfo.BarcodeWidth * zoomRatio));
            }

            var selectedValues = new string[1];
            if (cardInfo.SelectedPlaceHolder != null)
            {
                selectedValues = cardInfo.SelectedPlaceHolder.Split(',');
            }

            StringFormat frmt = new StringFormat();
            if (cardInfo.FontAlignment == "right")
                frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;

            if (selectedValues.Contains("Guest Name"))
            {

                // كود رسم اسم الضيف هنا
                double nameXAxis = (cardInfo.FontAlignment == "right") ? Convert.ToDouble(cardInfo.NameRightAxis) : Convert.ToDouble(cardInfo.ContactNameXaxis);
                double nameYAxis = (double)cardInfo.ContactNameYaxis;

                string guestName = $"{guest.FirstName} {guest.LastName ?? ""}".Trim();

                var font = new Font(cardInfo.FontName, (float)(cardInfo.FontSize * 0.63 * zoomRatio));
                var textSize = grap.MeasureString(guestName, font);

                // 1. مسح المنطقة بجزء من الخلفية الأصلية (الحل الأمثل)
                grap.DrawImage(
                    img,
                    destRect: new Rectangle(
                        (int)((nameXAxis - 10) * zoomRatio),
                        (int)((nameYAxis - 10) * zoomRatio),
                        (int)(textSize.Width + 20 * zoomRatio),
                        (int)(textSize.Height + 20 * zoomRatio)
                    ),
                    srcRect: new Rectangle(
                        (int)((nameXAxis - 10) * zoomRatio),
                        (int)((nameYAxis - 10) * zoomRatio),
                        (int)(textSize.Width + 20 * zoomRatio),
                        (int)(textSize.Height + 20 * zoomRatio)
                    ),
                    srcUnit:GraphicsUnit.Pixel
                );

                // 2. رسم النص الجديد
                StringFormat format = new StringFormat();
                if (cardInfo.FontAlignment == "right")
                    format.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                else if (cardInfo.FontAlignment == "center")
                {
                    nameXAxis = (Convert.ToDouble(cardInfo.NameRightAxis) + Convert.ToDouble(cardInfo.ContactNameXaxis)) / 2;
                    format.Alignment = StringAlignment.Center;
                }

                grap.DrawString(
                    guestName,
                    font,
                    new SolidBrush(ColorTranslator.FromHtml(cardInfo.FontColor)),
                    new PointF((float)(nameXAxis * zoomRatio), (float)(nameYAxis * zoomRatio)),
                    format
                );
            }
            if (selectedValues.Contains("Mobile No"))
            {
                double moXAxis = (cardInfo.ContactNoAlignment == "right") ? Convert.ToDouble(cardInfo.ContactRightAxis) : Convert.ToDouble(cardInfo.ContactNoXaxis);

                frmt = new StringFormat();
                if (cardInfo.ContactNoAlignment == "right")
                    frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                else
                    frmt = new StringFormat();


                var fMobile = new Font(cardInfo.ContactNoFontName, (float)(cardInfo.ContactNoFontSize * 0.63 * zoomRatio));
                var stringsizeMobile = grap.MeasureString(guest.PrimaryContactNo, fMobile);

                if (cardInfo.ContactNoAlignment == "center")
                {
                    moXAxis = (Convert.ToDouble(cardInfo.ContactRightAxis) + Convert.ToDouble(cardInfo.ContactNoXaxis)) / 2;
                    frmt = new StringFormat();
                    frmt.Alignment = StringAlignment.Center;
                }

                grap.DrawString(guest.PrimaryContactNo, new Font(cardInfo.ContactNoFontName, (float)(cardInfo.ContactNoFontSize * 0.63 * zoomRatio))
                , new SolidBrush(ColorTranslator.FromHtml(cardInfo.ContactNoFontColor))
                , new Point((int)((moXAxis * zoomRatio))
                , (int)(cardInfo.ContactNoYaxis * zoomRatio)), frmt);
            }
            if (selectedValues.Contains("Additional Text"))
            {
                double atXAxis = (cardInfo.AddTextFontAlignment == "right") ? Convert.ToDouble(cardInfo.AddTextRightAxis) : Convert.ToDouble(cardInfo.AltTextXaxis);


                frmt = new StringFormat();
                if (cardInfo.AddTextFontAlignment == "right")
                    frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                else
                    frmt = new StringFormat();


                var fAdditional = new Font(cardInfo.AltTextFontName, (float)(cardInfo.AltTextFontSize * 0.63 * zoomRatio));
                var stringsizeAdditional = grap.MeasureString(guest.AdditionalText, fAdditional);
                if (cardInfo.AddTextFontAlignment == "center")
                {
                    atXAxis = (Convert.ToDouble(cardInfo.AddTextRightAxis) + Convert.ToDouble(cardInfo.AltTextXaxis)) / 2;
                    frmt = new StringFormat();
                    frmt.Alignment = StringAlignment.Center;
                }

                grap.DrawString(guest.AdditionalText, new Font(cardInfo.AltTextFontName, (float)(cardInfo.AltTextFontSize * 0.63 * zoomRatio))
                , new SolidBrush(ColorTranslator.FromHtml(cardInfo.AltTextFontColor))
                , new Point((int)((atXAxis * zoomRatio))
                , (int)(cardInfo.AltTextYaxis * zoomRatio)), frmt);
            }
            if (selectedValues.Contains("No. of Scan"))
            {
                double nosXAxis = (cardInfo.NosAlignment == "right") ? Convert.ToDouble(cardInfo.NosRightAxis) : Convert.ToDouble(cardInfo.Nosxaxis);


                var fNos = new Font(cardInfo.NosfontName, (float)(cardInfo.NosfontSize * 0.63 * zoomRatio));
                var stringsizeNos = grap.MeasureString(guest.AdditionalText, fNos);

                frmt = new StringFormat();
                if (cardInfo.NosAlignment == "right")
                    frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                else
                    frmt = new StringFormat();

                if (cardInfo.NosAlignment == "center")
                {
                    nosXAxis = (Convert.ToDouble(cardInfo.NosRightAxis) + Convert.ToDouble(cardInfo.Nosxaxis)) / 2;
                    frmt = new StringFormat();
                    frmt.Alignment = StringAlignment.Center;
                }
                grap.DrawString(Convert.ToString(guest.NoOfMembers), new Font(cardInfo.NosfontName, (float)(cardInfo.NosfontSize * 0.63 * zoomRatio))
             , new SolidBrush(ColorTranslator.FromHtml(cardInfo.NosfontColor))
             , new Point((int)((nosXAxis * zoomRatio))
             , (int)(cardInfo.Nosyaxis * zoomRatio)), frmt);
            }


            using (MemoryStream ms = new MemoryStream())
            {
                // Save the final image to memory stream
                myBitmap.Save(ms, ImageFormat.Jpeg);
                // Here we will upload the image to cloudianry with a filename relate to the guest
                // to make us able to retrieve it later when sending whatsapp message
                var cloudinaryFileName = $"E00000{eventId}_{guestId}_{nos}.jpg";

                var cloudinaryUrl = await _cloudinaryService.UploadImageAsync(
                    ms,
                    cloudinaryFileName,
                    $"cards/{eventId}"
                );


                //await _blobStorage.UploadAsync(ms, "jpg", environment + cardPreview + @"/" + eventId + @"/" + "E00000" + eventId + "_" + guestId + "_" + nos + ".jpg", cancellationToken: default);
            }
            grap.Dispose();
            img.Dispose();
            myBitmap.Dispose();
            barcode.Dispose();
        }
        private static void GenerateBarcode(CardInfo card, string imagePath, string barcodeText)
        {
            var url = string.Format("http://chart.apis.google.com/chart?cht=qr&chs={1}x{2}&chl={0}", barcodeText, card.BarcodeWidth, card.BarcodeWidth);
            WebResponse response = default(WebResponse);
            Stream remoteStream = default(Stream);
            StreamReader readStream = default(StreamReader);
            WebRequest request = WebRequest.Create(url);
            response = request.GetResponse();
            remoteStream = response.GetResponseStream();
            readStream = new StreamReader(remoteStream);
            Image img = Image.FromStream(remoteStream);
            img.Save(imagePath);
            response.Close();
            remoteStream.Close();
            readStream.Close();
        }
        private async Task RefreshQRCode(Guest guest, CardInfo card)
        {
            int guestId = guest.GuestId;
            int eventId = guest.EventId ?? 0;

            // Generate QR code for guest
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(EventProCrypto.EncryptString(_configuration.GetSection("SecurityKey").Value, Convert.ToString(guestId)), QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage;

            if (string.Equals(card.ForegroundColor, "#FFFFFF", StringComparison.OrdinalIgnoreCase))
            {
                qrCodeImage = qrCode.GetGraphic(
                    5,
                    ColorTranslator.FromHtml(card.BackgroundColor),
                    Color.Transparent,
                    false
                );
            }
            else
            {
                qrCodeImage = qrCode.GetGraphic(5
                    , card.BackgroundColor
                    , card.ForegroundColor
                    , false);
            }

            // Upload to Cloudinary in folder structure: QR/{eventId}/{guestId}.png
            string qrFolderPath = $"QR/{eventId}";
            string qrFileName = $"{guestId}.png";

            using (MemoryStream ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, ImageFormat.Png);
                await _cloudinaryService.UploadImageAsync(ms, qrFileName, qrFolderPath);
            }
            qrCodeImage.Dispose();
        }
       
    }
}
