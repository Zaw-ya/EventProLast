using Microsoft.AspNetCore.Mvc;
using EventPro.DAL.Common;
using EventPro.DAL.Models;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        //private readonly FirbaseAPI _FirbaseAPI;
        //public MessageController(FirbaseAPI firbaseAPI)
        //{
        //    _FirbaseAPI = firbaseAPI;
        //}

        [HttpGet("SendToTopicOrToken")]
        public async Task<bool> SendToTopicOrTokenAsync([FromBody] MessageRequest request)
        {
            FirbaseAPI _FirbaseAPI = new FirbaseAPI();
            return await _FirbaseAPI.NotifyTopicOrTokenAsync(request);
        }

        [HttpPost("NotifyTokens")]
        public async Task<bool> NotifyTokensAsync([FromBody] MessageRequestTokens request)
        {
            FirbaseAPI _FirbaseAPI = new FirbaseAPI();
            return await _FirbaseAPI.NotifyTokensAsync(request);
        }
    }
}
