using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using EventPro.Web.Services.DefaultWhatsapp.Implementation;
using EventPro.Web.Services.DefaultWhatsapp.Interface;
using EventPro.Web.Services.DefaultWhatsappService.Interface;
using EventPro.Business.Storage.Interface;

namespace EventPro.Web.Services.DefaultWhatsappService.Implementation
{
    public class UnitOFWorkDefaultWhatsappService : IUnitOFWorkDefaultWhatsappService
    {
        public UnitOFWorkDefaultWhatsappService(
        IDefaultWhatsappServcieEgypt egyptService,
        IDefaultWhatsappServcieSaudi saudiService,
        IDefaultWhatsappServcieKuwait kuwaitService,
        IDefaultWhatsappServcieBahrain bahrainService,
        IDefaultWhatsappService defaultService)
        {
            defaultWhatsappServcieEgypt = egyptService;
            defaultWhatsappServcieSaudi = saudiService;
            defaultWhatsappServcieKuwait = kuwaitService;
            DefaultWhatsappServcieBahrain = bahrainService;
            DefaultWhatsappService = defaultService;
        }

        public IDefaultWhatsappService DefaultWhatsappService { get; private set; }
        public IDefaultWhatsappServcieBahrain DefaultWhatsappServcieBahrain { get; private set; }
        public IDefaultWhatsappServcieKuwait defaultWhatsappServcieKuwait { get; private set; }
        public IDefaultWhatsappServcieSaudi defaultWhatsappServcieSaudi { get; private set; }
        public IDefaultWhatsappServcieEgypt defaultWhatsappServcieEgypt { get; private set; }
    }
}
