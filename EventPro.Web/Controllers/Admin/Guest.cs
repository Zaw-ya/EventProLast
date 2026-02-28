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

using DocumentFormat.OpenXml.Spreadsheet;

using EventPro.Business.Helpers;
using EventPro.DAL;
using EventPro.DAL.Enum;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.Web.Common;
using EventPro.Web.Filters;

using ExcelDataReader;

using iText.Kernel.Events;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using QRCoder;

using Serilog;
namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        #region Region 1: Guest Management - List and CRUD Operations

        /// <summary>
        /// GET: Admin/Guests
        /// Displays the guest list view for a specific event
        /// Implements operator access control and loads necessary ViewBag data including event location and sending limits
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>Guests view with event context</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> Guests(int id)
        {
            // Verify operator has access to this event
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Set ViewBag data for the guest list page
            ViewBag.EventId = id;
            ViewBag.GmapCode = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
                .Select(p => p.GmapCode)
                .FirstOrDefaultAsync();

            ViewBag.sendingLimit = await db.AppSettings
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            // Determine if current user is administrator (for UI permissions)
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

        /// <summary>
        /// POST: Admin/GetGuests
        /// DataTables server-side processing endpoint for guest list
        /// Supports complex Arabic text filtering for message status (sent, delivered, read, failed, pending)
        /// Implements pagination, sorting, and comprehensive search including:
        /// - Guest ID, Name, Phone Number
        /// - Message delivery status (invitation card, location, reminder, congratulation)
        /// - Guest responses (confirmed, declined, maybe, pending)
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>JSON data for DataTables with guest records</returns>
        [AuthorizeRoles("Administrator", "Operator", "Agent", "Supervisor", "Accounting")]
        public async Task<IActionResult> GetGuests(int id)
        {
            // Verify operator access
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Parse DataTables parameters
            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            string searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString();

            var sortColumn = Request.Form[string.Concat("columns[", Request.Form["order[0][column]"], "][name]")];
            var sortColumnDirection = Request.Form["order[0][dir]"];

            // Base query: Get all non-archived guests for the event
            IQueryable<vwGuestInfo> guests = db.vwGuestInfo.Where(e => (e.EventId == id && e.GuestArchieved == false) && (
            string.IsNullOrEmpty(searchValue) ? true
            : (e.GuestId.ToString().Contains(searchValue))
            || (e.CreatedOn.ToString().Contains(searchValue))
            || (e.EventId.ToString().Contains(searchValue))
            || (string.Concat(e.FirstName, " ", e.LastName).Contains(searchValue))
            || (string.Concat("+", e.SecondaryContactNo, e.PrimaryContactNo).Contains(searchValue))
            )).AsNoTracking();

            // Complex Arabic text filtering for message delivery status
            // This section handles search terms in Arabic for different message states
            if (searchValue?.Length > 3)
            {
                // Filter for failed messages across all message types
                // Arabic: "لم ترسل الرسالة" (message not sent) or "failed" or "فشلت" (failed)
                if (searchValue.Contains("لم ترسل الرسالة") || searchValue.Contains("failed") || searchValue.Contains("فشلت") || searchValue.Contains("لم تتم الرسالة") || searchValue.Contains("رسالة فشلت ولم ترسل من الواتس اب رسالة بسبب مشكلة تقنية"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && ((
                    // Failed congratulation messages
                    ((p.ConguratulationMsgRead != true)
                && (p.ConguratulationMsgDelivered != true)
                && ((p.ConguratulationMsgSent == true && p.WatiConguratulationMsgId != null && p.TextDelivered != true && p.TextRead != true) || p.ConguratulationMsgFailed == true))) ||
                    // Failed reminder messages
                    ((p.ReminderMessageRead != true)
                && (p.ReminderMessageDelivered != true)
                && ((p.ReminderMessageSent == true && p.ReminderMessageWatiId != null && p.TextDelivered != true && p.TextRead != true) || p.ReminderMessageFailed == true)) ||
                    // Failed event location messages
                    ((p.EventLocationRead != true)
                && (p.EventLocationDelivered != true)
                && ((p.EventLocationSent == true && p.whatsappWatiEventLocationId != null && p.TextDelivered != true && p.TextRead != true) || p.EventLocationFailed == true)) ||
                    // Failed invitation card messages
                    ((p.ImgRead != true)
                && (p.ImgDelivered != true)
                && ((p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true) || p.ImgFailed == true)) ||
                    // Failed text messages
                ((p.TextRead != true)
                && (p.TextDelivered != true)
                && ((p.TextSent == true && p.whatsappMessageId != null) || p.TextFailed == true)))
                    ).AsNoTracking();
                }

                // Filter for read messages - Arabic: "قرأت الرسالة" (message read)
                if (searchValue.Contains("قرأت الرسالة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                    p.ImgRead == true))
                        .AsNoTracking();
                }

                // Filter for WhatsApp errors - "WA ERROR" or "wa error"
                if (searchValue.Contains("WA ERROR") || searchValue.Contains("wa error") || searchValue.Contains("error") || searchValue.Contains("wa") || searchValue.Contains("WA Error"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                    p.Response.Contains("WA Error")))
                        .AsNoTracking();
                }

                // Filter for delivered messages - Arabic: "وصلت الرسالة" (message delivered)
                if (searchValue.Contains("وصلت الرسالة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                    p.ImgRead != true &&
                         p.ImgDelivered == true))
                        .AsNoTracking();
                }

                // Filter for pending text messages - Arabic: "معلقة الرسالة" (message pending)
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

                // Filter for read invitation cards - Arabic: "قرأت الصورة" (image read)
                if (searchValue.Contains("قرأت الصورة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.ImgRead == true))
                        .AsNoTracking();
                }

                // Duplicate filter for delivered invitation cards (same condition as above)
                if (searchValue.Contains("قرأت الصورة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.ImgRead != true &&
                         p.ImgDelivered == true))
                        .AsNoTracking();
                }

                // Filter for pending invitation cards - Arabic: "معلقة الصورة" (image pending)
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

                // Filter for failed invitation cards - Arabic: "لم تتم الصورة" (image not completed)
                if (searchValue.Contains("لم تتم الصورة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     (p.ImgRead != true)
                && (p.ImgDelivered != true)
                && ((p.ImgSent == true && p.whatsappMessageImgId != null && p.TextDelivered != true && p.TextRead != true) || p.ImgFailed == true)))
                        .AsNoTracking();

                }

                // Filter for read event location - Arabic: "تمت قراءة الموقع" (location read)
                if (searchValue.Contains("تمت قراءة الموقع"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.EventLocationRead == true))
                        .AsNoTracking();
                }

                // Duplicate filter for delivered event location (same condition as above)
                if (searchValue.Contains("تمت قراءة الموقع"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                    p.EventLocationRead != true &&
                         p.EventLocationDelivered == true))
                        .AsNoTracking();
                }

                // Filter for pending event location - Arabic: "معلقة رسالة الموقع" (location message pending)
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

                // Filter for read reminder messages - Arabic: "قرأت رسالة التذكير" (reminder message read)
                if (searchValue.Contains("قرأت رسالة التذكير"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                      p.ReminderMessageRead == true))
                        .AsNoTracking();
                }

                // Filter for delivered reminder messages - Arabic: "وصلت رسالة التذكير" (reminder message delivered)
                if (searchValue.Contains("وصلت رسالة التذكير"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.ReminderMessageRead != true &&
                         p.ReminderMessageDelivered == true))
                        .AsNoTracking();
                }

                // Filter for pending reminder messages - Arabic: "معلقة رسالة التذكير" (reminder message pending)
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

                // Filter for failed reminder messages - Arabic: "لم ترسل رسالة التذكير" (reminder message not sent)
                if (searchValue.Contains("لم ترسل رسالة التذكير"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     (p.ReminderMessageRead != true)
                && (p.ReminderMessageDelivered != true)
                && ((p.ReminderMessageSent == true && p.ReminderMessageWatiId != null && p.TextDelivered != true && p.TextRead != true) || p.ReminderMessageFailed == true)))
                        .AsNoTracking();
                }

                // Filter for read congratulation messages - Arabic: "قرأت رسالة التهنئة" (congratulation message read)
                if (searchValue.Contains("قرأت رسالة التهنئة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                      p.ConguratulationMsgRead == true))
                        .AsNoTracking();
                }

                // Filter for delivered congratulation messages - Arabic: "وصلت رسالة التهنئة" (congratulation message delivered)
                if (searchValue.Contains("وصلت رسالة التهنئة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     p.ConguratulationMsgRead != true &&
                         p.ConguratulationMsgDelivered == true))
                        .AsNoTracking();
                }

                // Filter for pending congratulation messages - Arabic: "معلقة رسالة التهنئة" (congratulation message pending)
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

                // Filter for failed congratulation messages - Arabic: "لم ترسل رسالة التهنئة" (congratulation message not sent)
                if (searchValue.Contains("لم ترسل رسالة التهنئة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     (p.ConguratulationMsgRead != true)
                && (p.ConguratulationMsgDelivered != true)
                && ((p.ConguratulationMsgSent == true && p.WatiConguratulationMsgId != null && p.TextDelivered != true && p.TextRead != true) || p.ConguratulationMsgFailed == true)))
                        .AsNoTracking();
                }

                // Filter for declined responses - Arabic: "رفضوا" or "لم يتمكن بحضور الحفلة" (declined/unable to attend)
                if (searchValue.Contains("رفضوا") || searchValue.Contains("لم يتمكن بحضور الحفلة"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     (p.Response.Equals("Decline") ||
                        (p.Response.Equals("اعتذار عن الحضور")))))
                        .AsNoTracking();
                }

                // Filter for confirmed responses - Arabic: "حضور" (attendance)
                if (searchValue.Contains("حضور"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) && (
                     (p.Response.Equals("Confirm") ||
                        (p.Response.Equals("تأكيد الحضور")))))
                        .AsNoTracking();

                }

                // Filter for "maybe" responses - Arabic: "ربما" (maybe)
                if (searchValue.Contains("ربما"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) &&
                        (p.Response.Equals("ربما")))
                        .AsNoTracking();
                }

                // Filter for pending responses - Arabic: "تحت الانتظار" (under waiting/pending)
                if (searchValue.Contains("تحت الانتظار"))
                {
                    guests = db.vwGuestInfo.Where(p => (p.EventId == id) &&
                        (p.Response.Equals("تحت الانتظار")))
                        .AsNoTracking();
                }

                // Filter for sent messages - Arabic: "مرسلة" (sent)
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

            // Apply sorting and pagination
            int recordsTotal = 0;
            guests = guests.OrderByDescending(e => e.GuestId);
            recordsTotal = await guests.CountAsync();

            var result = new List<vwGuestInfo>();
            try
            {
                // Handle "show all" case where pageSize is -1
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

            // Map to view model
            List<GuestVM> guestsVM = new();

            foreach (var guest in result)
            {
                GuestVM guestVM = new(guest);
                guestsVM.Add(guestVM);
            }

            // Return DataTables format
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = guestsVM
            };

            return Ok(jsonData);
        }

        /// <summary>
        /// POST: Admin/Guest
        /// Adds or updates a guest record
        /// Calls AddOrModifyGuest for upsert logic
        /// </summary>
        /// <param name="guest">Guest object with data to add or update</param>
        /// <returns>JSON result with success status and operation type (1=added, 2=modified)</returns>
        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> Guest(Guest guest)
        {
            var addedOrModified = 0;
            var userId = HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation(
                    "Guest POST started | GuestId={GuestId} | EventId={EventId} | UserId={UserId}",
                    guest?.GuestId,
                    guest?.EventId,
                    userId
                );

            try
            {
                addedOrModified = await AddOrModifyGuest(guest);

                _logger.LogInformation(
                    "Guest POST succeeded | GuestId={GuestId} | Result={Result}",
                    guest.GuestId,
                    addedOrModified
                );

                return Json(new { success = true, addedOrModified });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Guest POST failed | GuestId={GuestId} | EventId={EventId} | UserId={UserId}",
                    guest?.GuestId,
                    guest?.EventId,
                    userId
                );

                return Json(new { success = false, message = "Unexpected error occurred" });
            }
        }

        /// <summary>
        /// Helper method for adding or modifying guest records (Upsert operation)
        /// If GuestId > 0: Updates existing guest
        /// If GuestId = 0: Creates new guest
        /// After save, refreshes guest QR code and invitation card with updated data
        /// Logs audit trail for guest creation/modification
        /// </summary>
        /// <param name="guest">Guest object to add or modify</param>
        /// <returns>1 if added, 2 if modified</returns>
        public async Task<int> AddOrModifyGuest(Guest guest)
        {
            var addedOrModified = 0;
            var eventId = 0;
            var guestId = 0;

            // Guest come from form without EventId (data leakage), so we need to fetch it
            var gst = await db.Guest.FirstOrDefaultAsync(p => p.GuestId == guest.GuestId);
            // we have to check add or modify based on GuestId to decided fetch or not
            if (guest.GuestId != 0)
            {
                // modify - means fetch guest existing record
                eventId = gst.EventId.Value;
                guestId = guest.GuestId;
            }
            else
            {
                // add - means take event id from guest object comming from form
                eventId = guest.EventId.Value;
            }

            // Get configuration paths for card generation
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            string guestcode = _configuration.GetSection("Uploads").GetSection("Guestcode").Value;
            string path = _configuration.GetSection("Uploads").GetSection("Card").Value;
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            _logger.LogInformation(
                "AddOrModifyGuest started | GuestId={GuestId} | EventId={EventId} | UserId={UserId}",
                guestId, eventId, userId
            );

            try
            {
                if (guest.GuestId > 0)
                {
                    _logger.LogInformation("Updating guest | GuestId={GuestId}", guest.GuestId);

                    if (gst == null)
                    {
                        _logger.LogWarning("Guest not found | GuestId={GuestId}", guest.GuestId);
                        throw new Exception("Guest not found");
                    }

                    // Normalize country code (stored in SecondaryContactNo)
                    var normalizedCountryCode = PhoneNumberHelper.NormalizeCountryCode(guest.SecondaryContactNo);

                    // Normalize and validate primary phone number
                    var phoneValidation = PhoneNumberHelper.ValidateAndNormalize(guest.PrimaryContactNo, normalizedCountryCode);

                    gst.FirstName = guest.FirstName;
                    gst.NoOfMembers = guest.NoOfMembers;
                    gst.AdditionalText = guest.AdditionalText;
                    gst.PrimaryContactNo = phoneValidation.NormalizedPhoneNumber;
                    gst.SecondaryContactNo = phoneValidation.NormalizedCountryCode;
                    gst.IsPhoneNumberValid = phoneValidation.IsValid;
                    gst.GuestArchieved = false;

                    guest.Cypertext = EventProCrypto.EncryptString(
                        _configuration["SecurityKey"],
                        guest.GuestId.ToString()
                    );

                    
                    await _auditLogService.AddAsync(userId, eventId, ActionEnum.UpdateGuest, guestId, gst.FirstName);
                    await db.SaveChangesAsync();

                    addedOrModified = 2;
                }
                else
                {
                    _logger.LogInformation("Creating new guest | EventId={EventId}", eventId);

                    // Normalize country code and phone number for new guest
                    var newGuestCountryCode = PhoneNumberHelper.NormalizeCountryCode(guest.SecondaryContactNo);
                    var newGuestPhoneValidation = PhoneNumberHelper.ValidateAndNormalize(guest.PrimaryContactNo, newGuestCountryCode);

                    guest.PrimaryContactNo = newGuestPhoneValidation.NormalizedPhoneNumber;
                    guest.SecondaryContactNo = newGuestPhoneValidation.NormalizedCountryCode;
                    guest.CreatedBy = userId;
                    guest.CreatedOn = DateTime.Now;
                    guest.IsPhoneNumberValid = newGuestPhoneValidation.IsValid;
                    guest.GuestArchieved = false;
                    guest.Source = "Entry";

                    var newGuest = await db.Guest.AddAsync(guest);
                    await db.SaveChangesAsync();

                    guest = newGuest.Entity;
                    guestId = guest.GuestId;

                    guest.Cypertext = EventProCrypto.EncryptString(
                        _configuration["SecurityKey"],
                        guestId.ToString()
                    );

                    await _auditLogService.AddAsync(userId, eventId, ActionEnum.AddGuest, guestId, guest.FirstName);

                    addedOrModified = 1;
                }

                _logger.LogInformation(
                    "Guest saved successfully | GuestId={GuestId} | Mode={Mode}",
                    guestId, addedOrModified == 1 ? "Add" : "Update"
                );

                var cardinfo = await db.CardInfo.FirstOrDefaultAsync(p => p.EventId == eventId);
                if (cardinfo == null)
                {
                    _logger.LogWarning("CardInfo not found | EventId={EventId}", eventId);
                }

                //await RefreshQRCode(guest, cardinfo);
                //await RefreshCard(guest, eventId, cardinfo, cardPreview, guestcode, path);
                await db.SaveChangesAsync();

                return addedOrModified;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "AddOrModifyGuest failed | GuestId={GuestId} | EventId={EventId} | UserId={UserId}",
                    guestId, eventId, userId
                );

                throw ex;
            }
        }

        /// <summary>
        /// GET: Admin/DeleteGuest
        /// Deletes a guest record and associated files (invitation card from blob storage)
        /// Logs audit trail for guest deletion
        /// </summary>
        /// <param name="id">Guest ID to delete</param>
        /// <returns>Redirect to guests list with success message</returns>
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
            await _blobStorage.DeleteFileAsync(environment + cardPreview + @"/" + eventId + @"/" + "E00000" + eventId + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", cancellationToken: default);

            TempData["error"] = "Guest information deleted successfully!";

            return RedirectToAction("Guests", "admin", new { id = eventId });
        }


        //[AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        //public async Task<IActionResult> DeleteGuest(int id)
        //{
        //    var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var guest = await db.Guest.Where(p => p.GuestId == id)
        //                .AsNoTracking()
        //                .FirstOrDefaultAsync();
        //    int eventId = Convert.ToInt32(guest.EventId);
        //    db.Guest.Remove(guest);
        //    await db.SaveChangesAsync();
        //    await _auditLogService.AddAsync(userId, eventId, ActionEnum.DeleteGuest, id, guest.FirstName);
        //    Log.Information("Event {eId} guest {gId} removed by {uId}", eventId, guest.GuestId, userId);

        //    // Delete guest invitation card from blob storage
        //    string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
        //    string environment = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
        //    await _blobStorage.DeleteFileAsync(environment + cardPreview + @"/" + eventId + @"/" + "E00000" + eventId + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", cancellationToken: default);

        //    TempData["error"] = "Guest information deleted successfully!";

        //    return RedirectToAction("Guests", "admin", new { id = eventId });
        //}

        #endregion

        #region Region 2: WhatsApp Template Selection

        /// <summary>
        /// Selects and sends the appropriate WhatsApp invitation template based on event configuration
        /// Supports multiple template types:
        /// - English templates (with/without header text/image)
        /// - Arabic Male/Female templates (with/without header text/image)
        /// - Work Invitation templates
        /// - Custom Invitation templates (with/without client name)
        /// - QR-Invitation templates
        /// Routes to appropriate WATI service method based on template configuration
        /// </summary>
        /// <param name="guest">Guest to send template to</param>
        /// <param name="evt">Event containing template configuration</param>
        /// <returns>Message ID or error message</returns>
        public async Task<string> sendSelectedTemplate(Guest guest, Events evt)
        {
            // English templates
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
            // Work invitation templates
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
            // Custom invitation templates
            else if (evt.MessageLanguage == "Custom-Invitation")
            {
                return await _watiService.SendCustomInvitaionTemplate(guest, evt);
            }
            else if (evt.MessageLanguage == "Custom-Invitation with client name")
            {
                return await _watiService.SendCustomInvitaionWithClientNameTemplate(guest, evt);
            }
            // QR invitation templates
            else if (evt.MessageLanguage == "QR-Invitation")
            {
                return await _watiService.SendQRInvitaionTemplate(guest, evt);
            }
            // Arabic Male templates
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
            // Arabic Female templates
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
            // English without parent title
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
            // Arabic with gender-based template selection
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

        #endregion

        #region Region 3: Bulk Messaging - Invitation Cards

        /// <summary>
        /// GET: Admin/GetCardSendingData
        /// Returns progress data for bulk invitation card sending operation
        /// Uses memory cache to track sending progress
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="remainingMessages">Number of remaining messages to send</param>
        /// <returns>JSON with sent and remaining message counts</returns>
        public async Task<IActionResult> GetCardSendingData(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                // Initial request - calculate total remaining messages
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
                // Progress update - retrieve from memory cache
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());
                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        /// <summary>
        /// POST: Admin/SendCardToAll
        /// Sends invitation cards (with QR codes) to all guests who haven't received them yet
        /// Implements bulk sending with rate limiting based on AppSettings.BulkSendingLimit
        /// Pauses WebHook consumer during bulk send to prevent conflicts
        /// Uses memory cache to track progress for UI updates
        /// Validates: operator access, guest phone numbers, card existence, and consumer state
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>JSON result with success status and error message if applicable</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendCardToAll(int id)
        {
            // Verify operator access
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Get bulk sending limit from app settings
            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            // Get guests who haven't received invitation cards yet (limited by bulk sending limit)
            var guests = await db.Guest
                .Where(p => p.EventId == id && (p.ImgSentMsgId == null))
                .Take(sendingBulkLimit)
                .ToListAsync();

            var _event = await db.Events
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (_event.WhatsappPush == false || _event.WhatsappPush == null)
                return Json(new { success = false, message = "خيار Send WhatsApp Push غير مُفعل لهذا الحدث" });

            // Validate sending operation is not already in progress
            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            // Validate guest phone numbers exist
            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "يجب وجود رقم احتياطي واساسي لكل ضيف من الضيوف" });

            // Validate invitation cards exist in blob storage
            var guestsWithoutCards = await GetGuestsWithoutCardsAsync(guests, _event);
            if (guestsWithoutCards.Any())
            {
                var names = string.Join("، ", guestsWithoutCards);
                return Json(new
                {
                    success = false,
                    message = $"الضيوف التالية ليس لديهم بطاقات دعوة:\n{names}"
                });
            }
            // Validate webhook consumer is in valid state for bulk sending
            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                // Pause webhook consumer to prevent conflicts during bulk send
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
                // Resume webhook consumer and clear progress cache
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });
        }

        /// <summary>
        /// POST: Admin/SendQRCode
        /// Sends invitation card with QR code to a single guest
        /// Validates guest phone number and card existence before sending
        /// </summary>
        /// <param name="id">Guest ID</param>
        /// <returns>JSON result with success status</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendQRCode(int id)
        {
            _logger.LogInformation("SendQRCode started for GuestId {GuestId}", id);

            Guest guest = await db.Guest.Where(p => p.GuestId == id)
                .FirstOrDefaultAsync();
            
            if (guest == null)
            {
                _logger.LogWarning("Guest not found with GuestId {GuestId}", id);
                return Json(new { success = false, message = "الضيف غير موجود" });
            }
            
            var guests = new List<Guest>() { guest };

            var _event = await db.Events.Where(p => p.Id == guest.EventId)
                .FirstOrDefaultAsync();

            if (_event == null)
            {
                _logger.LogWarning(
                    "Event not found for GuestId {GuestId}, EventId {EventId}",
                    guest.GuestId,
                    guest.EventId
                );
                return Json(new { success = false, message = "الحدث غير موجود" });
            }

            _logger.LogInformation(
                "GuestId {GuestId} belongs to EventId {EventId}",
                guest.GuestId,
                _event.Id
            );

            // Validate guest phone number
            if (!CheckGuestsNumbersExist(guests))
            {
                _logger.LogWarning("Phone number validation failed for GuestId {GuestId}", guest.GuestId);
                return Json(new { success = false, message = "رقم الجوال غير موجود" });
            }

            // Validate invitation card exists
            var guestsWithoutCards = await GetGuestsWithoutCardsAsync(guests, _event);
            if (guestsWithoutCards.Any())
            {
                var names = string.Join("، ", guestsWithoutCards);
                return Json(new { success = false, message = $"الضيف التالي ليس لديه بطاقة دعوة: {names}" });
            }

            try
            {
                var sendingProvider = await _WhatsappSendingProvider.SelectConfiguredSendingProviderAsync(_event);
                if (sendingProvider == null)
                {
                    _logger.LogError(
                        "Sending provider is NULL for EventId {EventId}",
                        _event.Id
                    );
                    return Json(new { success = false, message = "مزود الإرسال غير مُعد" });
                }

                _logger.LogInformation(
                    "Sending provider selected successfully for EventId {EventId}",
                    _event.Id
                );

                await sendingProvider.SendCardMessagesAsync(guests, _event);

                _logger.LogInformation(
                    "QRCode sent successfully to GuestId {GuestId}",
                    guest.GuestId
                );

                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                _logger.LogError(
                            ex,
                            "Error while sending QRCode to GuestId {GuestId}, EventId {EventId}",
                            guest.GuestId,
                            _event.Id
                        );

                return Json(new { success = false, message = "حدث خطأ أثناء الإرسال" });
            }

        }

        #endregion

        #region Region 4: Bulk Messaging - Event Location

        /// <summary>
        /// GET: Admin/GetEventLocationSendingData
        /// Returns progress data for bulk event location sending operation
        /// Uses memory cache to track sending progress
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="remainingMessages">Number of remaining messages to send</param>
        /// <returns>JSON with sent and remaining message counts</returns>
        public async Task<IActionResult> GetEventLocationSendingData(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                // Initial request - calculate total remaining messages
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
                // Progress update - retrieve from memory cache
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        /// <summary>
        /// POST: Admin/SendEventLocationToAll
        /// Sends event location (Google Maps link) to all guests who haven't received it yet
        /// Implements bulk sending with rate limiting based on AppSettings.BulkSendingLimit
        /// Pauses WebHook consumer during bulk send to prevent conflicts
        /// Uses memory cache to track progress for UI updates
        /// Validates: operator access, event location exists, guest phone numbers, and consumer state
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>JSON result with success status and error message if applicable</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendEventLocationToAll(int id)
        {
            // Verify operator access
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Get bulk sending limit
            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            // Get guests who haven't received event location yet
            var guests = await db.Guest.Where(p => p.EventId == id &&
            (p.waMessageEventLocationForSendingToAll == null))
                .Take(sendingBulkLimit)
                .ToListAsync();

            var _event = await db.Events
                .Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Validate operation not already in progress
            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            // Validate event location exists
            if (!CheckEventLocationExists(_event))
                return Json(new { success = false, message = "رابط الموقع غير موجود" });

            // Validate guest phone numbers
            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            // Validate webhook consumer state
            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                // Pause webhook consumer during bulk send
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
                // Resume webhook consumer and clear progress cache
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });
        }

        /// <summary>
        /// POST: Admin/SendEventLocation
        /// Sends event location to a single guest
        /// Validates event location exists and guest phone number before sending
        /// </summary>
        /// <param name="id">Guest ID</param>
        /// <returns>JSON result with success status</returns>
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

            // Validate event location exists
            if (!CheckEventLocationExists(_event))
                return Json(new { success = false, message = "رابط الموقع غير موجود" });

            // Validate guest phone number
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

            return Json(new { success = true });
        }

        #endregion

        #region Region 5: Bulk Messaging - Reminder Messages (All Guests)

        /// <summary>
        /// GET: Admin/GetReminderMessagesSendingDataToAll
        /// Returns progress data for bulk reminder message sending operation (all guests)
        /// Uses memory cache to track sending progress
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="remainingMessages">Number of remaining messages to send</param>
        /// <returns>JSON with sent and remaining message counts</returns>
        public async Task<IActionResult> GetReminderMessagesSendingDataToAll(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                // Initial request - calculate total remaining messages
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
                // Progress update - retrieve from memory cache
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        /// <summary>
        /// POST: Admin/SendReminderMessageToAll
        /// Sends reminder messages to all guests who haven't received them yet
        /// Excludes guests who declined attendance (Response = "Decline" or "اعتذار عن الحضور")
        /// Implements bulk sending with rate limiting based on AppSettings.BulkSendingLimit
        /// Pauses WebHook consumer during bulk send to prevent conflicts
        /// Uses memory cache to track progress for UI updates
        /// Validates: operator access, guest phone numbers, and consumer state
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>JSON result with success status and error message if applicable</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendReminderMessageToAll(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Verify operator access
            if (HasOperatorRole())
            {
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Get bulk sending limit
            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            // Get guests who haven't received reminder and haven't declined
            var guests = await db.Guest.Where(p => p.EventId == id &&
            (p.ReminderMessageId == null) &&
            (!p.Response.Equals("Decline")) &&
            (!p.Response.Equals("اعتذار عن الحضور")))
                .Take(sendingBulkLimit).ToListAsync();

            var _event = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Validate operation not already in progress
            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            // Validate guest phone numbers
            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            // Validate webhook consumer state
            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                // Pause webhook consumer during bulk send
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
                // Resume webhook consumer and clear progress cache
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });
        }

        #endregion

        #region Region 6: Bulk Messaging - Reminder Messages (Received Only)

        /// <summary>
        /// POST: Admin/SendReminderMessageToOnlyReceived
        /// Sends reminder messages only to guests who received the initial message
        /// Filters guests based on WhatsApp confirmation vs Push notification settings
        /// Excludes guests who declined attendance
        /// Implements bulk sending with rate limiting and webhook consumer pause
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>JSON result with success status and error message if applicable</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendReminderMessageToOnlyReceived(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Verify operator access
            if (HasOperatorRole())
            {
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Get bulk sending limit
            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            var _event = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
               .FirstOrDefaultAsync();

            var guests = new List<Guest>();

            // Filter based on event WhatsApp confirmation setting
            if (_event.WhatsappConfirmation == true)
            {
                // Get guests who received confirmation message (MessageId exists) and haven't declined
                guests = await db.Guest.Where(p => p.EventId == id && p.MessageId != null && p.TextFailed != true &&
                           (p.ReminderMessageId == null) &&
                           (!p.Response.Equals("Decline")) &&
                           (!p.Response.Equals("اعتذار عن الحضور")))
                            .Take(sendingBulkLimit).ToListAsync();
            }
            else if (_event.WhatsappPush == true)
            {
                // Get guests who received invitation card (ImgSentMsgId exists) and haven't declined
                guests = await db.Guest.Where(p => p.EventId == id && p.ImgSentMsgId != null && p.ImgFailed != true &&
                            (p.ReminderMessageId == null) &&
                            (!p.Response.Equals("Decline")) &&
                            (!p.Response.Equals("اعتذار عن الحضور")))
                             .Take(sendingBulkLimit).ToListAsync();
            }

            // Validate operation not already in progress
            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            // Validate guest phone numbers
            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            // Validate webhook consumer state
            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                // Pause webhook consumer during bulk send
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
                // Resume webhook consumer and clear progress cache
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });
        }

        #endregion

        #region Region 7: Bulk Messaging - Reminder Messages (Accepted Only)

        /// <summary>
        /// GET: Admin/GetReminderMessagesSendingDataToOnlyAccepted
        /// Returns progress data for bulk reminder message sending to accepted guests only
        /// Counts only guests who confirmed attendance (Response = "Confirm" or "تأكيد الحضور")
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="remainingMessages">Number of remaining messages to send</param>
        /// <returns>JSON with sent and remaining message counts</returns>
        public async Task<IActionResult> GetReminderMessagesSendingDataToOnlyAccepted(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                // Initial request - calculate total remaining messages for accepted guests
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
                // Progress update - retrieve from memory cache
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        /// <summary>
        /// POST: Admin/SendReminderMessageToOnlyAccepted
        /// Sends reminder messages only to guests who confirmed attendance
        /// Filters guests with Response = "Confirm" or "تأكيد الحضور"
        /// Implements bulk sending with rate limiting and webhook consumer pause
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>JSON result with success status and error message if applicable</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendReminderMessageToOnlyAccepted(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Verify operator access
            if (HasOperatorRole())
            {
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Get bulk sending limit
            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            // Get guests who confirmed attendance and haven't received reminder
            var guests = await db.Guest.Where(p => p.EventId == id && (p.ReminderMessageId == null) &&
                        ((p.Response.Equals("Confirm")) ||
                        (p.Response.Equals("تأكيد الحضور"))))
                        .Take(sendingBulkLimit).ToListAsync();

            var _event = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Validate operation not already in progress
            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            // Validate guest phone numbers
            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            // Validate webhook consumer state
            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                // Pause webhook consumer during bulk send
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
                // Resume webhook consumer and clear progress cache
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });
        }

        #endregion

        #region Region 8: Bulk Messaging - Reminder Messages (No Answer)

        /// <summary>
        /// GET: Admin/GetReminderMessagesSendingDataToNoAnswer
        /// Returns progress data for bulk reminder message sending to guests with no answer
        /// Counts only guests with Response = "Message Processed Successfully" (no response yet)
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="remainingMessages">Number of remaining messages to send</param>
        /// <returns>JSON with sent and remaining message counts</returns>
        public async Task<IActionResult> GetReminderMessagesSendingDataToNoAnswer(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                // Initial request - calculate total remaining messages for no-answer guests
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
                // Progress update - retrieve from memory cache
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        /// <summary>
        /// POST: Admin/SendReminderMessageToOnlyNoAnswer
        /// Sends reminder messages only to guests who haven't responded yet
        /// Filters guests with Response = "Message Processed Successfully"
        /// Implements bulk sending with rate limiting and webhook consumer pause
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>JSON result with success status and error message if applicable</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendReminderMessageToOnlyNoAnswer(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Verify operator access
            if (HasOperatorRole())
            {
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Get bulk sending limit
            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            // Get guests with no response and haven't received reminder
            var guests = await db.Guest.Where(p => p.EventId == id && (p.ReminderMessageId == null) &&
                        (p.Response.Equals("Message Processed Successfully")))
                        .Take(sendingBulkLimit).ToListAsync();

            var _event = await db.Events.Where(p => p.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Validate operation not already in progress
            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            // Validate guest phone numbers
            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            // Validate webhook consumer state
            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                // Pause webhook consumer during bulk send
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
                // Resume webhook consumer and clear progress cache
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });
        }

        #endregion

        #region Region 9: Bulk Messaging - Confirmation and Congratulation

        /// <summary>
        /// GET: Admin/GetConfirmationSendingData
        /// Returns progress data for bulk confirmation message sending operation
        /// Uses memory cache to track sending progress
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="remainingMessages">Number of remaining messages to send</param>
        /// <returns>JSON with sent and remaining message counts</returns>
        public async Task<IActionResult> GetConfirmationSendingData(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                // Initial request - calculate total remaining messages
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
                // Progress update - retrieve from memory cache
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        /// <summary>
        /// POST: Admin/SendToAll
        /// Sends confirmation messages (invitation with confirmation buttons) to all guests
        /// Validates event configuration (location, card text for link-based confirmations)
        /// Implements bulk sending with rate limiting and webhook consumer pause
        /// Used for events with interactive confirmation buttons (Confirm/Decline)
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>JSON result with success status and error message if applicable</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendToAll(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Verify operator access
            if (HasOperatorRole())
            {
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Get bulk sending limit
            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            // Get guests who haven't received confirmation message
            var guests = await db.Guest
                .Where(p => p.EventId == id && (p.MessageId == null))
                .Take(sendingBulkLimit)
                .ToListAsync();

            var _event = await db.Events.Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (_event.WhatsappConfirmation == false || _event.WhatsappConfirmation == null)
                return Json(new { success = false, message = "خيار Send Whatsapp Confirmation غير مُفعل لهذا الحدث" });

            // Validate operation not already in progress
            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            // For link-based confirmation buttons, validate required fields
            if (_event.ConfirmationButtonsType == "Links")
            {
                if (string.IsNullOrEmpty(_event.LinkGuestsLocationEmbedSrc))
                    return Json(new { success = false, message = "رابط الموقع غير موجود" });

                if (string.IsNullOrEmpty(_event.LinkGuestsCardText))
                    return Json(new { success = false, message = "نص الرسالة غير موجود" });
            }

            // Validate event location exists
            if (!CheckEventLocationExists(_event))
                return Json(new { success = false, message = "رابط الموقع غير موجود" });

            // Validate guest phone numbers
            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            // Validate invitation cards exist
            if (!await CheckGuestsCardsExistAsync(guests, _event))
                return Json(new { success = false, message = "صورة البطاقة غير موجودة" });

            // Validate webhook consumer state
            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                // Pause webhook consumer during bulk send
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
                // Resume webhook consumer and clear progress cache
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });

        }

        /// <summary>
        /// GET: Admin/GetCongratulationSendingData
        /// Returns progress data for bulk congratulation message sending operation
        /// Counts only guests who attended the event (have scan history with "Allowed" response)
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="remainingMessages">Number of remaining messages to send</param>
        /// <returns>JSON with sent and remaining message counts</returns>
        public async Task<IActionResult> GetCongratulationSendingData(int id, int remainingMessages)
        {
            if (remainingMessages == 0)
            {
                // Initial request - calculate total remaining messages for attended guests
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
                // Progress update - retrieve from memory cache
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { sentMessages = sentMessagesCount, remainingMessages = remainingMessages });
            }
        }

        /// <summary>
        /// POST: Admin/SendCongratulationMessageToAll
        /// Sends congratulation/thank you messages to guests who attended the event
        /// Only sends to guests with scan history (ResponseCode = "Allowed")
        /// Validates congratulation message phone number is configured
        /// Implements bulk sending with rate limiting and webhook consumer pause
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>JSON result with success status and error message if applicable</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendCongratulationMessageToAll(int id)
        {
            // Verify operator access
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Get bulk sending limit
            var sendingBulkLimit = await db.AppSettings
                .AsNoTracking()
                .Select(e => e.BulkSendingLimit)
                .FirstOrDefaultAsync();

            // Get guests who attended (have allowed scan history) and haven't received congratulation message
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

            // Validate operation not already in progress
            if (_MemoryCacheStoreService.IsExist(id.ToString()))
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود بصورة صحيحة" });

            // Validate congratulation message phone number is configured
            if (_event.ConguratulationsMsgSentOnNumber == null)
                return Json(new { success = false, message = "نسبة الضيف لارسل رقم الجوال الذي سيتم إرسال رسالة تأكيد الحضور منه غير محدد" });

            // Validate guest phone numbers
            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "بطاقة الضيوف غير موجودة" });

            // Validate webhook consumer state
            if (!_WebHookQueueConsumerService.IsValidSendingBulkMessages())
                return Json(new { success = false, message = "حدث خطأ فادح ، رابط الموقع غير موجود" });

            try
            {
                // Pause webhook consumer during bulk send
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
                // Resume webhook consumer and clear progress cache
                _WebHookQueueConsumerService.Resume();
                _MemoryCacheStoreService.delete(id.ToString());
            }

            return Json(new { success = true });

        }

        #endregion

        #region Region 10: Individual Messaging Operations

        /// <summary>
        /// POST: Admin/InviteOnWhatsapp
        /// Sends invitation with confirmation buttons to a single guest
        /// For link-based confirmations, validates location and card text exist
        /// Validates event location, guest phone number, and invitation card exist
        /// </summary>
        /// <param name="id">Guest ID</param>
        /// <returns>JSON result with success status and error message if applicable</returns>
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

            // For link-based confirmation buttons, validate required fields
            if (_event.ConfirmationButtonsType == "Links")
            {
                if (string.IsNullOrEmpty(_event.LinkGuestsLocationEmbedSrc))
                    return Json(new { success = false, message = "رابط الموقع غير موجود" });

                if (string.IsNullOrEmpty(_event.LinkGuestsCardText))
                    return Json(new { success = false, message = "نص الرسالة غير موجود" });
            }

            // Validate event location exists
            if (!CheckEventLocationExists(_event))
                return Json(new { success = false, message = "رابط الموقع غير موجود" });

            // Validate guest phone number
            if (!CheckGuestsNumbersExist(guests))
                return Json(new { success = false, message = "رقم الجوال غير موجود" });

            // Validate invitation card exists
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

        /// <summary>
        /// POST: Admin/SendReminderMessage
        /// Sends reminder message to a single guest
        /// Validates guest phone number before sending
        /// </summary>
        /// <param name="id">Guest ID</param>
        /// <returns>JSON result with success status</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SendReminderMessage(int id)
        {
            var guest = await db.Guest
                .FirstOrDefaultAsync(p => p.GuestId == id);

            var _event = await db.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == guest.EventId);
            var guests = new List<Guest> { guest };

            // Validate guest phone number
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

        /// <summary>
        /// POST: Admin/sendConguratilationMessage
        /// Sends congratulation/thank you message to a single guest
        /// Validates congratulation message phone number is configured
        /// Validates guest phone number before sending
        /// </summary>
        /// <param name="id">Guest ID</param>
        /// <returns>JSON result with success status</returns>
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

            // Validate congratulation message phone number is configured
            if (_event.ConguratulationsMsgSentOnNumber == null)
                return Json(new { success = false, message = "نسبة الضيف لارسل رقم الجوال الذي سيتم إرسال رسالة تأكيد الحضور منه غير محدد" });

            // Validate guest phone number
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

            return Json(new { success = true });
        }

        #endregion

        #region Region 11: Guest Response Management

        /// <summary>
        /// POST: Admin/SetDeclineResponse
        /// Manually sets guest response to "Decline"
        /// Administrator-only function for manual guest response management
        /// </summary>
        /// <param name="id">Guest ID</param>
        /// <returns>OK result</returns>
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

        /// <summary>
        /// POST: Admin/SetAcceptResponse
        /// Manually sets guest response to "Confirm"
        /// If MessageId is empty, sets it to "not null" to mark guest as contacted
        /// Used for manual guest response management
        /// </summary>
        /// <param name="id">Guest ID</param>
        /// <returns>OK result</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> SetAcceptResponse(int id)
        {
            string _yesText_Eng = _configuration.GetSection("PinacleSettings").GetSection("TextYes_Eng").Value;
            var guest = await db.Guest.FirstOrDefaultAsync(p => p.GuestId == id);
            guest.Response = _yesText_Eng;

            // If guest hasn't been contacted yet, mark as contacted
            if (string.IsNullOrEmpty(guest.MessageId))
            {
                guest.MessageId = "not null";
            }
            db.Guest.Update(guest);
            await db.SaveChangesAsync();
            return Ok();
        }

        #endregion

        #region Region 12: Guest Import and Excel Upload

        /// <summary>
        /// POST: Admin/Guests (Excel Import - Legacy)
        /// Legacy Excel import method using OleDb
        /// Note: This method appears to be unused in favor of the Upload method
        /// Uploads Excel file and imports guest data
        /// </summary>
        /// <param name="guest">Guest object with EventId</param>
        /// <returns>Redirect to guests list</returns>
        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public IActionResult Guests(Guest guest)
        {
            var files = Request.Form.Files;
            string path = _configuration.GetSection("Uploads").GetSection("Excel").Value;
            string excelConnection = _configuration.GetSection("Database").GetSection("ExcelConnection").Value;
            string filename = string.Empty;
            bool hasFile = false;

            // Save uploaded Excel file
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

            // Import data from Excel
            if (hasFile)
            {
                var excelData = ImportFromExcel.ImportDataFromExcel(path + @"\" + filename, excelConnection);
            }
            return RedirectToAction("Guests", "admin", new { id = guest.EventId });
        }

        /// <summary>
        /// POST: Admin/Upload
        /// Imports guest data from Excel file (.xlsx or .xls)
        /// Validates operator access and file format
        /// Uploads file to Azure Blob Storage for processing
        /// Extracts guest data (Name, Members, Country Code, Contact No, Additional Text)
        /// Creates guest records with encrypted IDs for QR code generation
        /// Logs audit trail for bulk guest upload
        /// Returns total guests and members imported
        /// </summary>
        /// <param name="eventId">Event ID to import guests for</param>
        /// <param name="form">Form collection containing uploaded file</param>
        /// <returns>JSON result with success status and import statistics</returns>
        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> Upload(int eventId, IFormCollection form)
        {
            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Verify operator access
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

                    // Validate file format
                    if (filename.ToLower().EndsWith(".xlsx") || filename.ToLower().EndsWith(".xls"))
                    {
                        // Upload to blob storage
                        using var stream = file.OpenReadStream();
                        await _blobStorage.UploadAsync(stream, "xlsx", environment + path + "/" + filename, cancellationToken: default);
                        DataSet data;

                        // Read Excel file using ExcelDataReader
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

                        // Process each row in the Excel file
                        foreach (DataRow row in data.Tables[0].Rows)
                        {
                            // Validate required fields exist
                            if (row["Members"].ToString().Length > 0 &&
                                row["GuestName"] != null)

                            {
                                // Read raw values as strings to preserve leading zeros
                                var rawCountryCode = row["CountryCode"]?.ToString()?.Trim() ?? "";
                                var rawContactNo = row["ContactNo"]?.ToString()?.Trim() ?? "";

                                // Normalize and validate phone number
                                var normalizedCountryCode = PhoneNumberHelper.NormalizeCountryCode(rawCountryCode);
                                var phoneValidation = PhoneNumberHelper.ValidateAndNormalize(rawContactNo, normalizedCountryCode);

                                Guest guest = new Guest();
                                guest.FirstName = Convert.ToString(row["GuestName"]);
                                guest.AdditionalText = Convert.ToString(row["AdditionalText"]);
                                guest.NoOfMembers = Convert.ToInt32(row["Members"]);
                                guest.SecondaryContactNo = phoneValidation.NormalizedCountryCode;
                                guest.PrimaryContactNo = phoneValidation.NormalizedPhoneNumber;
                                guest.CreatedBy = userId;
                                guest.CreatedOn = DateTime.UtcNow;
                                guest.EventId = eventId;
                                guest.Source = "Upload";
                                guest.IsPhoneNumberValid = phoneValidation.IsValid;
                                guest.GuestArchieved = false;
                                // Encrypt guest ID for QR code
                                guest.Cypertext = EventProCrypto.EncryptString(_configuration.GetSection("SecurityKey").Value, Convert.ToString(guest.GuestId));
                                newGuests.Add(guest);
                            }

                        }

                        // Save all guests and log audit trail
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

        /// <summary>
        /// Helper method for reading Excel files using OleDb (Legacy)
        /// Used by the legacy Guests POST method
        /// </summary>
        /// <param name="file">Excel file path</param>
        /// <param name="tableName">Excel sheet name</param>
        /// <returns>DataSet with Excel data</returns>
        public DataSet ReadExcel(string file, string tableName)
        {
            string strconnection = _configuration.GetSection("Database").GetSection("ExcelConnection").Value;
            strconnection = strconnection + "Data Source=" + file.ToString() + " ;Excel 12.0;HDR=Yes;IMEX=1";
            OleDbConnection olecn = new OleDbConnection(strconnection);
            OleDbCommand olecmd = new OleDbCommand("SELECT * from [" + tableName + "$]", olecn);
            OleDbDataAdapter olead = new OleDbDataAdapter(olecmd);
            DataSet ds = new DataSet();
            olead.Fill(ds);
            return ds;
        }

        #endregion

        #region Region 13: Status Refresh Operations

        /// <summary>
        /// POST: Admin/StatusRefreshIndividually
        /// Refreshes WhatsApp message delivery status for a single guest
        /// Updates message status (sent, delivered, read, failed) from Twilio
        /// </summary>
        /// <param name="id">Guest ID</param>
        /// <param name="eventid">Event ID</param>
        /// <returns>OK result</returns>
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

            // Update status via Twilio provider
            var guests = new List<Guest>() { guest };
            var whatsAppProvider = _WhatsappSendingProvider.SelectTwilioSendingProvider();
            await whatsAppProvider.UpdateMessagesStatus(guests, events);
            return Ok();
        }

        /// <summary>
        /// POST: Admin/StatusRefreshForAllGuests
        /// Refreshes WhatsApp message delivery status for all guests in an event
        /// Updates message status (sent, delivered, read, failed) from Twilio
        /// Administrator-only function due to potential high API usage
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>JSON result with success status</returns>
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
                // Update status for all guests via Twilio provider
                var whatsAppProvider = _WhatsappSendingProvider.SelectTwilioSendingProvider();
                await whatsAppProvider.UpdateMessagesStatus(guests, events);
            }
            catch
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }

        /// <summary>
        /// GET: Admin/GetRefreshGuestsStatusData
        /// Returns progress data for status refresh operation
        /// Uses memory cache to track refresh progress
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="remaningStatus">Remaining statuses to refresh</param>
        /// <returns>JSON with remaining and total status counts</returns>
        public async Task<IActionResult> GetRefreshGuestsStatusData(int id, int remaningStatus)
        {
            if (remaningStatus == 0)
            {
                // Initial request - get total guest count
                var allStatus = await db.Guest.Where(e => e.EventId == id).CountAsync();
                return Json(new { remaningStatus = 0, allStatus = allStatus });
            }
            else
            {
                // Progress update - retrieve from memory cache
                var sentMessagesCount = _MemoryCacheStoreService.Retrieve(id.ToString());

                return Json(new { remaningStatus = sentMessagesCount, allStatus = remaningStatus });
            }
        }

        #endregion

        #region Region 14: Maintenance and Utility Operations

        /// <summary>
        /// POST: Admin/DeletePastEventsCards
        /// Deletes invitation card folders for past events from Azure Blob Storage
        /// Only deletes cards for events that have already ended (EventTo < DateTime.Now)
        /// Prevents accidental deletion in UAT environment
        /// Administrator-only function for storage maintenance
        /// </summary>
        /// <returns>JSON result with success status</returns>
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> DeletePastEventsCards()
        {
            // Prevent deletion in UAT environment
            if (_configuration.GetSection("Database")["ConnectionString"].ToLower().Contains("EventProuat"))
            {
                return Json(new { success = false });
            }

            try
            {
                var AllCardsDirectory = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
                var environment = _configuration.GetSection("Uploads").GetSection("environment").Value;

                // Get all event folders in blob storage
                List<string> evntsFolder = await _blobStorage.GetFoldersInsideAFolderAsync(environment + AllCardsDirectory, cancellationToken: default);

                // Get upcoming and current events
                var upcomingAndCurrentEvnts = await db.Events.Where(e => e.EventTo >= DateTime.Now)
                    .AsNoTracking()
                    .ToListAsync();

                // Delete folders for past events
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

        /// <summary>
        /// POST: Admin/DeleteSerilogData
        /// Truncates Serilog logging tables (SeriLog and SeriLogAPI)
        /// Administrator-only function for database maintenance
        /// Clears application and API logs to free up database space
        /// </summary>
        /// <returns>JSON result with success status</returns>
        [AuthorizeRoles("Administrator")]
        public async Task<IActionResult> DeleteSerilogData()
        {
            try
            {
                // Truncate logging tables
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

        /// <summary>
        /// GET: Admin/DownloadGuestImage
        /// Downloads invitation card image from URL
        /// Returns image file as JPEG for download
        /// </summary>
        /// <param name="url">Image URL to download</param>
        /// <returns>File stream with invitation card image</returns>
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

        /// <summary>
        /// GET: Admin/AddToCalender
        /// Redirects to calendar file URL for adding event to user's calendar
        /// </summary>
        /// <param name="url">Calendar file (.ics) URL</param>
        /// <returns>Redirect to calendar file</returns>
        [HttpGet]
        public IActionResult AddToCalender(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("Invalid calendar file URL.");
            }

            try
            {
                return Redirect(url);
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// GET: Admin/ForceStartConsumer
        /// Forces the WebHook queue consumer to start processing
        /// Used for troubleshooting webhook processing issues
        /// </summary>
        /// <returns>JSON result with success status</returns>
        [HttpGet]
        public IActionResult ForceStartConsumer()
        {
            _WebHookQueueConsumerService.ForceStart();
            return Json(new { success = true });
        }

        /// <summary>
        /// GET: Admin/ForceStopConsumer
        /// Forces the WebHook queue consumer to stop processing
        /// Used for troubleshooting webhook processing issues
        /// </summary>
        /// <returns>JSON result with success status</returns>
        [HttpGet]
        public IActionResult ForceStopConsumer()
        {
            _WebHookQueueConsumerService.ForceStop();
            return Json(new { success = true });
        }

        #endregion

        #region Region 15: Helper Methods - Validation and Processing

        /// <summary>
        /// Validates that invitation cards exist in blob storage for all guests
        /// Checks if event card folder exists and validates each guest's card file
        /// Card filename format: E00000{eventId}_{guestId}_{noOfMembers}.jpg
        /// </summary>
        /// <param name="guests">List of guests to validate cards for</param>
        /// <param name="_event">Event containing card configuration</param>
        /// <returns>True if all cards exist, false otherwise</returns>
        private async Task<bool> CheckGuestsCardsExistAsync(List<Guest> guests, Events _event)
        {
            #region Old checking code with blob storage
            //string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            //string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;

            //// Check if event card folder exists
            //if (!await _blobStorage.FolderExistsAsync(environment + cardPreview + "/" + _event.Id))
            //{
            //    return false;
            //}

            //// Validate each guest has a card file
            //foreach (var guest in guests)
            //{
            //    string imagePath = cardPreview + "/" + guest.EventId + "/" + "E00000" + guest.EventId + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
            //    if (!await _blobStorage.FileExistsAsync(environment + imagePath))
            //    {
            //        return false;
            //    }
            //}
            #endregion
            _logger.LogInformation(
                "Checking invitation cards for EventId {EventId}, GuestsCount {GuestsCount}",
                _event.Id,
                guests.Count
            );

            foreach (var guest in guests)
            {
                var cardPublicId =
                    $"upload/cards/{_event.Id}/E00000{_event.Id}_{guest.GuestId}_{guest.NoOfMembers}.jpg";

                _logger.LogInformation(
                    "Checking card for GuestId {GuestId}, PublicId {PublicId}",
                    guest.GuestId,
                    cardPublicId
                );

                var guestFinalInvitationUrl =
                    await _cloudinaryService.GetLatestVersionUrlAsync(cardPublicId);

                if (string.IsNullOrEmpty(guestFinalInvitationUrl))
                {
                    _logger.LogError(
                        "Invitation card NOT found for GuestId {GuestId}, PublicId {PublicId}",
                        guest.GuestId,
                        cardPublicId
                    );
                    return false;
                }

                _logger.LogInformation(
                    "Invitation card found for GuestId {GuestId}, Url {Url}",
                    guest.GuestId,
                    guestFinalInvitationUrl
                );
            }

            _logger.LogInformation("All invitation cards exist for EventId {EventId}", _event.Id);
            return true;

        }

        /// <summary>
        /// Validates that all guests have valid phone numbers
        /// Checks both PrimaryContactNo and SecondaryContactNo are not empty
        /// </summary>
        /// <param name="guests">List of guests to validate</param>
        /// <returns>True if all guests have valid phone numbers(primary,secondary), false otherwise</returns>
        private bool CheckGuestsNumbersExist(List<Guest> guests)
        {
            _logger.LogInformation("Checking phone numbers for {GuestsCount} guests", guests.Count);

            foreach (var guest in guests)
            {
                if (string.IsNullOrEmpty(guest.PrimaryContactNo))
                {
                    _logger.LogWarning(
                        "GuestId {GuestId} does not have PrimaryContactNo",
                        guest.GuestId
                    );
                    return false;
                }

                if (string.IsNullOrEmpty(guest.SecondaryContactNo))
                {
                    _logger.LogWarning(
                        "GuestId {GuestId} does not have SecondaryContactNo",
                        guest.GuestId
                    );
                    return false;
                }
            }
            _logger.LogInformation("All guests have valid phone numbers");


            return true;
        }

        /// <summary>
        /// Validates that event location (Google Maps code) exists
        /// Checks if GmapCode field is populated
        /// </summary>
        /// <param name="_event">Event to validate</param>
        /// <returns>True if location exists, false otherwise</returns>
        private bool CheckEventLocationExists(Events _event)
        {
            if (_event.GmapCode == null || _event.GmapCode.Length == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates and updates the invitation card for a guest
        /// Process:
        /// 1. Loads card template from local storage or Cloudinary background image
        /// 2. Loads guest QR code from Cloudinary
        /// 3. Draws guest-specific data on card (name, phone, additional text, member count)
        /// 4. Handles text alignment (right/left/center) and font configuration
        /// 5. Applies zoom ratio for high-resolution images
        /// 6. Saves generated card to Azure Blob Storage
        /// Card customization supports: Guest Name, Mobile No, Additional Text, No. of Scan
        /// </summary>
        /// <param name="guest">Guest to generate card for</param>
        /// <param name="eventId">Event ID</param>
        /// <param name="cardInfo">Card configuration (fonts, colors, positions)</param>
        /// <param name="cardPreview">Card preview path</param>
        /// <param name="guestcode">Guest code path</param>
        /// <param name="path">Card path</param>
        private async Task RefreshCard(Guest guest, int eventId, CardInfo cardInfo, string cardPreview, string guestcode, string path)
        {
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            int guestId = guest.GuestId;
            int nos = Convert.ToInt32(guest.NoOfMembers);

            //Ali hani we depand on cloudinary service to get the latest version of qr code image becasue it clean to add all placeholder on it
            // Always load the original background image (without placeholders)
            // This ensures guest data replaces placeholders cleanly
            //Image img;
            // Load template image from local storage (generated in CardPreview)
            //string templatePath = Path.Combine(webHostEnvironment.WebRootPath, "upload", "cardpreview", $"{eventId}.png");

            //Image img;
            //if (System.IO.File.Exists(templatePath))
            //{
            //    // Use local template
            //    img = Image.FromFile(templatePath);
            //}
            //else
            //{
            //    // Fallback: Load background from Cloudinary if template doesn't exist
            //    using HttpClient client = new HttpClient();
            //    var imageUrl = cardInfo.BackgroundImage;
            //    byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            //    using MemoryStream fs = new MemoryStream(imageData);
            //    img = Image.FromStream(fs);
            //}
            using HttpClient client = new HttpClient();
            var imageUrl = cardInfo.BackgroundImage;
            byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            using MemoryStream fs = new MemoryStream(imageData);
            Image img = Image.FromStream(fs);

            // Calculate zoom ratio for high-resolution images (max width 900px)
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

            // Draw guest QR code on card
            if (cardInfo.BarcodeXaxis != null && cardInfo.BarcodeYaxis != null)
            {
                grap.DrawImage(barcode, (int)(cardInfo.BarcodeXaxis * zoomRatio), (int)(cardInfo.BarcodeYaxis * zoomRatio), (int)(cardInfo.BarcodeWidth * zoomRatio), (int)(cardInfo.BarcodeWidth * zoomRatio));
            }
            else
            {
                grap.DrawImage(barcode, (int)(1 * zoomRatio), (int)(1 * zoomRatio), (int)(cardInfo.BarcodeWidth * zoomRatio), (int)(cardInfo.BarcodeWidth * zoomRatio));
            }

            // Parse selected placeholders (fields to display on card)
            var selectedValues = new string[1];
            if (cardInfo.SelectedPlaceHolder != null)
            {
                selectedValues = cardInfo.SelectedPlaceHolder.Split(',');
            }

            // Configure text formatting for right-to-left languages
            StringFormat frmt = new StringFormat();
            if (cardInfo.FontAlignment == "right")
                frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;

            // Draw Guest Name if selected
            if (selectedValues.Contains("Guest Name"))
            {
                double nameXAxis = (cardInfo.FontAlignment == "right") ? Convert.ToDouble(cardInfo.NameRightAxis) : Convert.ToDouble(cardInfo.ContactNameXaxis);
                double nameYAxis = (double)cardInfo.ContactNameYaxis;

                string guestName = $"{guest.FirstName} {guest.LastName ?? ""}".Trim();

                var font = new System.Drawing.Font(cardInfo.FontName, (float)(cardInfo.FontSize * 0.63 * zoomRatio));

       

       //AliHani         //grap.DrawImage(
                //    img,
                //    destRect: new Rectangle(
                //        (int)((nameXAxis - 10) * zoomRatio),
                //        (int)((nameYAxis - 10) * zoomRatio),
                //        (int)(textSize.Width + 20 * zoomRatio),
                //        (int)(textSize.Height + 20 * zoomRatio)
                //    ),
                //    srcRect: new Rectangle(
                //        (int)((nameXAxis - 10) * zoomRatio),
                //        (int)((nameYAxis - 10) * zoomRatio),
                //        (int)(textSize.Width + 20 * zoomRatio),
                //        (int)(textSize.Height + 20 * zoomRatio)
                //    ),
                //    srcUnit: GraphicsUnit.Pixel
                //);
                StringFormat format = new StringFormat();
                if (cardInfo.FontAlignment == "right")
                {
                    format.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                }
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

            // Draw Mobile Number if selected
            if (selectedValues.Contains("Mobile No"))
            {
                double moXAxis = (cardInfo.ContactNoAlignment == "right") ? Convert.ToDouble(cardInfo.ContactRightAxis) : Convert.ToDouble(cardInfo.ContactNoXaxis);

                frmt = new StringFormat();
                if (cardInfo.ContactNoAlignment == "right")
                    frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                else
                    frmt = new StringFormat();

                var fMobile = new System.Drawing.Font(cardInfo.ContactNoFontName, (float)(cardInfo.ContactNoFontSize * 0.63 * zoomRatio));
                var stringsizeMobile = grap.MeasureString(guest.PrimaryContactNo, fMobile);

                // Handle center alignment
                if (cardInfo.ContactNoAlignment == "center")
                {
                    moXAxis = (Convert.ToDouble(cardInfo.ContactRightAxis) + Convert.ToDouble(cardInfo.ContactNoXaxis)) / 2;
                    frmt = new StringFormat();
                    frmt.Alignment = StringAlignment.Center;
                }

                grap.DrawString(guest.PrimaryContactNo, new System.Drawing.Font(cardInfo.ContactNoFontName, (float)(cardInfo.ContactNoFontSize * 0.63 * zoomRatio))
                , new SolidBrush(ColorTranslator.FromHtml(cardInfo.ContactNoFontColor))
                , new Point((int)((moXAxis * zoomRatio))
                , (int)(cardInfo.ContactNoYaxis * zoomRatio)), frmt);
            }

            // Draw Additional Text if selected
            if (selectedValues.Contains("Additional Text"))
            {
                double atXAxis = (cardInfo.AddTextFontAlignment == "right") ? Convert.ToDouble(cardInfo.AddTextRightAxis) : Convert.ToDouble(cardInfo.AltTextXaxis);

                frmt = new StringFormat();
                if (cardInfo.AddTextFontAlignment == "right")
                    frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                else
                    frmt = new StringFormat();

                var fAdditional = new System.Drawing.Font(cardInfo.AltTextFontName, (float)(cardInfo.AltTextFontSize * 0.63 * zoomRatio));
                var stringsizeAdditional = grap.MeasureString(guest.AdditionalText, fAdditional);

                // Handle center alignment
                if (cardInfo.AddTextFontAlignment == "center")
                {
                    atXAxis = (Convert.ToDouble(cardInfo.AddTextRightAxis) + Convert.ToDouble(cardInfo.AltTextXaxis)) / 2;
                    frmt = new StringFormat();
                    frmt.Alignment = StringAlignment.Center;
                }

                grap.DrawString(guest.AdditionalText, new System.Drawing.Font(cardInfo.AltTextFontName, (float)(cardInfo.AltTextFontSize * 0.63 * zoomRatio))
                , new SolidBrush(ColorTranslator.FromHtml(cardInfo.AltTextFontColor))
                , new Point((int)((atXAxis * zoomRatio))
                , (int)(cardInfo.AltTextYaxis * zoomRatio)), frmt);
            }

            // Draw Number of Scans/Members if selected
            if (selectedValues.Contains("No. of Scan"))
            {
                double nosXAxis = (cardInfo.NosAlignment == "right") ? Convert.ToDouble(cardInfo.NosRightAxis) : Convert.ToDouble(cardInfo.Nosxaxis);

                var fNos = new System.Drawing.Font(cardInfo.NosfontName, (float)(cardInfo.NosfontSize * 0.63 * zoomRatio));
                var stringsizeNos = grap.MeasureString(guest.AdditionalText, fNos);

                frmt = new StringFormat();
                if (cardInfo.NosAlignment == "right")
                    frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                else
                    frmt = new StringFormat();

                // Handle center alignment
                if (cardInfo.NosAlignment == "center")
                {
                    nosXAxis = (Convert.ToDouble(cardInfo.NosRightAxis) + Convert.ToDouble(cardInfo.Nosxaxis)) / 2;
                    frmt = new StringFormat();
                    frmt.Alignment = StringAlignment.Center;
                }

                grap.DrawString(Convert.ToString(guest.NoOfMembers), new System.Drawing.Font(cardInfo.NosfontName, (float)(cardInfo.NosfontSize * 0.63 * zoomRatio))
             , new SolidBrush(ColorTranslator.FromHtml(cardInfo.NosfontColor))
             , new Point((int)((nosXAxis * zoomRatio))
             , (int)(cardInfo.Nosyaxis * zoomRatio)), frmt);
            }

            // Save generated card to Azure Blob Storage
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

            // Dispose graphics objects
            grap.Dispose();
            img.Dispose();
            myBitmap.Dispose();
            barcode.Dispose();
        }

        /// <summary>
        /// Legacy method for generating QR codes using Google Charts API
        /// Note: Google Charts API QR code generation is deprecated
        /// This method is no longer used - RefreshQRCode uses QRCoder library instead
        /// </summary>
        /// <param name="card">Card configuration</param>
        /// <param name="imagePath">Path to save QR code image</param>
        /// <param name="barcodeText">Text to encode in QR code</param>
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

        /// <summary>
        /// Generates QR code for guest using QRCoder library
        /// Encrypts guest ID for QR code content
        /// Supports custom foreground/background colors from card configuration
        /// Handles transparent background when foreground is white (#FFFFFF)
        /// Uploads generated QR code to Cloudinary in folder structure: QR/{eventId}/{guestId}.png
        /// QR codes are used for guest check-in scanning at event entrance
        /// </summary>
        /// <param name="guest">Guest to generate QR code for</param>
        /// <param name="card">Card configuration with QR color settings</param>
        private async Task RefreshQRCode(Guest guest, CardInfo card)
        {
            _logger.LogInformation(
                "RefreshQRCode started | GuestId={GuestId} | EventId={EventId}",
                guest.GuestId,
                guest.EventId
            );

            try
            {
                if (card == null)
                    throw new ArgumentNullException(nameof(card), "CardInfo is null");

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                var encrypted = EventProCrypto.EncryptString(
                    _configuration["SecurityKey"],
                    guest.GuestId.ToString()
                );

                QRCodeData qrCodeData = qrGenerator.CreateQrCode(encrypted, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);

                Bitmap qrCodeImage;
                if (!string.IsNullOrEmpty(card.BackgroundColor) && !string.IsNullOrEmpty(card.ForegroundColor))
                {
                    if (string.Equals(card.ForegroundColor, "#FFFFFF", StringComparison.OrdinalIgnoreCase))
                    {
                        qrCodeImage = qrCode.GetGraphic(5, ColorTranslator.FromHtml(card.BackgroundColor), System.Drawing.Color.Transparent, false);
                    }
                    else
                    {
                        qrCodeImage = qrCode.GetGraphic(5, card.BackgroundColor, card.ForegroundColor, false);
                    }
                }
                else
                {
                    qrCodeImage = qrCode.GetGraphic(5);
                }

                using var ms = new MemoryStream();
                qrCodeImage.Save(ms, ImageFormat.Png);

                await _cloudinaryService.UploadImageAsync(
                    ms,
                    $"{guest.GuestId}.png",
                    $"QR/{guest.EventId}"
                );

                _logger.LogInformation(
                    "RefreshQRCode completed | GuestId={GuestId}",
                    guest.GuestId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "RefreshQRCode failed | GuestId={GuestId} | EventId={EventId}",
                    guest.GuestId,
                    guest.EventId
                );
                throw;
            }
        }

        private async Task<List<string>> GetGuestsWithoutCardsAsync(List<Guest> guests, DAL.Models.Events _event)
        {
            var guestsWithoutCards = new List<string>();

            foreach (var guest in guests)
            {
                // How we handle it 
                //var cardExists = await _blobStorage.FileExistsAsync();
                //var cardExists = await _cloudinaryService.
                if (true)
                {
                    guestsWithoutCards.Add($"{guest.FirstName} (ID: {guest.GuestId})");
                }
            }

            return guestsWithoutCards;
        }

        #endregion

    }
}
