using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.Web.Services;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioMessageTemplates : IMessageTemplates
    {
        public TwilioMessageTemplates(IConfiguration configuration
            , IMemoryCacheStoreService memoryCacheStoreService,UrlProtector urlProtector)
        {
            ConfirmationMessageTemplate = new Lazy<IConfirmationMessageTemplates>(() =>
            new TwilioConfirmationTemplates(configuration, memoryCacheStoreService,urlProtector));

            CardMessageTemplate = new Lazy<ICardMessageTemplates>(() =>
            new TwilioCardTemplates(configuration, memoryCacheStoreService));

            EventLocationMessageTemplate = new Lazy<IEventLocationMessageTemplates>(() =>
            new TwilioEventLocatioinTemplates(configuration, memoryCacheStoreService));

            DuplicateMessageTemplate = new Lazy<IDuplicateMessageTemplates>(() =>
            new TwilioDuplicateMessageTemplates(configuration, memoryCacheStoreService));

            ReminderMessageTemplate = new Lazy<IReminderMessageTemplates>(() =>
            new TwilioReminderMessageTemplates(configuration, memoryCacheStoreService));

            CongratulationsMessageTemplate = new Lazy<ICongratulationsMessageTemplates>(() =>
            new TwilioCongratulationTemplates(configuration, memoryCacheStoreService));

            DeclineMessageTemplate = new Lazy<IDeclineMessageTemplates>(() =>
            new TwilioDeclineMessageTemplates(configuration, memoryCacheStoreService));

            GateKeeperMessageTemplates = new Lazy<IGateKeeperMessageTemplates>(() =>
            new TwilioGateKeeperMessageTemplates(configuration,memoryCacheStoreService));
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
