using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using EventPro.Web.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers.User
{
    public partial class UserController : Controller
    {
        //private readonly IConfiguration _configuration;
        //private readonly EventProContext db;

        //public UserController(
        //   IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //    db = new EventProContext(configuration);
        //}
        //private void SetBreadcrum(string title, string link)
        //{
        //    ViewBag.PageTitle = title;
        //    ViewBag.BackLink = link;
        //}
        //public IActionResult Index()
        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);

        //    if (AppSession.GetCurrentUserRole(this.HttpContext) != "Client")
        //        return RedirectToAction(AppAction.Index, AppController.Login);
        //    SetBreadcrum("Dashboard", "/user");
        //    ViewBag.EventsCount = db.Events.Where(p => p.CreatedFor == AppSession.GetCurrentUserId(HttpContext)).Count();
        //    ViewBag.Icon = "nav-icon fas fa-tachometer-alt";
        //    ViewBag.UpcomingEvents = db.VwEvents.Where(p => p.CreatedFor == AppSession.GetCurrentUserId(HttpContext) && p.EventFrom > DateTime.Now.Date).OrderBy(p => p.EventFrom).Take(5).ToList();
        //    ViewBag.InProgressEvents = db.VwEvents.Where(p => p.CreatedFor == AppSession.GetCurrentUserId(HttpContext) && p.EventFrom >= DateTime.Now.Date && p.EventTo <= DateTime.Now.Date).OrderBy(p => p.EventFrom).Take(5).ToList();
        //    ViewBag.PastEvents = db.VwEvents.Where(p => p.CreatedFor == AppSession.GetCurrentUserId(HttpContext) && p.EventFrom < DateTime.Now.Date && p.EventTo < DateTime.Now.Date).OrderBy(p => p.EventFrom).Take(5).ToList();
        //    return View();
        //}

        //public IActionResult Events()
        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);

        //    if (AppSession.GetCurrentUserRole(this.HttpContext) != "Client")
        //        return RedirectToAction(AppAction.Index, AppController.Login);
        //    ViewBag.Icon = "nav-icon fas fa-calendar";
        //    ViewBag.FilePath = _configuration.GetSection("Uploads").GetSection("Invoice").Value;
        //    SetBreadcrum("Events", "/user");

        //    ViewBag.Events = db.VwEvents.Where(p => p.CreatedFor == AppSession.GetCurrentUserId(HttpContext)).ToList();
        //    return View();
        //}
        //public IActionResult AccessDenied()
        //{
        //    return View();
        //}
        //public IActionResult ViewEvent(int id)

        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);

        //    if (AppSession.GetCurrentUserRole(this.HttpContext) != "Client")
        //        return RedirectToAction(AppAction.Index, AppController.Login);


        //    var eventCheck = db.Events.Where(p => p.Id == id).FirstOrDefault();
        //    if (eventCheck.CreatedFor == null || eventCheck.CreatedFor != AppSession.GetCurrentUserId(HttpContext))
        //    {
        //        return RedirectToAction(AppAction.AccessDenied, AppController.User);
        //    }

        //    ViewBag.Icon = "nav-icon fas fa-pencil-square-o";
        //    ViewBag.CardImage = id + ".png";
        //    var cardInfo = db.CardInfo.Where(p => p.EventId == id).FirstOrDefault();
        //    if (cardInfo == null)
        //    {
        //        return RedirectToAction("QRSettings", "admin", new { id = id });
        //    }
        //    ViewBag.CardInfo = cardInfo;
        //    ViewBag.GuestList = db.Guest.Where(p => p.EventId == id).ToList();
        //    SetBreadcrum("Events", "/user");
        //    return View(db.VwEvents.Where(p => p.Id == id).FirstOrDefault());
        //}

        //public IActionResult Guests(int id)
        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);

        //    if (AppSession.GetCurrentUserRole(this.HttpContext) != "Client")
        //        return RedirectToAction(AppAction.Index, AppController.Login);

        //    var eventCheck = db.Events.Where(p => p.Id == id).FirstOrDefault();
        //    if (eventCheck.CreatedFor == null || eventCheck.CreatedFor != AppSession.GetCurrentUserId(HttpContext))
        //    {
        //        return RedirectToAction(AppAction.AccessDenied, AppController.User);
        //    }

        //    string route = Convert.ToString(this.HttpContext.Request.Query["route"]);
        //    ViewBag.Route = route;
        //    ViewBag.Type = new SelectList(db.EventCategory.ToList(), "EventId", "Category");
        //    ViewBag.Icon = "nav-icon fas fa-pencil-square-o";
        //    ViewBag.EventId = id;
        //    ViewBag.Data = db.VwMiagregationReport.Where(p => p.UserId == AppSession.GetCurrentUserId(HttpContext) && p.EventId == id).ToList();
        //    ViewBag.GuestList = db.VwScannedInfo.Where(p => p.EventId == id).ToList();
        //    SetBreadcrum("Guest", "/");
        //    return View();
        //}

        //[HttpPost]
        //public IActionResult Guests(Guest guest)
        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);

        //    if (AppSession.GetCurrentUserRole(this.HttpContext) != "Client")
        //        return RedirectToAction(AppAction.Index, AppController.Login);
        //    var files = Request.Form.Files;
        //    string path = _configuration.GetSection("Uploads").GetSection("Excel").Value;
        //    string excelConnection = _configuration.GetSection("Database").GetSection("ExcelConnection").Value;
        //    string filename = string.Empty;
        //    bool hasFile = false;
        //    foreach (var file in files)
        //    {
        //        string extension = file.ContentType.ToLower().Replace(@"image/", "");
        //        filename = Guid.NewGuid() + ".xlsx";
        //        using (var fileStream = new FileStream(path + @"\" + filename, FileMode.Create))
        //        {
        //            hasFile = true;
        //            file.CopyTo(fileStream);
        //        }
        //    }
        //    if (hasFile)
        //    {
        //        var excelData = ImportFromExcel.ImportDataFromExcel(path + @"\" + filename, excelConnection);
        //    }
        //    return RedirectToAction("Guests", "admin", new { id = guest.EventId });
        //}

        //[HttpPost]
        //public async Task<IActionResult> Guest(Guest guest)
        //{
        //    //if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //    //    return RedirectToAction(AppAction.Index, AppController.Login);

        //    //if (AppSession.GetCurrentUserRole(this.HttpContext) != "Client")
        //    //    return RedirectToAction(AppAction.Index, AppController.Login);
        //    //SetBreadcrum("Guest", "/");
        //    //AdminController controller = new AdminController(_configuration, null, null, null, null, null,null);
        //    //int eventId = await controller.AddOrModifyGuest(guest);
        //    return RedirectToAction("Guests", "user", new { id = 1});
        //}

        //public IActionResult ScanLogs()
        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);

        //    //if (AppSession.GetCurrentUserRole(this.HttpContext) != "Client")
        //    //    return RedirectToAction(AppAction.Index, AppController.Login);

        //    ViewBag.fromDate = "";
        //    ViewBag.todate = "";
        //    ViewBag.code = "All";
        //    ViewBag.gatekeeper = "All";
        //    ViewBag.eventName = "All";
        //    ViewBag.guestName = "";

        //    ViewBag.Icon = "nav-icon fas fa-cubes";

        //    ViewBag.Events = db.Events.Where(p => p.CreatedFor == AppSession.GetCurrentUserId(HttpContext)).OrderBy(p => p.EventTitle).ToList();
        //    ViewBag.Gatekeeper = db.VwUsers.Where(p => p.RoleName == "Gatekeeper").OrderBy(p => p.FullName).ToList();

        //    SetBreadcrum("Scan Logs", "/");
        //    ViewBag.Logs = db.VwScanHistoryLogs.Where(p => p.CreatedFor == AppSession.GetCurrentUserId(HttpContext)).OrderByDescending(p => p.ScannedOn).ToList();
        //    return View();
        //}
        //[HttpPost]
        //public IActionResult ScanLogs(IFormCollection collection)
        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);

        //    if (AppSession.GetCurrentUserRole(this.HttpContext) != "Client")
        //        return RedirectToAction(AppAction.Index, AppController.Login);

        //    var logs = db.VwScanHistoryLogs.Where(p => p.CreatedFor == AppSession.GetCurrentUserId(HttpContext)).OrderByDescending(p => p.ScannedOn).ToList();

        //    string fromdate = collection["fromdate"];
        //    string todate = collection["todate"];
        //    string code = collection["code"];
        //    string gatekeeper = collection["gatekeeper"];
        //    string events = collection["event"];
        //    string guestName = collection["guestName"];

        //    ViewBag.fromDate = fromdate;
        //    ViewBag.todate = todate;
        //    ViewBag.code = code;
        //    ViewBag.gatekeeper = gatekeeper;
        //    ViewBag.eventName = events;
        //    ViewBag.guestName = guestName;


        //    if (fromdate != null && fromdate != "")
        //    {
        //        DateTime _fromDate = Convert.ToDateTime(fromdate).Date;
        //        logs = logs.Where(p => p.ScannedOn >= _fromDate).ToList();
        //    }

        //    if (todate != null && todate != "")
        //    {
        //        DateTime _todate = Convert.ToDateTime(todate).Date;
        //        logs = logs.Where(p => p.ScannedOn <= _todate).ToList();
        //    }
        //    if (events != "All")
        //        logs = logs.Where(p => p.EventTitle == events).ToList();

        //    if (code != "All")
        //        logs = logs.Where(p => p.ResponseCode == code).ToList();

        //    if (gatekeeper != "All")
        //        logs = logs.Where(p => p.ScanBy == Convert.ToInt32(gatekeeper)).ToList();

        //    if (events != "All")
        //        logs = logs.Where(p => p.EventTitle == events).ToList();

        //    //if (guestName != "")
        //    //    logs = logs.Where(p => p.GuestName.ToLower().Contains(guestName.ToLower())).ToList();


        //    ViewBag.Icon = "nav-icon fas fa-cubes";
        //    ViewBag.Events = db.Events.Where(p => p.CreatedFor == AppSession.GetCurrentUserId(HttpContext)).OrderBy(p => p.EventTitle).ToList();
        //    ViewBag.Gatekeeper = db.VwUsers.Where(p => p.RoleName == "Gatekeeper").OrderBy(p => p.FullName).ToList();

        //    SetBreadcrum("Scan Logs", "/");
        //    ViewBag.Logs = logs;
        //    return View();
        //}
    }
}
