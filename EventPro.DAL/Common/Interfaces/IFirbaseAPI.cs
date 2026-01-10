using EventPro.DAL.Models;
using System.Threading.Tasks;

namespace EventPro.DAL.Common.Interfaces
{
    public interface IFirbaseAPI
    {
        public Task<bool> NotifyTopicOrTokenAsync(MessageRequest request);
        public Task<bool> NotifyTokensAsync(MessageRequestTokens request);
    }
}