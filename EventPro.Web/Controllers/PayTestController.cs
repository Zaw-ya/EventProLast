using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using EventPro.Web.Common;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;

namespace EventPro.Web.Controllers
{

    public class PaymentStruct
    {
        public int PayId { get; set; }
        public string Product { get; set; }
    }
    public class PayTestController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        private readonly string apiKey = "SWJN29WLKN-JDTWRBNN92-GWNWJKZ9ML";
        //private readonly string clientKey = "C9KMQT-HQ9N6D-7VR2QQ-HPPNVR";
        private readonly int profileId = 90990;
        private readonly string AppURL;
        public PayTestController(
            IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
            AppURL = _configuration.GetSection("AppURL").Value;
        }
        public IActionResult Index()
        {

            return View();
        }

        public new IActionResult Response()
        {
            if (AppSession.GetSession(this.HttpContext, SesionConstant.TranId) != null)
            {
                string tranId = AppSession.GetSession(this.HttpContext, SesionConstant.TranId);
                if (tranId.Length > 0)
                {
                    var data = new
                    {
                        profile_id = profileId,
                        tran_ref = tranId
                    };
                    var stringMessage = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    var bytes = Encoding.UTF8.GetBytes(stringMessage);

                    string URI = "https://secure.paytabs.sa/payment/query";
                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add("authorization", apiKey);
                        var result = client.UploadData(URI, bytes);
                        string response = Encoding.Default.GetString(result);
                        dynamic resp = JsonConvert.DeserializeObject(response);
                        string msg = resp.payment_result.response_message;
                        TempData["message"] = msg;
                    }
                }

            }

            return View();
        }


        [HttpPost]
        public IActionResult Index(PaymentStruct payStuct)
        {

            var data = new
            {
                profile_id = profileId,
                tran_type = "sale",
                tran_class = "ecom",
                cart_id = new Guid(),
                cart_description = "Sandox testing",
                cart_currency = "SAR",
                cart_amount = 45,
                callback = AppURL + "paycallback",
                _return = AppURL + "paytest/Response"
            };
            var stringMessage = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            stringMessage = stringMessage.Replace("_return", "return");
            var bytes = Encoding.UTF8.GetBytes(stringMessage);

            string URI = "https://secure.paytabs.sa/payment/request";
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("authorization", apiKey);
                var result = client.UploadData(URI, bytes);
                string response = Encoding.Default.GetString(result);
                dynamic resp = JsonConvert.DeserializeObject(response);
                string redirectURI = resp.redirect_url;
                AppSession.SetSession(this.HttpContext, SesionConstant.TranId, Convert.ToString(resp.tran_ref));
                return Redirect(redirectURI);
            }
        }
    }
}
