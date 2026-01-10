using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReadLogsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        public ReadLogsController(
            IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var duration = DateTime.Now - TimeSpan.FromDays(14);
            var scanHistory = await db.ScanHistory
                .Where(e => e.ScannedOn > duration)
                .ToListAsync();
            /*
             * Unnecessary Re-addition
            foreach (var scan in scanHistory)
            {
                ScanHistory history = new ScanHistory();
                dynamic data = scan;
                history.ScanBy = data.ScanBy;
                history.ScannedOn = data.ScannedOn;
                history.ScannedCode = data.ScannedCode;
                history.GuestId = data.GuestId;
                history.ResponseCode = data.ResponseCode;
                history.Response = data.Response;
                history.Guest = data.Guest;
                history.ScanByNavigation = data.ScanByNavigation;
                await db.ScanHistory.AddAsync(history);
            }
            await db.SaveChangesAsync();
            */
            return Ok(scanHistory);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PostLog([FromBody] List<MobileLog> log)
        {
            try
            {
                await db.MobileLog.AddRangeAsync(log);
                await db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error($"error happen in saving MobileLog:{ex.Message}");
                return BadRequest();
            }

        }

    }

}
