namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IMessageTemplates
    {
        public Lazy<IConfirmationMessageTemplates> ConfirmationMessageTemplate { get; }
        public Lazy<ICardMessageTemplates> CardMessageTemplate { get; }
        public Lazy<IEventLocationMessageTemplates> EventLocationMessageTemplate { get; }
        public Lazy<IDuplicateMessageTemplates> DuplicateMessageTemplate { get; }
        public Lazy<IReminderMessageTemplates> ReminderMessageTemplate { get; }
        public Lazy<ICongratulationsMessageTemplates> CongratulationsMessageTemplate { get; }
        public Lazy<IDeclineMessageTemplates> DeclineMessageTemplate { get; }
        public Lazy<IGateKeeperMessageTemplates> GateKeeperMessageTemplates { get; }

    }
}
