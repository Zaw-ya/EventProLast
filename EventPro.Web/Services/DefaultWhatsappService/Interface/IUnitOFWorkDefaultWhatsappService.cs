using EventPro.Web.Services.DefaultWhatsapp.Interface;

namespace EventPro.Web.Services.DefaultWhatsappService.Interface
{
    public interface IUnitOFWorkDefaultWhatsappService
    {
        IDefaultWhatsappService DefaultWhatsappService { get; }
        IDefaultWhatsappServcieBahrain DefaultWhatsappServcieBahrain { get; }
        IDefaultWhatsappServcieKuwait defaultWhatsappServcieKuwait { get; }
        IDefaultWhatsappServcieSaudi defaultWhatsappServcieSaudi { get; }
        IDefaultWhatsappServcieEgypt defaultWhatsappServcieEgypt { get; }
    }
}
