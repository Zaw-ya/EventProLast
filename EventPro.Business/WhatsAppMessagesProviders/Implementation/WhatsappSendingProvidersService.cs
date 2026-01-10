using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Services;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation
{
    public class WhatsappSendingProvidersService : IWhatsappSendingProviderService
    {
        private readonly EventProContext db;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheStoreService _memoryCacheStoreService;
        private readonly UrlProtector _urlProtector;
        public WhatsappSendingProvidersService(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService, UrlProtector urlProtector)
        {
            db = new EventProContext(configuration);
            _configuration = configuration;
            _memoryCacheStoreService = memoryCacheStoreService;
            _urlProtector = urlProtector;
        }

        public IMessagesSendingFactory messageSendingFactory { get; set; }

        public async Task<IMessagesSendingFactory> SelectConfiguredSendingProviderAsync(Events events)
        {
            var DefaultSendingProvider = await db.AppSettings
                .Select(e => e.WhatsappServiceProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (events.WhatsappProviderName == "Default")
            {
                if (DefaultSendingProvider == "Wati")
                {
                    return null;
                }
                else if (DefaultSendingProvider == "Twilio")
                {
                    return messageSendingFactory = new TwilioMessagesSendingFactory(_configuration, _memoryCacheStoreService, _urlProtector);
                }
            }
            else if (events.WhatsappProviderName == "Wati")
            {
                return null;
            }
            else if (events.WhatsappProviderName == "Twilio")
            {
                return messageSendingFactory = new TwilioMessagesSendingFactory(_configuration, _memoryCacheStoreService, _urlProtector);
            }

            return messageSendingFactory = new TwilioMessagesSendingFactory(_configuration, _memoryCacheStoreService, _urlProtector);
        }

        public TwilioMessagesSendingFactory SelectTwilioSendingProvider()
        {
            return new TwilioMessagesSendingFactory(_configuration, _memoryCacheStoreService, _urlProtector);
        }

        public IMessagesSendingFactory SelectWatiSendingProvider()
        {
            return null;
        }
    }
}
