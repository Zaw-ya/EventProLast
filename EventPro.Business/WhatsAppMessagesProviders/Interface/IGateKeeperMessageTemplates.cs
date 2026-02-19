using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IGateKeeperMessageTemplates
    {
        Task SendCheckInMessage( GKEventHistory gkEventHistory);
        Task SendCheckOutMessage( GKEventHistory gkEventHistory);
        Task SendGateKeeperassignEventMessage(GKEventHistory gkEventHistory);
        Task SendGateKeeperUnassignEventMessage( GKEventHistory gkEventHistory);
        Task SendGateKeeperTodayReminderWhatsAppAsync (GKWhatsRemiderMsgModel gkWhatsRemider);
        Task SendGateKeeperReminderWhatsAppAsync (GKWhatsRemiderMsgModel gkWhatsRemider);
        Task AssignGateKeeperEventMessage(GKWhatsRemiderMsgModel gkWhatsRemider);
        Task UnassignGateKeeperEventMessage(GKWhatsRemiderMsgModel gkWhatsRemider);
    }
}
