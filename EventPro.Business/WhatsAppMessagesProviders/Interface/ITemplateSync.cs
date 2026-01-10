using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface ITemplateSync
    {
        Task<string> GetCustomTemplateWithVariablesAsync(Events events,int typeId);
    }        
}
