using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EventPro.Web.Models;
using Serilog;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;


namespace EventPro.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        #region Error Handling

        /// <summary>
        /// Handles all unhandled exceptions and displays a professional error page
        /// </summary>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // Create error view model with all details
            var errorModel = new ErrorViewModel
            {
                RequestId = requestId,
                StatusCode = 500,
                Title = "Internal Server Error",
                Message = "Something went wrong on our end. Our team has been notified.",
                Path = exceptionDetails?.Path ?? "Unknown",
                ExceptionMessage = exceptionDetails?.Error?.Message ?? "Unknown error",
                InnerExceptionMessage = exceptionDetails?.Error?.InnerException?.Message,
                ExceptionType = exceptionDetails?.Error?.GetType().Name ?? "Unknown",
                StackTrace = exceptionDetails?.Error?.StackTrace,
                ShowDetails = _environment.IsDevelopment(),
                Timestamp = DateTime.Now
            };

            // Log the error with full details
            Log.Error($"[ERROR] RequestId: {requestId} | " +
                     $"Path: {errorModel.Path} | " +
                     $"Type: {errorModel.ExceptionType} | " +
                     $"Message: {errorModel.ExceptionMessage} | " +
                     $"InnerException: {errorModel.InnerExceptionMessage} | " +
                     $"StackTrace: {errorModel.StackTrace}");

            return View(errorModel);
        }

        /// <summary>
        /// Handles HTTP status code errors (404, 403, etc.)
        /// </summary>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Home/StatusCode/{code:int}")]
        public IActionResult StatusCode(int code)
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            var errorModel = new ErrorViewModel
            {
                RequestId = requestId,
                StatusCode = code,
                Path = statusCodeFeature?.OriginalPath ?? Request.Path,
                ShowDetails = _environment.IsDevelopment(),
                Timestamp = DateTime.Now
            };

            // Set title and message based on status code
            errorModel.Title = errorModel.GetStatusTitle();
            errorModel.Message = errorModel.GetStatusMessageEnglish();

            // Log the status code error
            Log.Warning($"[HTTP {code}] RequestId: {requestId} | Path: {errorModel.Path}");

            // Set the response status code
            Response.StatusCode = code;

            return View("Error", errorModel);
        }

        #endregion
    }
}
