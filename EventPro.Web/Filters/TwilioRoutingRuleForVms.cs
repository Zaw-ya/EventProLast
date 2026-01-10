using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EventPro.Web.Filters
{
    public class TwilioRoutingRuleForVms : Attribute, IAsyncActionFilter
    {

        private readonly bool _isPrimary;
        private readonly string _primaryVmUrl;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpContext _httpContext;
        public TwilioRoutingRuleForVms(IHttpClientFactory httpClientFactory,
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

            if (_isPrimary)
            {
                await next(); // This is the primary, handle the request
                return;
            }

            // Proxy to primary
            var request = httpContext.Request;
            var url = $"{_primaryVmUrl}{request.Path}{request.QueryString}";
            var method = new HttpMethod(request.Method);

            var client = _httpClientFactory.CreateClient();
            var requestMessage = new HttpRequestMessage(method, url);

            // Forward all headers
            foreach (var header in request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    // Add to content headers if not added to main headers
                    requestMessage.Content ??= new StreamContent(Stream.Null); // avoid null Content
                    requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            // Forward body for non-GET/HEAD requests
            if (method != HttpMethod.Get && method != HttpMethod.Head)
            {
                request.EnableBuffering();
                request.Body.Position = 0;

                // Copy the request body to a memory stream
                var bodyStream = new MemoryStream();
                await request.Body.CopyToAsync(bodyStream);
                bodyStream.Position = 0; // Reset position for reading

                request.Body.Position = 0; // Reset again for fallback local execution

                var streamContent = new StreamContent(bodyStream);

                // Copy only content-related headers
                var contentHeaderKeys = new[] { "Content-Type", "Content-Length" };
                foreach (var headerKey in contentHeaderKeys)
                {
                    if (request.Headers.TryGetValue(headerKey, out var values))
                    {
                        streamContent.Headers.TryAddWithoutValidation(headerKey, values.ToArray());
                    }
                }

                requestMessage.Content = streamContent;
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(7));
                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cts.Token);

                // Check if the status code is not 200 OK
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    await next();
                    return;
                }

                // Copy status code
                context.HttpContext.Response.StatusCode = (int)response.StatusCode;

                // Clear existing headers (optional, for safety)
                context.HttpContext.Response.Headers.Clear();

                // Copy response headers
                foreach (var header in response.Headers)
                {
                    context.HttpContext.Response.Headers[header.Key] = header.Value.ToArray();
                }
                foreach (var header in response.Content.Headers)
                {
                    context.HttpContext.Response.Headers[header.Key] = header.Value.ToArray();
                }

                // Avoid chunked encoding issues
                context.HttpContext.Response.Headers.Remove("transfer-encoding");

                // Copy body
                await using var responseStream = await response.Content.ReadAsStreamAsync();
                await responseStream.CopyToAsync(context.HttpContext.Response.Body);

            }
            catch (HttpRequestException)
            {
                // Fallback to local handling if primary VM unreachable
                await next();
                return;
            }
            catch (TaskCanceledException ex)
            {
                await next();
                return;
            }
            catch (Exception ex)
            {
                await next();
                return;
            }
        }

    }
}
