using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using EventPro.DAL.ViewModels;
using EventPro.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers
{
    public class ConfirmationMessageResponsesKeywordsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        public ConfirmationMessageResponsesKeywordsController(
            IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
        }
        public IActionResult Index()
        {
            var access = AccessService.AccessVerification(this.HttpContext);
            if (access != null) return access;

            ViewBag.confirmationButtonResponses = new SelectList(
                 new List<SelectListItem>
                 {
                    new SelectListItem { Text = "All", Value = ""},
                    new SelectListItem { Text = "Confirmation", Value = "confirm_button"},
                    new SelectListItem { Text = "Decline", Value = "decline_button"},
                    new SelectListItem { Text = "Event Location", Value = "eventlocation_button"}
                 }, "Value", "Text", ViewBag.SelectedRole);

            ViewBag.Icon = "nav-icon fas fa-thumbs-up";
            SetBreadcrum("Confirmation Responses", "/");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetConfirmationButtons(string? type)
        {
            var access = AccessService.AccessVerification(this.HttpContext);
            if (access != null) return access;

            var pageSize = int.Parse(Request.Form["length"]);
            var skip = int.Parse(Request.Form["start"]);

            var searchValue = Request.Form["search[value]"];
            searchValue = searchValue.ToString().Trim();

            DateTime searchDate;
            IQueryable<ConfirmationMessageResponsesKeyword> responsesKeywords = db.ConfirmationMessageResponsesKeyword
                .Include(e => e.CreatedByUser)
                .Include(e => e.UpdatedByUser)
                .Where(e =>
                    string.IsNullOrEmpty(searchValue)
                    || e.KeywordKey.Contains(searchValue)
                    || e.KeywordValue.Contains(searchValue)
                    || e.LanguageCode.Contains(searchValue)
                    || (DateTime.TryParse(searchValue, out searchDate) &&
                        (e.CreatedOn.Date == searchDate.Date ||
                         (e.UpdatedOn.HasValue && e.UpdatedOn.Value.Date == searchDate.Date)))
                    || (e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName).Contains(searchValue)
                    || (e.UpdatedByUser != null &&
                        (e.UpdatedByUser.FirstName + " " + e.UpdatedByUser.LastName).Contains(searchValue))
                )
                .AsNoTracking();


            if (!string.IsNullOrEmpty(type))
            {
                responsesKeywords = responsesKeywords.Where(e => e.KeywordKey == type);
            }

            responsesKeywords = responsesKeywords.OrderByDescending(p => p.Id);

            var result = await responsesKeywords.Skip(skip).Take(pageSize).ToListAsync();


            List<ConfirmationButtonsResponsesVM> confirmationButtonsResponsesVMs = new();
            int counter = 0;
            foreach (var buttonResponse in result)
            {
                counter++;
                ConfirmationButtonsResponsesVM confirmationButtonsResponsesVM = new(buttonResponse);
                confirmationButtonsResponsesVM.counter = counter;
                confirmationButtonsResponsesVMs.Add(confirmationButtonsResponsesVM);
            }

            var recordsTotal = await responsesKeywords.CountAsync();
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = confirmationButtonsResponsesVMs
            };

            return Ok(jsonData);

        }


        [HttpPost]
        public async Task<IActionResult> AddButtonResponse([FromBody] ConfirmationMessageResponsesKeywordVM model)
        {
            var access = AccessService.AccessVerification(this.HttpContext);
            if (access != null) return access;

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "??? ??? ??!" });

            if (await db.ConfirmationMessageResponsesKeyword.AnyAsync(e => e.KeywordValue == model.KeywordValue))
                return Json(new { success = false, message = "??? ????? ????? ?? ???!" });

            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var ResponseButton = new ConfirmationMessageResponsesKeyword()
            {
                KeywordKey = model.KeywordKey,
                KeywordValue = model.KeywordValue,
                LanguageCode = model.LanguageCode,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
            };

            await db.ConfirmationMessageResponsesKeyword.AddAsync(ResponseButton);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "?? ????? ?????" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateButtonResponse([FromBody] ConfirmationMessageResponsesKeywordVM model)
        {
            var access = AccessService.AccessVerification(this.HttpContext);
            if (access != null) return access;

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "??? ??? ??!" });

            if (await db.ConfirmationMessageResponsesKeyword.AnyAsync(e => e.KeywordValue == model.KeywordValue))
                return Json(new { success = false, message = "??? ????? ????? ?? ???!" });

            var userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var ResponseButton = await db.ConfirmationMessageResponsesKeyword
                .Where(e => e.Id == model.Id)
                .FirstOrDefaultAsync();

            ResponseButton.KeywordKey = model.KeywordKey;
            ResponseButton.KeywordValue = model.KeywordValue;
            ResponseButton.LanguageCode = model.LanguageCode;
            ResponseButton.UpdatedOn = DateTime.Now;
            ResponseButton.UpdatedBy = userId;

            db.ConfirmationMessageResponsesKeyword.Update(ResponseButton);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "?? ????? ?????" });
        }

        public async Task<IActionResult> DeleteButtonResponse(int id)
        {
            var access = AccessService.AccessVerification(this.HttpContext);
            if (access != null) return access;

            var model = await db.ConfirmationMessageResponsesKeyword
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            if (model == null)
                return Json(new { success = false, message = "??? ??? ??!" });

            db.ConfirmationMessageResponsesKeyword.Remove(model);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "?? ??? ?????" });
        }


        private void SetBreadcrum(string title, string link)
        {
            ViewBag.PageTitle = title;
            ViewBag.BackLink = link;
        }
    }
}
