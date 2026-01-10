using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.API.Models;
using EventPro.DAL;
using EventPro.DAL.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ScanController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;

        public ScanController(
            IConfiguration configuration,
             EventProContext context)
        {
            _configuration = configuration;
            db = context;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddScan addScan)
        {
            // Log.Information($"QR Code:{addScan.QRcode}");
            IHeaderDictionary headers;
            ScanHistory scanHistory = new ScanHistory();
            UserToken userToken = new UserToken();
            string decText = string.Empty;
            try
            {
                headers = Request.Headers;
                Log.Information($"post scan Started for headers:{JsonConvert.SerializeObject(headers)} & QRCODE :{addScan.QRcode} ");
                if (!headers.Where(p => p.Key == "Authorization").Any())
                {
                    scanHistory.ResponseCode = "Authorization";
                    scanHistory.Response = "Authorization token is missing";
                    await db.ScanHistory.AddAsync(scanHistory);
                    await db.SaveChangesAsync();
                    Log.Information($"Unauthorized:Authorization token is missing");
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(addScan.QRcode))
                {
                    scanHistory.ResponseCode = "QR Code Missing";
                    scanHistory.Response = "QR Code is required";
                    await db.ScanHistory.AddAsync(scanHistory);
                    await db.SaveChangesAsync();
                    Log.Information($"BadRequest:QR Code is required");
                    return BadRequest("QR Code is required");
                }

                EventProFun.DecodeToken(headers, userToken);

                //string cyperText = addScan.Qrcode;
                decText = EventProCrypto.DecryptString(_configuration.GetSection("SecurityKey").Value, addScan.QRcode);

                scanHistory.ScanBy = userToken.UserId;

                if (userToken.Issuer != _configuration["JWT:ValidIssuer"])
                {
                    scanHistory.ResponseCode = "Invalid Issuer";
                    scanHistory.Response = "Token is not valid for this request";
                    await db.ScanHistory.AddAsync(scanHistory);
                    await db.SaveChangesAsync();
                    Log.Information($"Unauthorized:Token is not valid for this request");
                    return Unauthorized("Token is not valid for this request");
                }

                if (userToken.ValidTo < DateTime.Now)
                {
                    scanHistory.ResponseCode = "Expire";
                    scanHistory.Response = "Token Expire";
                    await db.ScanHistory.AddAsync(scanHistory);
                    await db.SaveChangesAsync();
                    Log.Information($"Token Expire");
                    return Unauthorized("Token Expire");
                }

                int decNumber = 0;
                bool validCode = int.TryParse(decText, out decNumber);
                if (!validCode)
                {
                    Log.Information($"Invalid QR Code:{addScan.QRcode} & decText:{decText}");
                    return BadRequest(new { message = $"Error occured,please try again", Name = string.Empty, No = "0", EventName = string.Empty, Scanned = "0" });
                }

                var result = await db.Set<ValidateQRCodeResult>()
                           .FromSqlInterpolated($"exec Proc_ValidateScannedQRCode  @GuestID ={decNumber}, @GatekeeperId = {userToken?.UserId}")
                           .AsAsyncEnumerable().FirstOrDefaultAsync();

                if (result != null)
                {
                    Log.Information($"QR Code Scanned Status:{result.succeed} message:{result.message} for Guest {decNumber} NoOfMembers:{result.No} Scanned by:{userToken?.UserId}");
                    if (result.succeed == true)
                        return Ok(new { message = result.message, Name = result.Name, No = result.No, EventName = string.Empty });
                    else
                        return BadRequest(new { message = result.message, Name = result.Name, No = result.No, EventName = string.Empty, Scanned = result.Scanned });
                }
                else
                {
                    Log.Error($"Error occured ,userToken{userToken?.UserId}");
                    return BadRequest(new { message = $"Error occured,please try again", Name = "-", No = "0", EventName = string.Empty, Scanned = "0" });
                }

            }
            catch (Exception ex)
            {
                Log.Error($"Invalid QR Code from Exception: {ex.Message} -GuestID ={decText} & QRcode={addScan.QRcode},userToken{userToken?.UserId}");
                return BadRequest(new { message = $"Error occured,please try again", Name = "-", No = "0", EventName = string.Empty, Scanned = "0" });

            }
        }
    }
}