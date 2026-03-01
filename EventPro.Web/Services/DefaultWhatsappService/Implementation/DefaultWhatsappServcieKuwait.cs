using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using EventPro.Web.Services.DefaultWhatsapp.Interface;
using OpenQA.Selenium.Chrome;
using EventPro.Business.Storage.Interface;

namespace EventPro.Web.Services.DefaultWhatsapp.Implementation
{
    public class DefaultWhatsappServcieKuwait : DefaultWhatsappService.Implementation.DefaultWhatsappService, IDefaultWhatsappServcieKuwait
    {
        public DefaultWhatsappServcieKuwait(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IBlobStorage blobStorage) : base(configuration, httpContextAccessor, blobStorage)
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












