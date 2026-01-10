using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using EventPro.Web.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventPro.Web.Filters
{
    public class ForwardToPrimaryFilter : Attribute, IAsyncActionFilter
    {

        private readonly bool _isPrimary;
        private readonly string _primaryVmUrl;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpContext _httpContext;
        public ForwardToPrimaryFilter(IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
            
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _isPrimary = bool.Parse(_configuration.GetSection("VmEnvironment")["IS_PRIMARY"]);
            _primaryVmUrl = _configuration.GetSection("VmEnvironment")["PRIMARY_VM_URL"];
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            var httpContext = context.HttpContext;

            // ? Bypass auth if this is an internal forwarded request
            if (httpContext.Request.Headers.TryGetValue("X-Internal-Proxy-Approved", out var internalFlag)
                && internalFlag == "true")
            {
                await next();
                return;
            }

            var access = AccessService.AllowAccessForAdministratorAndOperatorOnly(context.HttpContext);
            if (access != null)
            {
                context.Result = new JsonResult(new { success = true, message = "Unauthorized" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            if (_isPrimary)
            {
                await next(); // proceed to action
                return;
            }


            var primaryVmUrl = _primaryVmUrl;
            var request = context.HttpContext.Request;

            var url = $"{primaryVmUrl}{request.Path}{request.QueryString}";
            var method = new HttpMethod(request.Method);

            var client = _httpClientFactory.CreateClient();
            var requestMessage = new HttpRequestMessage(method, url);

            foreach (var header in context.HttpContext.Request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            // ? Add internal trust header
            requestMessage.Headers.Add("X-Internal-Proxy-Approved", "true");

            //// Make sure cookie is forwarded
            //if (context.HttpContext.Request.Headers.TryGetValue("Cookie", out var cookie))
            //{
            //    requestMessage.Headers.Add("Cookie", cookie.ToString());
            //}

            //// For non-GET methods, forward the body
            //if (method != HttpMethod.Get && method != HttpMethod.Head)
            //{
            //    request.EnableBuffering();
            //    request.Body.Position = 0;
            //    requestMessage.Content = new StreamContent(request.Body);
            //    foreach (var header in request.Headers)
            //    {
            //        requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            //    }
            //}

            var response = await client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();

            context.Result = new JsonResult(new { success = true })
            {
                StatusCode = StatusCodes.Status200OK
            };
            return;
        }
    }
}
