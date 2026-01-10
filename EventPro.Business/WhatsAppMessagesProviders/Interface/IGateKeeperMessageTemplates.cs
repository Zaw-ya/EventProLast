using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IGateKeeperMessageTemplates
    {
        Task SendCheckInMessage( GKEventHistory gkEventHistory);
        Task SendCheckOutMessage( GKEventHistory gkEventHistory);
        Task SendGateKeeperUnassignEventMessage( GKEventHistory gkEventHistory);
    }
}
