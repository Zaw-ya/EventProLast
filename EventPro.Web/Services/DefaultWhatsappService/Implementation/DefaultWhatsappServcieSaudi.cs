using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using EventPro.Web.Services.DefaultWhatsapp.Interface;

namespace EventPro.Web.Services.DefaultWhatsapp.Implementation
{
    public class DefaultWhatsappServcieSaudi : DefaultWhatsappService.Implementation.DefaultWhatsappService, IDefaultWhatsappServcieSaudi
    {
        public DefaultWhatsappServcieSaudi(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor)
        {
        }
    }
}












