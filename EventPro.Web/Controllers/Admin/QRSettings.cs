using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPro.DAL.Models;
using EventPro.Web.Common;
using EventPro.Web.Filters;
using EventPro.Web.Models;
using EventPro.Web.Services;
using QRCoder;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using EventPro.Business.Storage.Interface;

namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        #region QR Code and Card Settings

        /// <summary>
        /// GET: Admin/QRSettings
        /// Displays QR code configuration page for an event
        /// Creates default CardInfo if none exists with standard settings
        /// Validates operator access for the event
        /// </summary>
        /// <param name="id">Event ID to configure QR settings for</param>
        /// <returns>View with QR settings form and available fonts</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public IActionResult QRSettings(int id)
        {
            // Validate operator access to this event
            if (HasOperatorRole())
            {
                var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            ViewBag.Icon = "nav-icon fas fa-qrcode";

            // Load or create card info with default settings
            var cardInfo = db.CardInfo.Where(p => p.EventId == id).FirstOrDefault();
            if (cardInfo == null)
            {
                // Create default card configuration
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

            // Ensure foreground color has default value
            if (cardInfo.ForegroundColor == "")
            {
                cardInfo.ForegroundColor = "#FFFFFF";
            }

            // Load system fonts for font selection dropdown
            ViewBag.Fonts = new SelectList(GetFonts(), "Name", "Name");

            SetBreadcrum("QR Settings", "/admin");
            return View(cardInfo);
        }

        /// <summary>
        /// POST: Admin/QRSettings
        /// Saves QR code and card settings including background image upload
        /// Generates QR code with specified colors and uploads to Cloudinary
        /// Validates file size (max 1MB) and processes background image
        /// Creates folder structure: QR/{eventId}/{eventId}.png
        /// Logs update action in audit trail
        /// </summary>
        /// <param name="info">Card information with QR and design settings</param>
        /// <returns>Redirect to CardPreview on success, or view with errors</returns>
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

            // Process uploaded background image
            foreach (var file in files)
            {
                // Validate file size (max 1MB)
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

                // Extract file extension and generate unique filename
                string extension = file.ContentType.ToLower().Replace(@"image/", "");
                filename = Guid.NewGuid() + "." + extension;

                // Upload to Cloudinary
                using var stream = file.OpenReadStream();
                backgroundImageUrl = await _cloudinaryService.UploadImageAsync(stream, environment + path + "/" + filename, path);
                hasFile = true;
            }

            // Update card information
            CardInfo card = new CardInfo();
            card = db.CardInfo.Where(p => p.CardId == info.CardId).FirstOrDefault();
            card.BackgroundColor = info.BackgroundColor;

            // Set foreground color with default
            if (info.ForegroundColor != null)
            {
                card.ForegroundColor = info.ForegroundColor;
            }
            else
            {
                card.ForegroundColor = "#FFFFFF";
            }

            // Update card dimensions and barcode settings
            card.CardWidth = info.CardWidth;
            card.CardHeight = info.CardHeight;
            card.BarcodeWidth = info.BarcodeWidth;
            card.BarcodeHeight = info.BarcodeHeight;
            card.DefaultFont = info.DefaultFont;

            // Save background image URL if uploaded
            if (hasFile)
                card.BackgroundImage = backgroundImageUrl;

            await db.SaveChangesAsync();

            // Generate QR Code with specified colors
            string barcodePath = _configuration.GetSection("Uploads").GetSection("Barcode").Value;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("My Invite.", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            // Create QR code bitmap with background/foreground colors
            Bitmap qrCodeImage = qrCode.GetGraphic(5
                , info.BackgroundColor
                , info.ForegroundColor
                , false);

            // Use transparent foreground for white color
            if (string.Equals(info.ForegroundColor, "#FFFFFF", StringComparison.OrdinalIgnoreCase))
            {
                qrCodeImage = qrCode.GetGraphic(
                    5,
                    ColorTranslator.FromHtml(info.BackgroundColor),
                    Color.Transparent,
                    false
                );
            }

            string cardPreview = _configuration.GetSection("Uploads").GetSection("environment").Value +
                _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;

            // Create folder structure: QR/{eventId}/{eventId}.png
            string qrFolderPath = $"QR/{info.EventId}";
            string qrFileName = $"{info.EventId}.png";

            // Delete existing QR code if any
            await _cloudinaryService.DeleteAsync($"{qrFolderPath}/{qrFileName}");

            // Upload QR code to Cloudinary
            string qrCodeUrl = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert to PNG format (supports transparency)
                qrCodeImage.Save(ms, ImageFormat.Png);
                qrCodeUrl = await _cloudinaryService.UploadImageAsync(ms, qrFileName, qrFolderPath);
            }

            // Store QR code URL in database
            card.BarcodeColorCode = qrCodeUrl;
            db.CardInfo.Update(card);
            await db.SaveChangesAsync();
            qrCodeImage.Dispose();
            SetBreadcrum("QR Settings", "/admin");

            // Validate background image was uploaded
            if (card.BackgroundImage == null)
            {
                ViewBag.Fonts = new SelectList(GetFonts(), "Name", "Name");
                SetBreadcrum("QR Settings", "/admin");
                TempData["entry-error"] = "Background image not uploaded, please upload background image to proceed with designer";
                return View(card);
            }

            // Log audit trail
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, info.EventId, DAL.Enum.ActionEnum.UpdateQRCode);

            // Redirect to card preview
            return RedirectToAction("CardPreview", "admin", new { id = info.EventId });
        }

        #endregion

        #region Card Preview and Design

        /// <summary>
        /// GET: Admin/CardPreview
        /// Displays card design preview with background image and placeholder positioning
        /// Validates background image URL and loads image dimensions
        /// Provides font options and QR code URL for preview
        /// </summary>
        /// <param name="id">Event ID to preview card for</param>
        /// <returns>View with card preview and design tools</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> CardPreview(int id)
        {
            // Validate operator access
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

            // Redirect to settings if no card info exists
            if (cardInfo == null)
                return RedirectToAction("QRSettings", "admin", new { id = id });

            string cardPreview = _configuration.GetSection("Uploads").GetSection("Card").Value;

            // Validate background image URL exists
            if (string.IsNullOrWhiteSpace(cardInfo.BackgroundImage))
            {
                TempData["entry-error"] = "Background image not uploaded, please upload background image to proceed with designer";
                return RedirectToAction("QRSettings", "admin", new { id = id });
            }

            List<int> fontSize = new List<int>();
            using HttpClient client = new HttpClient();

            var imageUrl = cardInfo.BackgroundImage;

            // Validate URL is absolute
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                TempData["entry-error"] = "Invalid background image URL. Please upload the background image again.";
                return RedirectToAction("QRSettings", "admin", new { id = id });
            }

            // Load image to get dimensions
            byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            using MemoryStream ms = new MemoryStream(imageData);
            Image img = Image.FromStream(ms);

            // Build font size options (8-99)
            for (int i = 8; i < 100; i++)
            {
                fontSize.Add(i);
            }

            ViewBag.FontSize = new SelectList(fontSize);
            ViewBag.ImageWidth = img.Width;
            ViewBag.ImageHeight = img.Height;
            ViewBag.Fonts = new SelectList(GetFonts(), "Name", "Name");

            // Set QR code URL - use stored URL or construct Cloudinary URL
            if (!string.IsNullOrEmpty(cardInfo.BarcodeColorCode))
            {
                ViewBag.Barcode = cardInfo.BarcodeColorCode;
            }
            else
            {
                // Construct Cloudinary URL: QR/{eventId}/{eventId}.png
                string cloudName = _configuration.GetSection("CloudinarySettings").GetSection("CloudName").Value;
                ViewBag.Barcode = $"https://res.cloudinary.com/{cloudName}/image/upload/QR/{id}/{id}.png";
            }

            return View(cardInfo);
        }

        /// <summary>
        /// POST: Admin/CardPreview
        /// Saves card design with placeholder positions, fonts, colors, and alignments
        /// Generates preview template image with Arabic placeholder text
        /// Calculates zoom ratio for proper scaling on large images
        /// Logs design update in audit trail and redirects to Guests view
        /// </summary>
        /// <param name="info">Card design information with all placeholder settings</param>
        /// <returns>Redirect to Guests view on success, or QR Settings on error</returns>
        [HttpPost]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> CardPreview(CardInfo info)
        {
            ViewBag.Icon = "nav-icon fas fa-qrcode";
            SetBreadcrum("Card Preview", "/admin");

            // Load existing card info
            CardInfo cardInfo = db.CardInfo.Where(p => p.EventId == info.EventId).FirstOrDefault();

            // Update placeholder positions (X/Y axis)
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

            // Update font settings for each placeholder
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

            // Update font styles
            cardInfo.FontStyleAddText = info.FontStyleAddText;
            cardInfo.FontStyleMobNo = info.FontStyleMobNo;
            cardInfo.FontStyleName = info.FontStyleName;
            cardInfo.FontStyleNos = info.FontStyleNos;

            // Update text alignments
            cardInfo.FontAlignment = info.FontAlignment;
            cardInfo.AddTextFontAlignment = info.AddTextFontAlignment;
            cardInfo.ContactNoAlignment = info.ContactNoAlignment;
            cardInfo.NosAlignment = info.NosAlignment;

            // Update barcode dimensions
            if (info.BarcodeWidth != null)
                cardInfo.BarcodeWidth = info.BarcodeWidth;
            if (info.BarcodeHeight != null)
                cardInfo.BarcodeHeight = info.BarcodeHeight;

            // Update selected placeholders and right-to-left axis positions
            cardInfo.SelectedPlaceHolder = info.SelectedPlaceHolder;
            cardInfo.NameRightAxis = info.NameRightAxis;
            cardInfo.ContactRightAxis = info.ContactRightAxis;
            cardInfo.NosRightAxis = info.NosRightAxis;
            cardInfo.AddTextRightAxis = info.AddTextRightAxis;

            db.SaveChanges();

            // Get configuration paths
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string barcodePath = _configuration.GetSection("Uploads").GetSection("Barcode").Value;
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            string path = _configuration.GetSection("Uploads").GetSection("Card").Value;

            // Set QR code URL
            if (!string.IsNullOrEmpty(cardInfo.BarcodeColorCode))
            {
                ViewBag.Barcode = cardInfo.BarcodeColorCode;
            }
            else
            {
                string cloudName = _configuration.GetSection("CloudinarySettings").GetSection("CloudName").Value;
                ViewBag.Barcode = $"https://res.cloudinary.com/{cloudName}/image/upload/QR/{cardInfo.EventId}/{cardInfo.EventId}.png";
            }

            // Validate background image
            if (string.IsNullOrWhiteSpace(cardInfo.BackgroundImage))
            {
                TempData["message2"] = null;
                TempData["message"] = "Background image not uploaded. Please upload background image in QR Settings first.";
                return RedirectToAction("QRSettings", "admin", new { id = info.EventId });
            }

            using HttpClient client = new HttpClient();
            var imageUrl = cardInfo.BackgroundImage;

            // Validate URL
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                TempData["message2"] = null;
                TempData["message"] = "Invalid background image URL. Please upload the background image again in QR Settings.";
                return RedirectToAction("QRSettings", "admin", new { id = info.EventId });
            }

            // Load image and calculate zoom ratio
            byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            using MemoryStream ms = new MemoryStream(imageData);
            Image img = Image.FromStream(ms);
            ViewBag.ImageWidth = img.Width;
            ViewBag.ImageHeight = img.Height;

            // Calculate zoom ratio for images wider than 900px
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

            // Generate preview card template
            await GenerateCardAsync(cardInfo, barcodePath, cardPreview, path, (float)zoomRatio);

            ViewBag.Fonts = new SelectList(GetFonts(), "Name", "Name");
            TempData["successMessage"] = "Card template saved successfully!";

            string cardPreviews = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;

            // Log audit trail
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _auditLogService.AddAsync(userId, info.EventId, DAL.Enum.ActionEnum.UpdateCardDesign);

            // Redirect to Guests view
            return RedirectToAction("Guests", "Admin", new { id = info.EventId });
        }

        #endregion

        #region Card Generation and Refresh

        /// <summary>
        /// GET: Admin/GetCurrentRefreshUpdates
        /// Returns card generation progress for an event
        /// Compares total guests vs created card files
        /// Used for progress tracking during bulk refresh operations
        /// </summary>
        /// <param name="id">Event ID to check progress for</param>
        /// <returns>JSON with totalEventCards and totalCreatedCards counts</returns>
        [HttpGet]
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> GetCurrentRefreshUpdates(int id)
        {
            // Count total guests for this event
            int eventCardsCount = await db.Guest.Where(e => e.EventId == id)
                .AsNoTracking()
                .CountAsync();

            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;
            double createdFiles = 0;

            // Count created card files if folder exists
            if (await _blobStorage.FolderExistsAsync(environment + cardPreview + "/" + id))
            {
                createdFiles = await _blobStorage.CountFilesInFolderAsync(environment + cardPreview + "/" + id);
            }

            return Json(new { totalEventCards = eventCardsCount, totalCreatedCards = createdFiles });
        }

        /// <summary>
        /// POST: Admin/RefreshAll
        /// Regenerates all invitation cards for an event
        /// Skips guests that already have current cards
        /// Uses parallel processing (1 core) for card generation
        /// Logs refresh action in audit trail
        /// </summary>
        /// <param name="id">Event ID to refresh cards for</param>
        /// <param name="type">Refresh type (not currently used)</param>
        /// <returns>JSON response with success status</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> RefreshAll(int id, int type)
        {
            // Validate operator access
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

                // Get all guests for this event
                List<Guest> guests = await db.Guest.Where(p => p.EventId == id)
                    .AsNoTracking()
                    .ToListAsync();

                // Check existing files and exclude guests with current cards
                if (await _blobStorage.FolderExistsAsync(environment + cardPreview + "/" + id))
                {
                    files = await _blobStorage.GetFolderFilesAsync(environment + cardPreview + "/" + id + "/", cancellationToken: default);
                    foreach (var file in files)
                    {
                        // Remove guests whose cards already exist
                        guests.RemoveAll(e => environment + cardPreview + "/" + id + "/" + "E00000" + id + "_" + e.GuestId + "_" + e.NoOfMembers + ".jpg" == file);
                    }
                }

                // Get card design settings
                var card = await db.CardInfo.Where(p => p.EventId == id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                string guestcode = _configuration.GetSection("Uploads").GetSection("Guestcode").Value;
                string path = _configuration.GetSection("Uploads").GetSection("Card").Value;

                // Validate required folders exist
                if (!await _blobStorage.FolderExistsAsync(environment + guestcode) || !await _blobStorage.FolderExistsAsync(environment + path))
                    return Json(new Response { succeeded = false, result = "Directory not exist" });

                // Process guests in parallel (limited to 1 core to avoid overload)
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 1
                };

                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    await RefreshQRCode(guest, card);
                    await RefreshCard(guest, id, card, cardPreview, guestcode, path);
                });

                // Log audit trail
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

        /// <summary>
        /// POST: Admin/DownloadAll
        /// Downloads all generated cards for an event as a ZIP file
        /// Filename based on event title with spaces replaced by underscores
        /// Logs download action in audit trail
        /// </summary>
        /// <param name="id">Event ID to download cards for</param>
        /// <returns>ZIP file with all event cards</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public async Task<IActionResult> DownloadAll(int id)
        {
            var userId = Int32.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Validate operator access
            if (HasOperatorRole())
            {
                var isOperatorHasAccess = db.EventOperator
                     .Any(e => e.OperatorId == userId && e.EventId == id);

                if (!isOperatorHasAccess)
                    return new RedirectToActionResult(AppAction.AccessDenied, AppController.Login, new { });
            }

            // Get event info for filename
            var eventInfo = db.Events.Where(p => p.Id == id).FirstOrDefault();
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;
            string cardPreview = _configuration.GetSection("Uploads").GetSection("Cardpreview").Value;

            // Download all cards as ZIP stream
            MemoryStream zipStream = new MemoryStream();
            zipStream = await _blobStorage.DownloadFilesAsZipStreamAsync(environment + cardPreview + "/" + id);

            // Log audit trail
            await _auditLogService.AddAsync(userId, id, DAL.Enum.ActionEnum.DownloadCards);

            // Return ZIP file with event title as filename
            return File(zipStream, "application/zip", eventInfo.SystemEventTitle.Replace(" ", "_") + ".zip");
        }

        /// <summary>
        /// GET: Admin/DownloadAsPDF
        /// Placeholder for PDF download functionality
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>View for PDF download</returns>
        [AuthorizeRoles("Administrator", "Operator", "Supervisor")]
        public IActionResult DownloadAsPDF(int id)
        {
            return View();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieves list of installed system fonts
        /// Returns fonts ordered alphabetically by name
        /// Used for font selection dropdowns in card design
        /// </summary>
        /// <returns>List of SystemFont objects with ID and Name</returns>
        private List<SystemFont> GetFonts()
        {
            List<SystemFont> fonts = new List<SystemFont>();
            int i = 1;

            // Enumerate installed fonts
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

        /// <summary>
        /// Generates card template image with placeholders and QR code
        /// Draws Arabic placeholder text for: Guest Name, Mobile No, Additional Text, No. of Scan
        /// Applies fonts, colors, sizes, and alignments from card settings
        /// Calculates positions with zoom ratio for proper scaling
        /// Saves template locally in wwwroot/upload/cardpreview folder
        /// </summary>
        /// <param name="cardInfo">Card design settings</param>
        /// <param name="barcodePath">QR code folder path</param>
        /// <param name="cardPreview">Card preview folder path</param>
        /// <param name="path">Card template folder path</param>
        /// <param name="zoomRatio">Zoom ratio for scaling (1.0 = no zoom)</param>
        /// <param name="guestId">Optional guest ID for individual card generation</param>
        private async Task GenerateCardAsync(CardInfo cardInfo, string barcodePath, string cardPreview, string path, float zoomRatio, int guestId = 0)
        {
            using HttpClient client = new HttpClient();
            var imageUrl = cardInfo.BackgroundImage;
            string environment = _configuration.GetSection("Uploads").GetSection("environment").Value;

            // Download background image
            byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            using MemoryStream ms = new MemoryStream(imageData);

            Image background = Image.FromStream(ms);
            Bitmap myBitmap = new Bitmap(background);
            Graphics grap = Graphics.FromImage(myBitmap);

            // Add QR code to card
            Image barcode = await AddBarcodeAsync(cardInfo, barcodePath, grap, zoomRatio);

            // Draw placeholders if selected
            if (cardInfo.SelectedPlaceHolder != null && cardInfo.SelectedPlaceHolder.Length > 0)
            {
                StringFormat frmt = new StringFormat();
                var selectedValues = cardInfo.SelectedPlaceHolder.Split(',');

                // Draw Guest Name placeholder
                if (selectedValues.Contains("Guest Name"))
                {
                    double nameXAxis = (cardInfo.FontAlignment == "right") ? Convert.ToDouble(cardInfo.NameRightAxis) : Convert.ToDouble(cardInfo.ContactNameXaxis);

                    // Set text direction for RTL
                    if (cardInfo.FontAlignment == "right")
                        frmt.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    else
                        frmt = new StringFormat();

                    // Set center alignment
                    if (cardInfo.FontAlignment == "center")
                    {
                        frmt = new StringFormat();
                        frmt.Alignment = StringAlignment.Center;
                    }

                    // Draw Arabic text "اسم الضيف" (Guest Name)
                    grap.DrawString("اسم الضيف", new Font(cardInfo.FontName, (float)(cardInfo.FontSize * 0.63 * zoomRatio))
                    , new SolidBrush(ColorTranslator.FromHtml(cardInfo.FontColor))
                    , new Point((int)(nameXAxis * zoomRatio)
                    , (int)(cardInfo.ContactNameYaxis * zoomRatio)), frmt);
                }

                // Draw Mobile No placeholder
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

                    // Draw Arabic text "رقم الهاتف" (Mobile Number)
                    grap.DrawString("رقم الهاتف", new Font(cardInfo.ContactNoFontName, (float)(cardInfo.ContactNoFontSize * 0.63 * zoomRatio))
                    , new SolidBrush(ColorTranslator.FromHtml(cardInfo.ContactNoFontColor))
                    , new Point((int)(moXAxis * zoomRatio)
                    , (int)(cardInfo.ContactNoYaxis * zoomRatio)), frmt);
                }

                // Draw Additional Text placeholder
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

                    // Draw Arabic text "نص إضافي" (Additional Text)
                    grap.DrawString("نص إضافي", new Font(cardInfo.AltTextFontName, (float)(cardInfo.AltTextFontSize * 0.63 * zoomRatio))
                    , new SolidBrush(ColorTranslator.FromHtml(cardInfo.AltTextFontColor))
                    , new Point((int)(atXAxis * zoomRatio)
                    , (int)(cardInfo.AltTextYaxis * zoomRatio)), frmt);
                }

                // Draw No. of Scan placeholder
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

                    // Draw Arabic text "رقم" (Number)
                    grap.DrawString("رقم", new Font(cardInfo.NosfontName, (float)(cardInfo.NosfontSize * 0.63 * zoomRatio))
                     , new SolidBrush(ColorTranslator.FromHtml(cardInfo.NosfontColor))
                     , new Point((int)(nosXAxis * zoomRatio)
                     , (int)(cardInfo.Nosyaxis * zoomRatio)), frmt);
                }
            }

            // Save template locally in wwwroot/upload/cardpreview
            string localPath = Path.Combine(webHostEnvironment.WebRootPath, "upload", "cardpreview");
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }

            string fileName = guestId > 0 ? $"{guestId}.png" : $"{cardInfo.EventId}.png";
            string fullPath = Path.Combine(localPath, fileName);

            myBitmap.Save(fullPath, ImageFormat.Png);

            // Dispose resources
            grap.Dispose();
            myBitmap.Dispose();
            barcode.Dispose();
            background.Dispose();
        }

        /// <summary>
        /// Adds QR code image to card at specified position
        /// Retrieves QR code from database URL or constructs Cloudinary URL
        /// Applies zoom ratio for proper scaling
        /// </summary>
        /// <param name="cardInfo">Card settings with barcode position and size</param>
        /// <param name="barcodePath">QR code folder path</param>
        /// <param name="grap">Graphics object to draw on</param>
        /// <param name="zoomRatio">Zoom ratio for scaling</param>
        /// <returns>QR code Image object</returns>
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
                // Construct Cloudinary URL: QR/{eventId}/{eventId}.png
                string cloudName = _configuration.GetSection("CloudinarySettings").GetSection("CloudName").Value;
                imageUrl = $"https://res.cloudinary.com/{cloudName}/image/upload/QR/{cardInfo.EventId}/{cardInfo.EventId}.png";
            }

            // Download QR code image
            byte[] imageData = await client.GetByteArrayAsync(imageUrl);
            using MemoryStream ms = new MemoryStream(imageData);

            Image barcode = Image.FromStream(ms);

            // Draw QR code at specified position with zoom ratio applied
            grap.DrawImage(barcode,
                (int)(cardInfo.BarcodeXaxis * zoomRatio) - 2,
                (int)(cardInfo.BarcodeYaxis * zoomRatio) - 2,
                (int)(cardInfo.BarcodeWidth * zoomRatio) + 2,
                (int)(cardInfo.BarcodeWidth * zoomRatio) + 2);

            return barcode;
        }

        #endregion
    }
}
