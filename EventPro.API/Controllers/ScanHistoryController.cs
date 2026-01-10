using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.API.Models;
using EventPro.DAL.Models;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ScanHistoryController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        public ScanHistoryController(
            IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
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

                string eventId = headers.Where(p => p.Key.ToLower() == "eventid").Select(p => p.Value).FirstOrDefault();

                string page = headers.Where(p => p.Key.ToLower() == "pageno").Select(p => p.Value).FirstOrDefault();
                var pageNo = 1;
                if (!string.IsNullOrEmpty(page) && Convert.ToInt32(page) > 1)
                    pageNo = Convert.ToInt32(page);

                int PageSize = 30;
                if (!string.IsNullOrEmpty(_configuration["PageSize"]) && int.Parse(_configuration["PageSize"]) > 1)
                    PageSize = int.Parse(_configuration["PageSize"]);
                string guestID = headers.Where(p => p.Key == "guestid").Select(p => p.Value).FirstOrDefault();
                IQueryable<EventScanHistory> eventScanHistories;
                if (userToken.Role == "Client")
                    eventScanHistories =
                                db.ScanHistory.Include(x => x.Guest).ThenInclude(sh => sh.Event)
                                  .Where(x => x.Guest.Archived == null &&
                                              x.Guest.EventId == Convert.ToInt32(eventId) &&
                                              x.GuestId == Convert.ToInt32(guestID))
                                   .OrderByDescending(p => p.ScannedOn).Select(sch => new EventScanHistory()
                                   {
                                       ScannedOn = sch.ScannedOn.Value.ToLocalTime(),
                                       ResponseCode = sch.ResponseCode,
                                       Response = sch.Response,
                                       GuestFullName = sch.Guest.FirstName + ' ' + sch.Guest.LastName,
                                       NoOfMembers = sch.Guest.NoOfMembers
                                   });
                else
                    eventScanHistories =
                                     db.ScanHistory.Include(x => x.Guest).ThenInclude(sh => sh.Event)
                                       .Where(x => x.ScanBy == userToken.UserId &&
                                                 x.Guest.Archived == null &&
                                                 x.Guest.EventId == Convert.ToInt32(eventId))
                                        .OrderByDescending(p => p.ScannedOn).Select(sch => new EventScanHistory()
                                        {
                                            ScannedOn = sch.ScannedOn.Value.ToLocalTime(),
                                            ResponseCode = sch.ResponseCode,
                                            Response = sch.Response,
                                            GuestFullName = sch.Guest.FirstName + ' ' + sch.Guest.LastName,
                                            NoOfMembers = sch.Guest.NoOfMembers
                                        });

                long count = 0;
                if (pageNo == 1)
                    count = await eventScanHistories.LongCountAsync();
                eventScanHistories = eventScanHistories.Skip((Convert.ToInt32(pageNo) - 1) * PageSize).Take(PageSize);
                long noOfPages = 0;
                if (count > 0)
                    noOfPages = (long)Math.Ceiling((double)count / PageSize);

                var result = new EntityListResult<EventScanHistory>
                { EntityList = await eventScanHistories.ToListAsync(), NoOfPages = noOfPages };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ErrorMessage = ex.Message,
                    FullMessage = ex.StackTrace
                });
            }
        }
    }
}
