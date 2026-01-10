using EventPro.DAL.Models;
using System.Threading.Tasks;

namespace EventPro.API.Services.WatiService.Interface
{
    public interface IWatiService
    {
        public Task<string> SendGatekeeperCheckoutMessage(GKEventHistory gKEventHistory);
        public Task<string> SendGatekeeperCheckInMessage(GKEventHistory history, string filePathForSending);
    }
}
