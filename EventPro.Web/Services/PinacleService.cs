using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Threading.Tasks;

namespace EventPro.Web.Services
{
    public class PinacleService
    {
        ChromeOptions options = new ChromeOptions();
        ChromeDriver driver;

        ~PinacleService()
        {
            driver.Close();
            driver.Dispose();
        }
        public PinacleService()
        {
            options.AddArguments("headless", "disable-gpu", "renderer");
            options.AddArgument("disable-translate");
            options.AddArgument("--disable-blink-features");
            options.AddArgument("--disable-extensions");
            options.AddArgument("no-default-browser-check");
            options.AddArgument("site-per-process");
            options.AddArgument("disable-3d-apis");
            options.AddArgument("disable-background-mode");
            options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
            options.AddUserProfilePreference("credentials_enable_service", false);
            driver = new ChromeDriver(options);
        }
        public async Task<string> GetPinacleBalance()
        {
            var balance = "";
            return await Task.Run(() =>
            {
                driver.Navigate().GoToUrl("https://console.pinbot.ai/login");
                Thread.Sleep(50);
                driver.FindElement(By.Id("inputEmailAddress")).SendKeys("EventProWapp");
                Thread.Sleep(5);
                driver.FindElement(By.Id("inputPassword")).SendKeys("EventPro19@#!12");
                Thread.Sleep(5);
                driver.FindElement(By.Id("signinbtn")).Click();
                Thread.Sleep(5);
                try
                {
                    balance = driver.FindElement(By.Id("balanceamt")).Text;
                }
                catch
                {
                    balance = "An error occured, please refresh.";
                }
                return balance;
            });
        }
    }
}
