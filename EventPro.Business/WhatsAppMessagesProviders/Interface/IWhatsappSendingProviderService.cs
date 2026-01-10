using EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio;
using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IWhatsappSendingProviderService
    {
        Task<IMessagesSendingFactory> SelectConfiguredSendingProviderAsync(Events events);
        IMessagesSendingFactory SelectWatiSendingProvider();
        TwilioMessagesSendingFactory SelectTwilioSendingProvider();
    }
}
