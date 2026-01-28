using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Services;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation
{
    public class WhatsappSendingProvidersService : IWhatsappSendingProviderService
    {
        private readonly EventProContext db;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheStoreService _memoryCacheStoreService;
        private readonly UrlProtector _urlProtector;
        private readonly ILogger<TwilioCardTemplates> _logger;
        public WhatsappSendingProvidersService(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService, UrlProtector urlProtector, ILogger<TwilioCardTemplates> logger)
        {
            db = new EventProContext(configuration);
            _configuration = configuration;
            _memoryCacheStoreService = memoryCacheStoreService;
            _urlProtector = urlProtector;
            _logger = logger;
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
                    return messageSendingFactory = new TwilioMessagesSendingFactory(_configuration, _memoryCacheStoreService, _urlProtector, _logger);
                }
            }
            else if (events.WhatsappProviderName == "Wati")
            {
                return null;
            }
            else if (events.WhatsappProviderName == "Twilio")
            {
                return messageSendingFactory = new TwilioMessagesSendingFactory(_configuration, _memoryCacheStoreService, _urlProtector, _logger);
            }

            return messageSendingFactory = new TwilioMessagesSendingFactory(_configuration, _memoryCacheStoreService, _urlProtector, _logger);
        }

        public TwilioMessagesSendingFactory SelectTwilioSendingProvider()
        {
            return new TwilioMessagesSendingFactory(_configuration, _memoryCacheStoreService, _urlProtector, _logger);
        }

        public IMessagesSendingFactory SelectWatiSendingProvider()
        {
            return null;
        }
    }
}
