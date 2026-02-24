using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.Web.Services;

using Google.Apis.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioMessageTemplates : IMessageTemplates
    {
        private readonly ILogger<TwilioCardTemplates> _logger;

        public TwilioMessageTemplates(ILogger<TwilioCardTemplates> logger)
        {
            _logger = logger;
        }

        public TwilioMessageTemplates(IConfiguration configuration
            , IMemoryCacheStoreService memoryCacheStoreService, UrlProtector urlProtector, ILogger<TwilioCardTemplates> logger)
        {
            ConfirmationMessageTemplate = new Lazy<IConfirmationMessageTemplates>(() =>
            new TwilioConfirmationTemplates(configuration, memoryCacheStoreService, urlProtector,_logger));

            CardMessageTemplate = new Lazy<ICardMessageTemplates>(() =>
            new TwilioCardTemplates(configuration, memoryCacheStoreService, _logger));

            EventLocationMessageTemplate = new Lazy<IEventLocationMessageTemplates>(() =>
            new TwilioEventLocatioinTemplates(configuration, memoryCacheStoreService, _logger));

            DuplicateMessageTemplate = new Lazy<IDuplicateMessageTemplates>(() =>
            new TwilioDuplicateMessageTemplates(configuration, memoryCacheStoreService, _logger));

            ReminderMessageTemplate = new Lazy<IReminderMessageTemplates>(() =>
            new TwilioReminderMessageTemplates(configuration, memoryCacheStoreService, _logger));

            CongratulationsMessageTemplate = new Lazy<ICongratulationsMessageTemplates>(() =>
            new TwilioCongratulationTemplates(configuration, memoryCacheStoreService, _logger));

            DeclineMessageTemplate = new Lazy<IDeclineMessageTemplates>(() =>
            new TwilioDeclineMessageTemplates(configuration, memoryCacheStoreService, _logger));

            GateKeeperMessageTemplates = new Lazy<IGateKeeperMessageTemplates>(() =>
            new TwilioGateKeeperMessageTemplates(configuration, memoryCacheStoreService, _logger));
            this._logger = logger;
        }
        public Lazy<IConfirmationMessageTemplates> ConfirmationMessageTemplate { get; private set; }
        public Lazy<ICardMessageTemplates> CardMessageTemplate { get; private set; }
        public Lazy<IEventLocationMessageTemplates> EventLocationMessageTemplate { get; private set; }
        public Lazy<IDuplicateMessageTemplates> DuplicateMessageTemplate { get; private set; }
        public Lazy<IReminderMessageTemplates> ReminderMessageTemplate { get; private set; }
        public Lazy<ICongratulationsMessageTemplates> CongratulationsMessageTemplate { get; private set; }
        public Lazy<IDeclineMessageTemplates> DeclineMessageTemplate { get; private set; }

        public Lazy<IGateKeeperMessageTemplates> GateKeeperMessageTemplates { get; private set; }
    }
}
