using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using EventPro.Web.Services.DefaultWhatsapp.Interface;
using EventPro.Business.Storage.Interface;

namespace EventPro.Web.Services.DefaultWhatsapp.Implementation
{
    public class DefaultWhatsappServcieSaudi : DefaultWhatsappService.Implementation.DefaultWhatsappService, IDefaultWhatsappServcieSaudi
    {
        public DefaultWhatsappServcieSaudi(IConfiguration configuration, IHttpContextAccessor httpContextAccessor,ICloudinaryService cloudinaryService) : base(configuration, httpContextAccessor, cloudinaryService)
        {
        }
    }
}












