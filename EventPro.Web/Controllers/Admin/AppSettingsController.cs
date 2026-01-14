using EventPro.DAL.Models;
using EventPro.Services.TwilioService.Interface;
using EventPro.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers.Admin
{
    public class AppSettingsController : Controller
    {
        #region Properties

        private readonly EventProContext db;
        private readonly ITwilioService _twilioService;

        public AppSettingsController(IConfiguration configuration, ITwilioService twilioService)
        {
            db = new EventProContext(configuration);
            _twilioService = twilioService;
        }

        #endregion

        #region App Settings Management

        /// <summary>
        /// GET: AppSettings/Update
        /// Displays the application settings page with Twilio profile dropdown
        /// </summary>
        [AuthorizeRoles("Administrator")]
        [HttpGet]
        public async Task<IActionResult> Update()
        {
            SetBreadcrum("Settings", "/");
            ViewBag.Icon = "nav-icon fas fa-gear";

            var settings = await db.AppSettings.FirstOrDefaultAsync();

            ViewBag.Profiles = new SelectList(await db.TwilioProfileSettings
                                .AsNoTracking()
                                .ToListAsync(),
                                 "Name", "Name");

            return View(settings);
        }

        /// <summary>
        /// POST: AppSettings/Update
        /// Updates application settings in the database
        /// </summary>
        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public async Task<IActionResult> Update(AppSettings appSettings)
        {
            if (!ModelState.IsValid)
            {
                return View(appSettings);
            }

            db.AppSettings.Update(appSettings);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        #endregion

        #region Default WhatsApp Settings

        /// <summary>
        /// GET: AppSettings/DefautlWhatsApp
        /// Displays the default WhatsApp settings configuration page
        /// Loads existing settings or creates new instance if none exist
        /// </summary>
        [AuthorizeRoles("Administrator")]
        [HttpGet]
        public async Task<IActionResult> DefautlWhatsApp()
        {
            SetBreadcrum("Settings / Default Whatsapp ", "/");
            ViewBag.Icon = "nav-icon fas fa-gear";

            var settings = await db.DefaultWhatsappSettings
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (settings == null)
                settings = new DefaultWhatsappSettings();

            return View(settings);
        }

        /// <summary>
        /// POST: AppSettings/DefautlWhatsApp
        /// Updates default WhatsApp settings in the database
        /// Sanitizes input by removing backslashes from various field values
        /// </summary>
        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public async Task<IActionResult> DefautlWhatsApp(DefaultWhatsappSettings whatsappSettings)
        {
            if (!ModelState.IsValid)
            {
                return View(whatsappSettings);
            }

            // Sanitize input - remove backslashes
            whatsappSettings?.MessageTextBox?.Replace("\\", "");
            whatsappSettings?.SendMessageButton?.Replace("\\", "");
            whatsappSettings?.MediaOptions?.Replace("\\", "");
            whatsappSettings?.ImageOption?.Replace("\\", "");
            whatsappSettings?.SendImageButton?.Replace("\\", "");
            whatsappSettings?.ImageTextBox?.Replace("\\", "");
            whatsappSettings?.VideoTextBox?.Replace("\\", "");
            whatsappSettings?.AddNewChatButton?.Replace("\\", "");
            whatsappSettings?.SearchNewChatButton?.Replace("\\", "");
            whatsappSettings?.NewContactButton?.Replace("\\", "");

            db.DefaultWhatsappSettings.Update(whatsappSettings);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        #endregion

        #region Twilio Profile Templates Management

        /// <summary>
        /// GET: AppSettings/DefaultTemplates
        /// Displays Twilio profile settings for viewing/editing
        /// - If id is null: loads the first profile
        /// - If id is -1: creates a new profile
        /// - If id is provided: loads specific profile by id
        /// Also fetches and displays the Twilio account balance
        /// </summary>
        [AuthorizeRoles("Administrator")]
        [HttpGet]
        public async Task<IActionResult> DefaultTemplates(int? id)
        {
            ViewBag.Update = 1;
            SetBreadcrum("Settings / Edit Templates", "/");

            var twilioProfile = new TwilioProfileSettings();

            // Load first profile if no id provided
            if (id == null)
            {
                twilioProfile = await db.TwilioProfileSettings
                     .AsNoTracking()
                     .FirstOrDefaultAsync();
            }

            if (twilioProfile == null)
            {
                twilioProfile = new TwilioProfileSettings();
            }

            // Load specific profile by id
            if (id != null && id != -1)
            {
                twilioProfile = await db.TwilioProfileSettings
                                .AsNoTracking()
                                .FirstOrDefaultAsync(e => e.Id == id);
            }

            // Create new profile mode
            if (id == -1)
            {
                ViewBag.Update = 0;
                SetBreadcrum("Settings / Create Profile", "/");
                twilioProfile = new TwilioProfileSettings();
            }

            // Populate profiles dropdown
            ViewBag.Profiles = new SelectList(await db.TwilioProfileSettings
                 .AsNoTracking()
                 .ToListAsync(),
                "Id", "Name");

            // Fetch Twilio account balance
            string profileBalance = "N/A";
            try
            {
                var balanceResource = await _twilioService.GetBalanceAsync(twilioProfile.AccountSid, twilioProfile.AuthToken);
                var balance = balanceResource.Balance;
                var currency = balanceResource.Currency;
                profileBalance = $"{balance} {currency}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Twilio balance: {ex.Message}");
            }

            ViewBag.Balance = profileBalance;

            return View(twilioProfile);
        }

        /// <summary>
        /// POST: AppSettings/DefaultTemplates
        /// Updates an existing Twilio profile settings
        /// </summary>
        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public async Task<IActionResult> DefaultTemplates(TwilioProfileSettings twilioProfileSettings)
        {
            if (!ModelState.IsValid)
            {
                return View(twilioProfileSettings);
            }

            db.TwilioProfileSettings.Update(twilioProfileSettings);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        /// <summary>
        /// POST: AppSettings/CreateDefaultTemplates
        /// Creates a new Twilio profile in the database
        /// Returns error message if model is invalid or save fails
        /// </summary>
        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public async Task<IActionResult> CreateDefaultTemplates(TwilioProfileSettings twilioProfileSettings)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "الرجاء التأكد من البيانات" });
            }

            twilioProfileSettings.Id = 0;
            await db.TwilioProfileSettings.AddAsync(twilioProfileSettings);

            try
            {
                await db.SaveChangesAsync();
            }
            catch
            {
                return Json(new { success = false, message = "حدث خطأ ما!" });
            }

            return Json(new { success = true });
        }

        /// <summary>
        /// POST: AppSettings/DeleteDefaultTemplates
        /// Deletes a Twilio profile from the database
        /// Returns success or failure status
        /// </summary>
        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public async Task<IActionResult> DeleteDefaultTemplates(TwilioProfileSettings twilioProfileSettings)
        {
            db.TwilioProfileSettings.Remove(twilioProfileSettings);

            try
            {
                await db.SaveChangesAsync();
            }
            catch
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }

        /// <summary>
        /// GET: AppSettings/GetProfileSettingsData
        /// Retrieves specific Twilio profile settings data by id
        /// Returns profile data as JSON for AJAX calls
        /// </summary>
        [AuthorizeRoles("Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetProfileSettingsData(int id)
        {
            var twilioProfile = await db.TwilioProfileSettings
                               .AsNoTracking()
                               .FirstOrDefaultAsync(e => e.Id == id);

            return Json(twilioProfile);
        }

        #endregion

        #region Twilio Account Management

        /// <summary>
        /// GET: AppSettings/CheckAllAccountsAsync
        /// Checks the status of all Twilio accounts concurrently
        /// Runs validation/health checks on all profiles in the system
        /// </summary>
        [HttpGet]
        public async Task CheckAllAccountsAsync()
        {
            List<TwilioProfileSettings> accounts;

            try
            {
                accounts = await db.TwilioProfileSettings.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                return;
            }

            // Run all checks concurrently for better performance
            var tasks = accounts.Select(acc => _twilioService.CheckSingleAccountAsync(acc));
            await Task.WhenAll(tasks);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sets breadcrumb navigation data in ViewBag
        /// </summary>
        /// <param name="title">Page title to display</param>
        /// <param name="link">Back link URL</param>
        private void SetBreadcrum(string title, string link)
        {
            ViewBag.PageTitle = title;
            ViewBag.BackLink = link;
        }

        #endregion
    }
}
