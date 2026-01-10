using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using EventPro.Kernal.StaticFiles;

namespace EventPro.Utility
{
    public static class HardDriveExploration
    {
        public static IApplicationBuilder UseCustomFileServer(this IApplicationBuilder app)
        {
            var fileProvider = new PhysicalFileProvider(StaticDirectory.CurrentDirectory);
            var requestPath = "/Upload";
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = fileProvider,
                RequestPath = requestPath,
                EnableDirectoryBrowsing = true
            });
            return app;
        }
    }
}
