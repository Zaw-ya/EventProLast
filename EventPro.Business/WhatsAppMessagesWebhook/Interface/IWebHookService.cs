namespace EventPro.Business.WhatsAppMessagesWebhook.Interface
{
    public interface IWebHookService
    {
        Task ProcessStatusAsync(dynamic obj);
        Task ProcessIncomingMessageAsync(dynamic obj);
    }
}
