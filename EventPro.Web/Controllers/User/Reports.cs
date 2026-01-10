using Microsoft.AspNetCore.Mvc;
using EventPro.Web.Common;
using System;
using System.Linq;

namespace EventPro.Web.Controllers.User
{
    public partial class UserController : Controller
    {

        //public IActionResult WaTextReport()
        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);
        //    SetBreadcrum("Confirmaton Report", "/");
        //    int userId = Convert.ToInt32(AppSession.GetSession(this.HttpContext, SesionConstant.UserId));
        //    ViewBag.CurrentUser = AppSession.GetCurrentUserId(HttpContext);
        //    ViewBag.Data = db.VwMiagregationReport.Where(p => p.UserId == userId).ToList();
        //    return View();
        //}
        //public IActionResult WaQRReport()
        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);
        //    SetBreadcrum("QR Report", "/");
        //    int userId = Convert.ToInt32(AppSession.GetSession(this.HttpContext, SesionConstant.UserId));
        //    ViewBag.CurrentUser = AppSession.GetCurrentUserId(HttpContext);
        //    ViewBag.Data = db.VwMiagregationReport.Where(p => p.UserId == userId).ToList();
        //    return View();
        //}
        //public IActionResult WaRespReport()
        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);
        //    SetBreadcrum("Response Report", "/");
        //    int userId = Convert.ToInt32(AppSession.GetSession(this.HttpContext, SesionConstant.UserId));
        //    ViewBag.CurrentUser = AppSession.GetCurrentUserId(HttpContext);
        //    ViewBag.Data = db.VwMiagregationReport.Where(p => p.UserId == userId).ToList();
        //    return View();
        //}
        //public IActionResult ScanSummary()
        //{
        //    if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
        //        return RedirectToAction(AppAction.Index, AppController.Login);
        //    SetBreadcrum("Scan Summary", "/");
        //    int userId = Convert.ToInt32(AppSession.GetSession(this.HttpContext, SesionConstant.UserId));
        //    ViewBag.CurrentUser = AppSession.GetCurrentUserId(HttpContext);
        //    ViewBag.Data = db.VwMiagregationReport.Where(p => p.UserId == userId).ToList();
        //    return View();
        //}
    }
}
