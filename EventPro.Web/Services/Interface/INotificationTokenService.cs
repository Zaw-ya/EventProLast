using EventPro.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventPro.Web.Services.Interface
{
    public interface INotificationTokenService
    {
        public Task SendNotifyTokensAsync();
        public Task SendWhatsMsgToGK();

        public Task<List<MessageRequestTokens>> GetEventsWithToken(int beforeDays);
        public Task<List<GKWhatsRemiderMsgModel>> GetGKsWithEvent(int beforeDays);
    }
}