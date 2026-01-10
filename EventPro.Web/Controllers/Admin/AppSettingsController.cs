using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using EventPro.Web.Filters;
using EventPro.Services.TwilioService.Interface;
using EventPro.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers.Admin
{
    public class AppSettingsController : Controller
    {
        private readonly EventProContext db;
        private readonly ITwilioService _twilioService;
        public AppSettingsController(IConfiguration configuration, ITwilioService twilioService)
        {
            db = new EventProContext(configuration);
            _twilioService = twilioService;         
        }

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

        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public async Task<IActionResult> DefautlWhatsApp(DefaultWhatsappSettings whatsappSettings)
        {
            if (!ModelState.IsValid)
            {
                return View(whatsappSettings);
            }
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

        [AuthorizeRoles("Administrator")]
        [HttpGet]
        public async Task<IActionResult> DefaultTemplates(int? id)
        {
            ViewBag.Update = 1;
            SetBreadcrum("Settings / Edit Templates", "/");

            var twilioProfile = new TwilioProfileSettings();

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

            if (id != null && id != -1)
            {
                twilioProfile = await db.TwilioProfileSettings
                                .AsNoTracking()
                                .FirstOrDefaultAsync(e => e.Id == id);
            }
            if (id == -1)
            {
                ViewBag.Update = 0;
                SetBreadcrum("Settings / Create Profile", "/");
                twilioProfile = new TwilioProfileSettings();
            }

            ViewBag.Profiles = new SelectList(await db.TwilioProfileSettings
                 .AsNoTracking()
                 .ToListAsync(),
                "Id", "Name");
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

        [AuthorizeRoles("Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetProfileSettingsData(int id)
        {
            var twilioProfile = await db.TwilioProfileSettings
                               .AsNoTracking()
                               .FirstOrDefaultAsync(e => e.Id == id);

            return Json(twilioProfile);
        }

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

        [AuthorizeRoles("Administrator")]
        [HttpPost]
        public async Task<IActionResult> CreateDefaultTemplates(TwilioProfileSettings twilioProfileSettings)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "????? ????? ???? ????????" });
            }
            twilioProfileSettings.Id = 0;
            await db.TwilioProfileSettings.AddAsync(twilioProfileSettings);
            try
            {
                await db.SaveChangesAsync();
            }
            catch
            {
                return Json(new { success = false, message = "!??? ??? ??" });
            }

            return Json(new { success = true });
        }

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

        private void SetBreadcrum(string title, string link)
        {
            ViewBag.PageTitle = title;
            ViewBag.BackLink = link;
        }


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
            // run all checks concurrently
            var tasks = accounts.Select(acc => _twilioService.CheckSingleAccountAsync(acc));
            await Task.WhenAll(tasks);
        }
    }
}
