using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.API.Models;
using EventPro.DAL.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        public ProfileController(
            IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var headers = Request.Headers;
            UserToken userToken = new UserToken();
            EventProFun.DecodeToken(headers, userToken);
            if (userToken.Issuer != _configuration["JWT:ValidIssuer"])
            {
                return Unauthorized("Token is not valid for this request");
            }
            if (userToken.ValidTo < DateTime.Now)
            {
                return Unauthorized("Token Expire");
            }
            var allowed = await db.ScanHistory.Where(p => p.ScanBy == userToken.UserId && p.ResponseCode == "Allowed").CountAsync();
            var declined = await db.ScanHistory.Where(p => p.ScanBy == userToken.UserId && p.ResponseCode == "Declined").CountAsync();
            var todaysScan = await db.ScanHistory.Where(p => p.ScanBy == userToken.UserId && p.ResponseCode == "Allowed" && p.ScannedOn >= DateTime.Today.Date).CountAsync();
            var profileInfo = await db.Users.Where(p => p.UserId == userToken.UserId).FirstOrDefaultAsync();
            return Ok(new
            {
                FullName = profileInfo.FirstName + " " + profileInfo.LastName,
                Email = profileInfo.Email,
                PrimaryContactNo = profileInfo.PrimaryContactNo,
                SuccessScan = allowed,
                Decline = declined,
                TodaysScan = todaysScan
            });
        }
    }
}
