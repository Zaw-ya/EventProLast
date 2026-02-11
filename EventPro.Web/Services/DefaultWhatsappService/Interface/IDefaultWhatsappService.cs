using System.Threading.Tasks;

using EventPro.DAL.Models;
using OpenQA.Selenium.Chrome;


namespace EventPro.Web.Services.DefaultWhatsappService.Interface
{
    public interface IDefaultWhatsappService
    {
        public Task SendMessage(Events evnt, Guest guest);
        public Task SendImage(Events evnt, Guest guest);
        public void SendReminderMessage(Events evnt, Guest guest);
        public void SendCongratulationMessage(Events evnt, Guest guest, string linkId);
        public ChromeOptions ChromeOptions();
    }
}
