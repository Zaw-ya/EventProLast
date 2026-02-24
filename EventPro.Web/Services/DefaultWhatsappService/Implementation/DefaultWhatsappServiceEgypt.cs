using EventPro.Business.Storage.Interface;
using EventPro.Web.Services.DefaultWhatsappService.Interface;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using OpenQA.Selenium.Chrome;

namespace EventPro.Web.Services.DefaultWhatsappService.Implementation
{
    public class DefaultWhatsappServiceEgypt : DefaultWhatsappService, IDefaultWhatsappServcieEgypt
    {
        public DefaultWhatsappServiceEgypt(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ICloudinaryService cloudinaryService) : base(configuration, httpContextAccessor, cloudinaryService)
        {

        }
        public override ChromeOptions ChromeOptions()
        {
            ChromeOptions options = new ChromeOptions();
            options.DebuggerAddress = "localhost:9333";
            options.AddArguments("--remote-debugging-port=9333");
            options.AddArguments("--headless");
            options.AddArguments("--headless=new");
            options.AddArguments("--disable-gpu");
            options.AddArgument("detach");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-renderer-backgrounding");
            options.AddArgument("--disable-background-timer-throttling");
            options.AddArgument("--disable-backgrounding-occluded-windows");
            options.AddArgument("--disable-client-side-phishing-detection");
            options.AddArgument("--disable-crash-reporter");
            options.AddArgument("--disable-oopr-debug-crash-dump");
            options.AddArgument("--no-crash-upload");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-low-res-tiling");
            options.AddArgument("--log-level=3");
            options.AddArgument("--silent");

            return options;
        }
    }
}
