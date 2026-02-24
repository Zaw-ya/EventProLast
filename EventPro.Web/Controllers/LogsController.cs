using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using EventPro.Web.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EventPro.Web.Controllers
{
    public class LogsController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public LogsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [AuthorizeRoles("Administrator")]
        public IActionResult Index()
        {
            var logsPath = Path.Combine(_env.ContentRootPath, "Logs");

            var files = new List<LogFileInfo>();

            if (Directory.Exists(logsPath))
            {
                files = Directory.GetFiles(logsPath, "*.txt")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => new LogFileInfo
                    {
                        FileName = f.Name,
                        Size = f.Length,
                        LastModified = f.LastWriteTime
                    })
                    .ToList();
            }

            return View(files);
        }

        [AuthorizeRoles("Administrator")]
        public IActionResult ViewFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest();

            // Prevent path traversal
            fileName = Path.GetFileName(fileName);

            var filePath = Path.Combine(_env.ContentRootPath, "Logs", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Log file not found.");

            string content;
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }

            ViewBag.FileName = fileName;
            ViewBag.Content = content;

            return View();
        }

        [AuthorizeRoles("Administrator")]
        public IActionResult DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest();

            fileName = Path.GetFileName(fileName);

            var filePath = Path.Combine(_env.ContentRootPath, "Logs", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Log file not found.");

            return PhysicalFile(filePath, "text/plain", fileName);
        }
    }

    public class LogFileInfo
    {
        public string FileName { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }

        public string SizeFormatted
        {
            get
            {
                if (Size < 1024) return $"{Size} B";
                if (Size < 1024 * 1024) return $"{Size / 1024.0:F1} KB";
                return $"{Size / (1024.0 * 1024.0):F1} MB";
            }
        }
    }
}
