using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using EventPro.Web.Services.DefaultWhatsapp.Implementation;
using EventPro.Web.Services.DefaultWhatsapp.Interface;
using EventPro.Web.Services.DefaultWhatsappService.Interface;

namespace EventPro.Web.Services.DefaultWhatsappService.Implementation
{
    public class UnitOFWorkDefaultWhatsappService : IUnitOFWorkDefaultWhatsappService
    {
        private readonly IConfiguration _configuration;
        public UnitOFWorkDefaultWhatsappService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            DefaultWhatsappService = new DefaultWhatsappService(_configuration, httpContextAccessor);
            DefaultWhatsappServcieBahrain = new DefaultWhatsappServcieBahrain(_configuration, httpContextAccessor);
            defaultWhatsappServcieKuwait = new DefaultWhatsappServcieKuwait(_configuration, httpContextAccessor);
            defaultWhatsappServcieSaudi = new DefaultWhatsappServcieSaudi(_configuration, httpContextAccessor);
        }
        public IDefaultWhatsappService DefaultWhatsappService { get; private set; }

        public IDefaultWhatsappServcieBahrain DefaultWhatsappServcieBahrain { get; private set; }

        public IDefaultWhatsappServcieKuwait defaultWhatsappServcieKuwait { get; private set; }

        public IDefaultWhatsappServcieSaudi defaultWhatsappServcieSaudi { get; private set; }
    }
}
