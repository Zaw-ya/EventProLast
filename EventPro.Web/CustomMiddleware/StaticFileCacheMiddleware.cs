using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EventPro.Web.CustomMiddleware
{
    public class StaticFileCacheMiddleware
    {
        private readonly RequestDelegate _next;

        public StaticFileCacheMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (path != null && IsCacheableStaticAsset(path))
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
                    return Task.CompletedTask;
                });
            }

            await _next(context);
        }

        private bool IsCacheableStaticAsset(string path)
        {
            return path.EndsWith(".js") ||
                   path.EndsWith(".css") ||
                   path.EndsWith(".woff2") ||
                   path.EndsWith(".woff") ||
                   path.EndsWith(".ttf") ||
                   path.EndsWith(".eot") ||
                   path.EndsWith(".svg") ||
                   path.EndsWith(".jpg") ||
                   path.EndsWith(".jpeg") ||
                   path.EndsWith(".png") ||
                   path.EndsWith(".gif") ||
                   path.EndsWith(".webp");
        }
    }
}
