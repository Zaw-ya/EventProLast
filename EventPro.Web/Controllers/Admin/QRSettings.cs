using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using EventPro.DAL.Models;
using EventPro.Web.Common;
using EventPro.Web.Filters;
using EventPro.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using QRCoder;

using Serilog;

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public IActionResult QRSettings(int id)
        {
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            ViewBag.Icon = "nav-icon fas fa-qrcode";
            var cardInfo = db.CardInfo.Where(p => p.EventId == id).FirstOrDefault();
            if (cardInfo == null)
            {
                cardInfo = new CardInfo
                {
                    EventId = id,
                    FontSize = 20,
                    ContactNoFontSize = 20,
                    AltTextFontSize = 20,
                    NosfontSize = 20,
                    BackgroundColor = "#003322",
                    ForegroundColor = "#FFFFFF",
                    BarcodeWidth = 150,
                    BarcodeHeight = 150,
                    FontName = "Agency FB",
                    ContactNoFontName = "Agency FB",
                    AltTextFontName = "Agency FB",
                    NosfontName = "Agency FB",
                    FontColor = "#003322",
                    ContactNoFontColor = "#003322",
                    AltTextFontColor = "#003322",
                    NosfontColor = "#003322"
                };

                db.CardInfo.Add(cardInfo);
                db.SaveChanges();
            }
            if (cardInfo.ForegroundColor == "")
            {
                cardInfo.ForegroundColor = "#FFFFFF";
            }
            ViewBag.Fonts = new SelectList(GetFonts(), "Name", "Name");

            SetBreadcrum("QR Settings", "/admin");
            return View(cardInfo);
        }

        private List<SystemFont> GetFonts()
        {
            List<SystemFont> fonts = new List<SystemFont>();
            int i = 1;
            using (InstalledFontCollection col = new InstalledFontCollection())
            {
                foreach (FontFamily fa in col.Families)
                {
                    SystemFont font = new SystemFont
                    {
                        Id = i,
                        Name = fa.Name
                    };
                    fonts.Add(font);
                    i++;
                }
            }

            return fonts.OrderBy(p => p.Name).ToList();
        }

        // Post the card info 
        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> QRSettings(CardInfo info)
        {
            ViewBag.Icon = "nav-icon fas fa-qrcode";
            var files = Request.Form.Files;
            string path = _configuration.GetSection("Uploads").GetSection("Card").Value;
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string filename = string.Empty;
            bool hasFile = false;
            string backgroundImageUrl = string.Empty;
            // Process uploaded files for the card
            foreach (var file in files)
            {
                if (file.Length > 1000 * 1024)
                {
                    ModelState.AddModelError(string.Empty, "File size must be less than 1 MB");

                    if (info.ForegroundColor == "")
                    {
                        info.ForegroundColor = "#FFFFFF";
                    }
                    ViewBag.Fonts = new SelectList(GetFonts(), "Name", "Name");

                    SetBreadcrum("QR Settings", "/admin");
                    return View(info);
                }
                string extension = file.ContentType.ToLower().Replace(@"image/", "");
                filename = Guid.NewGuid() + "." + extension;

                using var stream = file.OpenReadStream();
                // Gharabawy : Here we have to replace with the cloudinary
                backgroundImageUrl = await _cloudinaryService.UploadImageAsync(stream, environment + path + "/" + filename, path);
                //await _blobStorage.UploadAsync(stream, extension, environment + path + "/" + filename, cancellationToken: default);
                hasFile = true;
            }
            CardInfo card = new CardInfo();
            card = db.CardInfo.Where(p => p.CardId == info.CardId).FirstOrDefault();
            card.BackgroundColor = info.BackgroundColor;
            if (info.ForegroundColor != null)
            {
                card.ForegroundColor = info.ForegroundColor;
            }
            else
            {
                card.ForegroundColor = "#FFFFFF";
            }

            card.CardWidth = info.CardWidth;
            card.CardHeight = info.CardHeight;
            card.BarcodeWidth = info.BarcodeWidth;
            card.BarcodeHeight = info.BarcodeHeight;
            card.DefaultFont = info.DefaultFont;

            if (hasFile)
                // Gharabawy : Here we have to replace with the cloudinary and save the URL not the file name
                card.BackgroundImage = backgroundImageUrl;
            await db.SaveChangesAsync();

            // Generate QR Code
            string barcodePath = _configuration.GetSection("Uploads").GetSection("Barcode").Value;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("My Invite.", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(5
                , info.BackgroundColor
                , info.ForegroundColor
                , false);

            if (string.Equals(info.ForegroundColor, "#FFFFFF", StringComparison.OrdinalIgnoreCase))
            {
                qrCodeImage = qrCode.GetGraphic(
                    5,
                    ColorTranslator.FromHtml(info.BackgroundColor), // Convert hex string to Color
                    Color.Transparent, // Use Color.Transparent directly (if supported)
                    false
                );
            }

            string cardPreview = _configuration.GetSection("Uploads").GetSection("environment").Value +
                _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;

            //Ali hani // Create folder structure: QR/{eventId}/{eventId}.png
            string qrFolderPath = $"QR/{info.EventId}";
            string qrFileName = $"{info.EventId}.png";

            // Delete existing barcode if any
            await _cloudinaryService.DeleteAsync($"{qrFolderPath}/{qrFileName}");
            //await _blobStorage.DeleteFolderAsync(environment + cardPreview + "/" + info.EventId + "/", cancellationToken: default);

            string qrCodeUrl = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                // ?? Change to PNG format (supports transparency)
                // Convert the QR code to PNG format and upload it
                qrCodeImage.Save(ms, ImageFormat.Png);
                qrCodeUrl = await _cloudinaryService.UploadImageAsync(ms, qrFileName, qrFolderPath);
                //await _blobStorage.UploadAsync(ms, "png", environment + barcodePath + "/" + info.EventId + ".png", cancellationToken: default);
            }

            // Ali hani // Store the QR code URL in the card info
            card.BarcodeColorCode = qrCodeUrl;
            db.CardInfo.Update(card);
            await db.SaveChangesAsync();
            qrCodeImage.Dispose();
            SetBreadcrum("QR Settings", "/admin");

            if (card.BackgroundImage == null)
            {
                ViewBag.Fonts = new SelectList(GetFonts(), "Name", "Name");
                SetBreadcrum("QR Settings", "/admin");
                TempData["entry-error"] = "Background image not uploaded, please upload background image to proceed with designer";
                return View(card);
            }

            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, info.EventId, DAL.Enum.ActionEnum.UpdateQRCode);
            // redirect to the CardPreview view
            return RedirectToAction("CardPreview", "admin", new { id = info.EventId });
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> CardPreview(int id)
        {
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            ViewBag.Icon = "nav-icon fas fa-qrcode";
            SetBreadcrum("Card Preview", "/admin");
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            var cardInfo = db.CardInfo.Where(p => p.EventId == id).FirstOrDefault();
            if (cardInfo == null)
                return RedirectToAction("QRSettings", "admin", new { id = id });
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Card").Value;


            // Gharabawy : check if the filename is not null and construct the same
            // structure : environment + path(code) + / + fileName(BackgroundImage)

            // Gharabawy TODO 12/1 : Think to store the URL not the file name
            // !await _cloudinaryService.FileExistsAsync(environment + cardPreview + "/" + cardInfo.BackgroundImage)
            // if (!await _blobStorage.FileExistsAsync(environment + cardPreview + "/" + cardInfo.BackgroundImage))
            //{
            //    TempData["entry-error"] = "Background image not uploaded, please upload background image to proceed with designer";
            //    return RedirectToAction("QRSettings", "admin", new { id = id });
            //}
            if (string.IsNullOrWhiteSpace(cardInfo.BackgroundImage))
            {
                TempData["entry-error"] = "Background image not uploaded, please upload background image to proceed with designer";
                return RedirectToAction("QRSettings", "admin", new { id = id });
            }

            List<int> fontSize = new List<int>();

            using HttpClient client = new HttpClient();

            #region Old Code for local uploads we dont need it any more
            //var request = _httpContextAccessor.HttpContext.Request;
            //var baseUrl = $"{request.Scheme}://{request.Host}";
            //var imageUrl = $"{baseUrl}/upload" + cardPreview + @"/" + cardInfo.BackgroundImage;
            //byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            #endregion

            var imageUrl = cardInfo.BackgroundImage;

            // Validate that the URL is absolute
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                TempData["entry-error"] = "Invalid background image URL. Please upload the background image again.";
                return RedirectToAction("QRSettings", "admin", new { id = id });
            }

            byte[] imageData = await client.GetByteArrayAsync(imageUrl);

            using MemoryStream ms = new MemoryStream(imageData);
            Image img = Image.FromStream(ms);

            for (int i = 8; i < 100; i++)
            {
                fontSize.Add(i);
            }
            ViewBag.FontSize = new SelectList(fontSize);
            ViewBag.ImageWidth = img.Width;
            ViewBag.ImageHeight = img.Height;
            ViewBag.Fonts = new SelectList(GetFonts(), "Name", "Name");

            //Ali hani // Set QR code URL - if not stored in database, construct Cloudinary URL
            if (!string.IsNullOrEmpty(cardInfo.BarcodeColorCode))
            {
                ViewBag.Barcode = cardInfo.BarcodeColorCode;
            }
            else
            {
                //Ali hani    // Construct Cloudinary URL for existing QR codes with folder structure: QR/{eventId}/{eventId}.png
                string cloudName = _configuration.GetSection("CloudinarySettings").GetSection("CloudName").Value;
                ViewBag.Barcode = $"https://res.cloudinary.com/{cloudName}/image/upload/QR/{id}/{id}.png";
            }

            return View(cardInfo);
        }

        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public IActionResult DownloadAsPDF(int id)
        {
            return View();
        }

        // When Refresh All is clicked
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> RefreshAll(int id, int type)
        {
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            try
            {

                string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
                List<string> files = null;

                List<Guest> guests = await db.Guest.Where(p => p.EventId == id)
                    .AsNoTracking()
                    .ToListAsync();

                //if (await _blobStorage.FolderExistsAsync(environment + cardPreview + "/" + id))
                //{
                //    files = await _blobStorage.GetFolderFilesAsync(environment + cardPreview + "/" + id + "/", cancellationToken: default);
                //    foreach (var file in files)
                //    {
                //        guests.RemoveAll(e => environment + cardPreview + "/" + id + "/" + "E00000" + id + "_" + e.GuestId + "_" + e.NoOfMembers + ".jpg" == file);
                //    }
                //}

                // Get the invitation or the card info
                // That has the design details such as positions ,font size, Qr position etc
                var card = await db.CardInfo.Where(p => p.EventId == id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                string guestcode = _configuration.GetSection("Uploads").GetSection("Guestcode").Value;
                string path = _configuration.GetSection("Uploads").GetSection("Card").Value;

                //if (!await _blobStorage.FolderExistsAsync(environment + guestcode) || !await _blobStorage.FolderExistsAsync(environment + path))
                //    return Json(new Response { succeeded = false, result = "Directory not exist" });

                //var parallelOptions = new ParallelOptions
                //{
                //    MaxDegreeOfParallelism = 1 // Use only one core
                //};

                //await Parallel.ForEachAsync(guests, async (guest, CancellationToken) =>
                //{
                //    await RefreshQRCode(guest, card);
                //    await RefreshCard(guest, id, card, cardPreview, guestcode, path);
                //});
                foreach(var guest in guests)
                {
                    await RefreshQRCode(guest, card);
                    await RefreshCard(guest, id, card, cardPreview, guestcode, path);
                }

                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.RefreshCards);
                return Json(new Response() { succeeded = true });
            }
            catch (Exception ex)
            {
                Log.Error($"Something went wrong In RefreshAll ,Message:{ex.Message} InnerEx :{ex.InnerException}");
                return Json(new Response { succeeded = false, result = ex.Message });
            }
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> GetCurrentRefreshUpdates(int id)
        {
            int eventCardsCount = await db.Guest.Where(e => e.EventId == id)
                .AsNoTracking()
                .CountAsync();
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            double createdFiles = 0;

            if (await _blobStorage.FolderExistsAsync(environment + cardPreview + "/" + id))
            {
                createdFiles = await _blobStorage.CountFilesInFolderAsync(environment + cardPreview + "/" + id);
            }
            return Json(new { totalEventCards = eventCardsCount, totalCreatedCards = createdFiles });
        }
      
        // This action get all guests final invitations and compress them 
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> DownloadAll(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (HasOperatorRole())
            {

                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            var eventInfo = db.Events.Where(p => p.Id == id).FirstOrDefault();

            // Gharabawy : We have commented this cause we will use the cloudinary
            //string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            //string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;

            string folderName = $"cards/{id}/"; // It means the cards -> then go to the eventId(EventFolder) that has the guests folders

            //MemoryStream zipStream = new MemoryStream();
            //zipStream = await _blobStorage.DownloadFilesAsZipStreamAsync(environment + cardPreview + "/" + id);
            //await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.DownloadCards);
            //return File(zipStream, "application/zip", eventInfo.SystemEventTitle.Replace(" ", "_") + ".zip");

            try
            {
                // Get active guests from the database
                var guests = await db.Guest
                    .Where(g => g.EventId == id && !g.GuestArchieved)
                    .Select(g => new { g.GuestId, g.NoOfMembers })  // بس البيانات اللازمة عشان نوفر موارد
                    .ToListAsync();

                if (!guests.Any())
                    return NotFound("لا يوجد ضيوف نشيطين في هذا الحدث");

                // Create zip stream
                var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var guest in guests)
                    {
                        // build the fileId or publicId 
                        string publicId = $"cards/{id}/E00000{id}_{guest.GuestId}_{guest.NoOfMembers}";

                        try
                        {
                            // get the latest URL
                            string latestUrl = await _cloudinaryService.GetLatestVersionUrlAsync(publicId);

                            // download image
                            using var client = new HttpClient();
                            var imageBytes = await client.GetByteArrayAsync(latestUrl);

                            // fileName in zip folder
                            string fileName = $"E00000{id}_{guest.GuestId}_{guest.NoOfMembers}.jpg";

                            // add file to the zip file
                            var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                            using var entryStream = entry.Open();
                            await entryStream.WriteAsync(imageBytes);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning($"Failed to download card for guest {guest.GuestId} in event {id}: {ex.Message}");
                        }
                    }
                }
                zipStream.Position = 0;

                await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.DownloadCards);

                return File(zipStream, "application/zip", $"{eventInfo.SystemEventTitle.Replace(" ", "_")}_Cards.zip");
            }
            catch (Exception ex)
            {
                // يفضل تسجل الخطأ في log حقيقي
                Log.Error($"Failed to download all cards for event {id}: {ex.Message}");
                return StatusCode(500, "حدث خطأ أثناء إنشاء ملف الضغط");
            }
        }

        /// <summary>
        /// Returns the latest URL (with current version) for a guest's final card
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLatestCardUrl(int eventId, int guestId, int noOfMembers)
        {
            try
            {
                // بناء الـ publicId بنفس الطريقة اللي بتستخدمها في الرفع
                string publicId = $"cards/{eventId}/E00000{eventId}_{guestId}_{noOfMembers}";

                // استدعاء الدالة اللي عملناها
                string latestUrl = await _cloudinaryService.GetLatestVersionUrlAsync(publicId);

                return Ok(new { success = true, url = latestUrl });
            }
            catch (Exception ex)
            {
                // يفضل تسجل الخطأ في log حقيقي
                return StatusCode(500, new { success = false, message = "حدث خطأ أثناء جلب رابط الدعوة: " + ex.Message });
            }
        }

        /// <summary>
        /// Returns the latest URL for a guest's QR code
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLatestQrUrl(int eventId, int guestId)
        {
            try
            {
                string publicId = $"QR/{eventId}/{guestId}";

                string latestUrl = await _cloudinaryService.GetLatestVersionUrlAsync(publicId);

                return Ok(new { success = true, url = latestUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ أثناء جلب رابط الـ QR: " + ex.Message });
            }
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> CardPreview(CardInfo info)
        {
            ViewBag.Icon = "nav-icon fas fa-qrcode";
            SetBreadcrum("Card Preview", "/admin");
            CardInfo cardInfo = db.CardInfo.Where(p => p.EventId == info.EventId).FirstOrDefault();
            cardInfo.ContactNoXaxis = info.ContactNoXaxis > 0 ? info.ContactNoXaxis : 0.0;
            cardInfo.ContactNoYaxis = info.ContactNoYaxis > 0 ? info.ContactNoYaxis : 0.0;
            cardInfo.ContactNameXaxis = info.ContactNameXaxis > 0 ? info.ContactNameXaxis : 0.0;
            cardInfo.ContactNameYaxis = info.ContactNameYaxis > 0 ? info.ContactNameYaxis : 0.0;
            cardInfo.BarcodeXaxis = info.BarcodeXaxis > 0 ? info.BarcodeXaxis : 0.0;
            cardInfo.BarcodeYaxis = info.BarcodeYaxis > 0 ? info.BarcodeYaxis : 0.0;

            cardInfo.Nosyaxis = info.Nosyaxis > 0 ? info.Nosyaxis : 0.0;
            cardInfo.Nosxaxis = info.Nosxaxis > 0 ? info.Nosxaxis : 0.0;

            cardInfo.AltTextYaxis = info.AltTextYaxis > 0 ? info.AltTextYaxis : 0.0;
            cardInfo.AltTextXaxis = info.AltTextXaxis > 0 ? info.AltTextXaxis : 0.0;

            cardInfo.ContactNoFontName = info.ContactNoFontName;
            cardInfo.ContactNoFontColor = info.ContactNoFontColor;
            cardInfo.ContactNoFontSize = info.ContactNoFontSize;

            cardInfo.FontColor = info.FontColor;
            cardInfo.FontSize = info.FontSize;
            cardInfo.FontName = info.FontName;

            cardInfo.AltTextFontName = info.AltTextFontName;
            cardInfo.AltTextFontSize = info.AltTextFontSize;
            cardInfo.AltTextFontColor = info.AltTextFontColor;

            cardInfo.NosfontColor = info.NosfontColor;
            cardInfo.NosfontName = info.NosfontName;
            cardInfo.NosfontSize = info.NosfontSize;

            cardInfo.FontStyleAddText = info.FontStyleAddText;
            cardInfo.FontStyleMobNo = info.FontStyleMobNo;
            cardInfo.FontStyleName = info.FontStyleName;
            cardInfo.FontStyleNos = info.FontStyleNos;


            cardInfo.FontAlignment = info.FontAlignment;
            cardInfo.AddTextFontAlignment = info.AddTextFontAlignment;
            cardInfo.ContactNoAlignment = info.ContactNoAlignment;
            cardInfo.NosAlignment = info.NosAlignment;

            if (info.BarcodeWidth != null)
                cardInfo.BarcodeWidth = info.BarcodeWidth;
            if (info.BarcodeHeight != null)
                cardInfo.BarcodeHeight = info.BarcodeHeight;

            cardInfo.SelectedPlaceHolder = info.SelectedPlaceHolder;

            cardInfo.NameRightAxis = info.NameRightAxis;
            cardInfo.ContactRightAxis = info.ContactRightAxis;
            cardInfo.NosRightAxis = info.NosRightAxis;
            cardInfo.AddTextRightAxis = info.AddTextRightAxis;

            db.SaveChanges();

            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string barcodePath = _configuration.GetSection("Uploads").GetSection("Barcode").Value;
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            string path = _configuration.GetSection("Uploads").GetSection("Card").Value;

            // Set QR code URL - if not stored in database, construct Cloudinary URL
            if (!string.IsNullOrEmpty(cardInfo.BarcodeColorCode))
            {
                ViewBag.Barcode = cardInfo.BarcodeColorCode;
            }
            else
            {
                // Construct Cloudinary URL for existing QR codes with folder structure: QR/{eventId}/{eventId}.png
                string cloudName = _configuration.GetSection("CloudinarySettings").GetSection("CloudName").Value;
                ViewBag.Barcode = $"https://res.cloudinary.com/{cloudName}/image/upload/QR/{cardInfo.EventId}/{cardInfo.EventId}.png";
            }
            // Validate background image URL from database
            if (string.IsNullOrWhiteSpace(cardInfo.BackgroundImage))
            {
                TempData["message2"] = null;
                TempData["message"] = "Background image not uploaded. Please upload background image in QR Settings first.";
                return RedirectToAction("QRSettings", "admin", new { id = info.EventId });
            }

            using HttpClient client = new HttpClient();
            //var request = _httpContextAccessor.HttpContext.Request;
            //var baseUrl = $"{request.Scheme}://{request.Host}";
            //var imageUrl = $"{baseUrl}/upload" + path + @"/" + cardInfo.BackgroundImage;
            var imageUrl = cardInfo.BackgroundImage; // Use cardInfo.BackgroundImage which is from database

            // Validate that the URL is absolute
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                TempData["message2"] = null;
                TempData["message"] = "Invalid background image URL. Please upload the background image again in QR Settings.";
                return RedirectToAction("QRSettings", "admin", new { id = info.EventId });
            }

            byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            using MemoryStream ms = new MemoryStream(imageData);
            Image img = Image.FromStream(ms);
            ViewBag.ImageWidth = img.Width;
            ViewBag.ImageHeight = img.Height;
            decimal zoomRatio = 1;
            if (img.Width > 900)
            {
                zoomRatio = Convert.ToDecimal(img.Width) / Convert.ToDecimal(900);
            }
            List<int> fontSize = new List<int>();
            for (int i = 8; i < 100; i++)
            {
                fontSize.Add(i);
            }
            ViewBag.FontSize = new SelectList(fontSize);
            await GenerateCardAsync(cardInfo, barcodePath, cardPreview, path, (float)zoomRatio);
            ViewBag.Fonts = new SelectList(GetFonts(), "Name", "Name");
            TempData["successMessage"] = "Card template saved successfully!";

            string cardPreviews = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            //await _blobStorage.DeleteFolderAsync(environment + cardPreview + "/" + info.EventId + "/", cancellationToken: default);

            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, info.EventId, DAL.Enum.ActionEnum.UpdateCardDesign);

            // Redirect to Guests view with event id
            return RedirectToAction("Guests", "Admin", new { id = info.EventId });
        }

        private async Task GenerateCardAsync(CardInfo cardInfo, string barcodePath, string cardPreview, string path, float zoomRatio, int guestId = 0)
        {
            using HttpClient client = new HttpClient();
            //var request = _httpContextAccessor.HttpContext.Request;
            //var baseUrl = $"{request.Scheme}://{request.Host}";
            //var imageUrl = $"{baseUrl}/upload" + path + @"/" + cardInfo.BackgroundImage;
            var imageUrl = cardInfo.BackgroundImage;
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            using MemoryStream ms = new MemoryStream(imageData);

            Image background = Image.FromStream(ms);
            Bitmap myBitmap = new Bitmap(background);
            Graphics grap = Graphics.FromImage(myBitmap);
            Image barcode = await AddBarcodeAsync(cardInfo, barcodePath, grap, zoomRatio);
            if (cardInfo.SelectedPlaceHolder != null && cardInfo.SelectedPlaceHolder.Length > 0)
            {
                StringFormat frmt = new StringFormat();

                var selectedValues = cardInfo.SelectedPlaceHolder.Split(',');
                if (selectedValues.Contains("Guest Name"))
                {
                    double nameXAxis = (cardInfo.FontAlignment == "right") ? Convert.ToDouble(cardInfo.NameRightAxis) : Convert.ToDouble(cardInfo.ContactNameXaxis);
                    if (cardInfo.FontAlignment == "right")
                        frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    else
                        frmt = new StringFormat();

                    if (cardInfo.FontAlignment == "center")
                    {
                        frmt = new StringFormat();
                        frmt.Alignment = StringAlignment.Center;
                    }


                    grap.DrawString("اسم الضيف", new Font(cardInfo.FontName, (float)(cardInfo.FontSize * 0.63 * zoomRatio))
                , new SolidBrush(ColorTranslator.FromHtml(cardInfo.FontColor))
                , new Point((int)(nameXAxis * zoomRatio)
                , (int)(cardInfo.ContactNameYaxis * zoomRatio)), frmt);
                }
                if (selectedValues.Contains("Mobile No"))
                {
                    frmt = new StringFormat();
                    double moXAxis = (cardInfo.ContactNoAlignment == "right") ? Convert.ToDouble(cardInfo.ContactRightAxis) : Convert.ToDouble(cardInfo.ContactNoXaxis);
                    if (cardInfo.ContactNoAlignment == "right")
                        frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    else
                        frmt = new StringFormat();


                    if (cardInfo.ContactNoAlignment == "center")
                    {
                        frmt.Alignment = StringAlignment.Center;
                    }


                    grap.DrawString("رقم الهاتف", new Font(cardInfo.ContactNoFontName, (float)(cardInfo.ContactNoFontSize * 0.63 * zoomRatio))
                    , new SolidBrush(ColorTranslator.FromHtml(cardInfo.ContactNoFontColor))
                    , new Point((int)(moXAxis * zoomRatio)
                    , (int)(cardInfo.ContactNoYaxis * zoomRatio)), frmt);
                }
                if (selectedValues.Contains("Additional Text"))
                {
                    frmt = new StringFormat();
                    double atXAxis = (cardInfo.AddTextFontAlignment == "right") ? Convert.ToDouble(cardInfo.AddTextRightAxis) : Convert.ToDouble(cardInfo.AltTextXaxis);
                    if (cardInfo.AddTextFontAlignment == "right")
                        frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    else
                        frmt = new StringFormat();


                    if (cardInfo.AddTextFontAlignment == "center")
                        frmt.Alignment = StringAlignment.Center;

                    grap.DrawString("نص إضافي", new Font(cardInfo.AltTextFontName, (float)(cardInfo.AltTextFontSize * 0.63 * zoomRatio))
                    , new SolidBrush(ColorTranslator.FromHtml(cardInfo.AltTextFontColor))
                    , new Point((int)(atXAxis * zoomRatio)
                    , (int)(cardInfo.AltTextYaxis * zoomRatio)), frmt);
                }
                if (selectedValues.Contains("No. of Scan"))
                {
                    frmt = new StringFormat();
                    double nosXAxis = (cardInfo.NosAlignment == "right") ? Convert.ToDouble(cardInfo.NosRightAxis) : Convert.ToDouble(cardInfo.Nosxaxis);
                    if (cardInfo.NosAlignment == "right")
                        frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    else
                        frmt = new StringFormat();

                    if (cardInfo.NosAlignment == "center")
                    {
                        frmt.Alignment = StringAlignment.Center;
                    }

                    grap.DrawString("رقم", new Font(cardInfo.NosfontName, (float)(cardInfo.NosfontSize * 0.63 * zoomRatio))
                 , new SolidBrush(ColorTranslator.FromHtml(cardInfo.NosfontColor))
                 , new Point((int)(nosXAxis * zoomRatio)
                 , (int)(cardInfo.Nosyaxis * zoomRatio)), frmt);
                }
            }


            // Save template locally
            string localPath = Path.Combine(webHostEnvironment.WebRootPath, "upload", "cardpreview");
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }

            string fileName = guestId > 0 ? $"{guestId}.png" : $"{cardInfo.EventId}.png";
            string fullPath = Path.Combine(localPath, fileName);

            myBitmap.Save(fullPath, ImageFormat.Png);

            grap.Dispose();
            myBitmap.Dispose();
            barcode.Dispose();
            background.Dispose();
        }

        private async Task<Image> AddBarcodeAsync(CardInfo cardInfo, string barcodePath, Graphics grap, float zoomRatio)
        {
            using HttpClient client = new HttpClient();

            // Get QR code URL from database or construct Cloudinary URL
            string imageUrl;
            if (!string.IsNullOrEmpty(cardInfo.BarcodeColorCode))
            {
                imageUrl = cardInfo.BarcodeColorCode;
            }
            else
            {
                // Construct Cloudinary URL for existing QR codes
                string cloudName = _configuration.GetSection("CloudinarySettings").GetSection("CloudName").Value;
                imageUrl = $"https://res.cloudinary.com/{cloudName}/image/upload/QR/{cardInfo.EventId}/{cardInfo.EventId}.png";
            }

            byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            using MemoryStream ms = new MemoryStream(imageData);

            Image barcode = Image.FromStream(ms);
            grap.DrawImage(barcode, (int)(cardInfo.BarcodeXaxis * zoomRatio) - 2, (int)(cardInfo.BarcodeYaxis * zoomRatio) - 2, (int)(cardInfo.BarcodeWidth * zoomRatio) + 2, (int)(cardInfo.BarcodeWidth * zoomRatio) + 2);
            return barcode;
        }
    }
}
