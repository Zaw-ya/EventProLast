using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using EventPro.Web.Services.DefaultWhatsappService.Interface;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace EventPro.Web.Services.DefaultWhatsappService.Implementation
{
    public class DefaultWhatsappService : IDefaultWhatsappService
    {
        protected readonly IConfiguration _configuration;
        private string sendingTextBox;
        private string sendingOptions;
        private string sendImage;
        private string sendImageButton;
        private string sendImageTextButton;
        private string sendVideoTextButton;
        private string sendTextButton;
        private string addNewChat;
        private string searchNewChat;
        private string newChatContact;
        private string attentionMessageArabic;
        private string attentionMessageEnglish;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EventProContext _db;
        public DefaultWhatsappService(IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _db = new EventProContext(configuration);
            _httpContextAccessor = httpContextAccessor;
        }


        private void UpdateDefaultWhatsAppSettings()
        {
            var defaultWhatsappsettings = _db.DefaultWhatsappSettings
                                          .AsNoTracking()
                                          .FirstOrDefault();

            sendingTextBox = defaultWhatsappsettings.MessageTextBox;
            sendingOptions = defaultWhatsappsettings.MediaOptions;
            sendImage = defaultWhatsappsettings.ImageOption;
            sendImageButton = defaultWhatsappsettings.SendImageButton;
            sendImageTextButton = defaultWhatsappsettings.ImageTextBox;
            sendVideoTextButton = defaultWhatsappsettings.VideoTextBox;
            sendTextButton = defaultWhatsappsettings.SendMessageButton;
            addNewChat = defaultWhatsappsettings.AddNewChatButton;
            searchNewChat = defaultWhatsappsettings.SearchNewChatButton;
            newChatContact = defaultWhatsappsettings.NewContactButton;
            attentionMessageArabic = _configuration.GetSection("SendFailed").GetSection("attentionMessageArabic").Value;
            attentionMessageEnglish = _configuration.GetSection("SendFailed").GetSection("attentionMessageEnglish").Value;
        }
        public virtual ChromeOptions ChromeOptions()
        {
            ChromeOptions options = new ChromeOptions();
            options.DebuggerAddress = "localhost:9222";
            options.AddArguments("--remote-debugging-port=9222");
            options.AddArguments("--headless");
            options.AddArguments("--headless=new");
            options.AddArguments("--disable-gpu");
            options.AddArgument("detach");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-renderer-backgrounding");
            options.AddArgument("--disable-background-timer-throttling");
            options.AddArgument("--disable-backgrounding-occluded-windows");
            options.AddArgument("--disable-client-side-phishing-detection");
            options.AddArgument("--disable-crash-reporter");
            options.AddArgument("--disable-oopr-debug-crash-dump");
            options.AddArgument("--no-crash-upload");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-low-res-tiling");
            options.AddArgument("--log-level=3");
            options.AddArgument("--silent");

            return options;
        }

        public void SendCongratulationMessage(Events evnt, Guest guest, string linkId)
        {
            UpdateDefaultWhatsAppSettings();
            ChromeOptions options = ChromeOptions();
            IWebDriver driver = new ChromeDriver(options);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            try
            {
                lock (driver)
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    string msg = string.Empty;
                    string mobile = guest.SecondaryContactNo + guest.PrimaryContactNo;

                    if (evnt.ShowFailedSendingCongratulationLink == true)
                    {
                        msg = guest.FirstName + "\n\r\n\r" + evnt.FailedGuestsCongratulationMsg
                              + "\n\r https://www.EventPro.cc/feedback?id=" + linkId;
                    }
                    else
                    {
                        msg = guest.FirstName + "\n\r\n\r" + evnt.FailedGuestsCongratulationMsg;
                    }


                    try
                    {
                        throw new Exception(); // to use reloading page
                        IWebElement addNewChatButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(addNewChat)));
                        addNewChatButton.Click();
                    }
                    catch
                    {
                        driver.Navigate().GoToUrl("https://web.whatsapp.com/send?phone=" + mobile + "&text=" + Uri.EscapeDataString(msg));
                        IWebElement sendingButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendTextButton)));
                        sendingButton.Click();
                        return;
                    }

                    IWebElement searchNewChatButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(searchNewChat)));
                    searchNewChatButton.SendKeys(mobile.Trim());
                    Thread.Sleep(1500);
                    try
                    {
                        IWebElement newChatContactButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(newChatContact)));
                        newChatContactButton.Click();
                    }
                    catch
                    {
                        //searchNewChatButton.SendKeys(Keys.Backspace);
                        //Thread.Sleep(1500);
                        //IWebElement newChatContactButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(newChatContact)));
                        //newChatContactButton.Click();
                    }
                    Thread.Sleep(500);
                    IWebElement textButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingTextBox)));
                    //textButton.SendKeys(Keys.LeftControl + "A");
                    //textButton.SendKeys(Keys.Backspace);

                    textButton.SendKeys(msg);
                    IWebElement sendButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendTextButton)));
                    sendButton.Click();

                }
            }
            catch (Exception ex)
            {
                driver.Navigate().GoToUrl("https://web.whatsapp.com/send");
                throw ex;
            }
            finally
            {
                driver.Dispose();
                driver.Quit();
            }
        }

        public void SendImage(Events evnt, Guest guest)
        {
            UpdateDefaultWhatsAppSettings();
            ChromeOptions options = ChromeOptions();
            IWebDriver driver = new ChromeDriver(options);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            string img = string.Empty;

          //  Image image = Image.FromFile(img);
            try
            {
                lock (driver)
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                    var request = _httpContextAccessor.HttpContext.Request;
                    var baseUrl = $"{request.Scheme}://{request.Host}";
                    string imgUrl = $"{baseUrl}/upload/preview/" + evnt.Id + "/E00000" + evnt.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                    img = Path.Combine(Path.GetTempPath(), "tempImage.jpg");

                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(imgUrl, img);
                    }
                    string msg = evnt.FailedGuestsCardText;
                    string mobile = guest.SecondaryContactNo + guest.PrimaryContactNo;
                    try
                    {
                        throw new Exception(); // to use reloading page
                        IWebElement addNewChatButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(addNewChat)));
                        addNewChatButton.Click();
                    }
                    catch
                    {
                        driver.Navigate().GoToUrl("https://web.whatsapp.com/send?phone=" + mobile + "&text=" + Uri.EscapeDataString(msg));
                        //IWebElement sendingOptionsButton2 = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingOptions)));
                        //sendingOptionsButton2.Click();
                        //var x =wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(text(), 'Photos')]/ancestor::li")));
                        //System.Threading.Thread.Sleep(1000);
                        //IWebElement fileInput = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(sendImage)));
                        //fileInput.SendKeys(img);
                        //IWebElement button2 = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(sendImageButton)));
                        //button2.Click();


                        //===========================
                        //IWebElement chatBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"main\"]/footer/div[1]/div/span/div/div[2]/div/div[3]/div/p")));

                        //// 2. COPY IMAGE TO CLIPBOARD (Must be done in an STA Thread)
                        //// 'img' variable is assumed to be your string file path (e.g., "C:\photo.jpg")
                        //Thread staThread = new Thread(() =>
                        //{
                        //    try
                        //    {
                        //        // Load the image from the path string
                        //        using (System.Drawing.Image imageObject = System.Drawing.Image.FromFile(img))
                        //        {
                        //            Clipboard.SetImage(imageObject);
                        //        }
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        // Log error if file not found or invalid
                        //        Console.WriteLine($"Clipboard Error: {ex.Message}");
                        //    }
                        //});

                        //// Set thread to STA (Single Thread Apartment) - Required for OLE/Clipboard calls
                        //staThread.SetApartmentState(ApartmentState.STA);
                        //staThread.Start();
                        //staThread.Join(); // Wait for the thread to finish copying

                        //// 3. PASTE INTO WHATSAPP
                        //chatBox.Click();
                        //// Use OpenQA.Selenium.Keys, NOT System.Windows.Forms.Keys
                        //chatBox.SendKeys(OpenQA.Selenium.Keys.Control + "v");

                        //// 4. WAIT FOR PREVIEW & CLICK SEND
                        //// WhatsApp shows a green circle button with a paper plane icon
                        //IWebElement sendButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"app\"]/div/div/div[3]/div/div[3]/div[2]/div/span/div/div/div/div[2]/div/div[2]/div[2]/div")));
                        //sendButton.Click();


                        //////////////////////////////////////////////////////////
                        ///
                        // Navigate and open chat
                        // 1. Navigate
                       

                        // 2. Wait for the Main Chat View (The Drop Zone)
                        // We target the main container ID 'main', which is stable and always accepts drops.
                        IWebElement chatBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingTextBox)));

                        // 3. Prepare the file in C# (Read into memory)
                        // This works in Headless because we read the file here, not the browser.
                        string filename = System.IO.Path.GetFileName(img);
                        string base64Image = Convert.ToBase64String(System.IO.File.ReadAllBytes(img));
                        string mimeType = "image/jpeg"; // Change to "video/mp4" if sending video

                        // 4. JAVASCRIPT: Synthesize a Paste Event
                        // We pass the file data directly into a JavaScript event.
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        string pasteScript = @"
    var target = arguments[0];
    var base64Data = arguments[1];
    var mimeType = arguments[2];
    var fileName = arguments[3];

    try {
        // A. Convert Base64 string back to a Binary Blob
        var byteCharacters = atob(base64Data);
        var byteArrays = [];
        var sliceSize = 512;
        
        for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
            var slice = byteCharacters.slice(offset, offset + sliceSize);
            var byteNumbers = new Array(slice.length);
            for (var i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }
            var byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }
        var blob = new Blob(byteArrays, {type: mimeType});

        // B. Create a File Object
        var file = new File([blob], fileName, {type: mimeType});

        // C. Create a DataTransfer (This mimics the clipboard payload)
        var dataTransfer = new DataTransfer();
        dataTransfer.items.add(file);

        // D. Create the Paste Event
        var pasteEvent = new ClipboardEvent('paste', {
            bubbles: true,
            cancelable: true,
            clipboardData: dataTransfer
        });

        // E. Dispatch
        target.focus();
        target.dispatchEvent(pasteEvent);
        return 'success';
    } catch (err) {
        return err.toString();
    }
";

                        // Execute the script
                        // We pass the chatBox element and the file data as arguments
                        object result = js.ExecuteScript(pasteScript, chatBox, base64Image, mimeType, filename);

                        if (result.ToString() != "success")
                        {
                            throw new Exception("JS Paste Failed: " + result.ToString());
                        }

                        // 5. Wait for the 'Send' button (Paper Plane) to appear
                        // Using a specific attribute is safer than a long absolute XPath
                        IWebElement sendButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendImageButton)));
                        sendButton.Click();

                        return;
                    }

                    IWebElement searchNewChatButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(searchNewChat)));
                    searchNewChatButton.SendKeys(mobile.Trim());
                    Thread.Sleep(1500);
                    try
                    {
                        IWebElement newChatContactButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(newChatContact)));
                        newChatContactButton.Click();
                    }
                    catch
                    {
                      //  searchNewChatButton.SendKeys(Keys.Backspace);
                        Thread.Sleep(1500);
                        IWebElement newChatContactButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(newChatContact)));
                        newChatContactButton.Click();
                    }
                    Thread.Sleep(500);
                    IWebElement textButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingTextBox)));
                    //textButton.SendKeys(Keys.LeftControl + "A");
                    //textButton.SendKeys(Keys.Backspace);

                    IWebElement sendingOptionsButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingOptions)));
                    sendingOptionsButton.Click();
                    driver.FindElement(By.XPath(sendImage)).SendKeys(img);
                    IWebElement sendImageTextButton1 = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendImageTextButton)));
                    sendImageTextButton1.SendKeys(msg);
                    IWebElement button = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendImageButton)));
                    button.Click();
                }
            }
            catch (Exception ex)
            {
                driver.Navigate().GoToUrl("https://web.whatsapp.com/send");
                throw ex;
            }
            finally
            {
           //     image.Dispose();
                driver.Dispose();
                driver.Quit();
                if (File.Exists(img))
                {
                    File.Delete(img);
                }

                Thread.Sleep(7000);
            }
        }

        public void SendMessage(Events evnt, Guest guest)
        {
            UpdateDefaultWhatsAppSettings();
            ChromeOptions options = ChromeOptions();
            IWebDriver driver = new ChromeDriver(options);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            string img = string.Empty;
            try
            {
                lock (driver)
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    string msg = string.Empty;
                    var attentionMsg = string.Empty;
                    if (evnt.FailedSendingConfiramtionMessagesLinksLanguage == "Arabic")
                    {
                        attentionMsg = attentionMessageArabic;
                        if (evnt.ShowFailedSendingEventLocationLink == true)
                        {
                            msg = guest.FirstName + "\n\r" + evnt.FailedGuestsMessag;
                            //        +
                            //"\n\r" + "\r\n?????? ?????? ?? ???????? ?? ?????? , ???? ??? ?????? ?????? :\r\n https://www.EventPro.cc/DefaultWhatsapp/AcceptOrDecline?id=" + guest.GuestId + guest.EventId +
                            //"\r\n????? ???????? , ???? ??? ?????? ?????? :\r\n https://www.EventPro.cc/DefaultWhatsapp/EventLocation?id=" + guest.GuestId + guest.EventId;
                        }
                        else
                        {
                            msg = guest.FirstName + "\n\r" + evnt.FailedGuestsMessag;
                            //        +
                            //"\n\r" + "\r\n?????? ?????? ?? ???????? ?? ?????? , ???? ??? ?????? ?????? :\r\n https://www.EventPro.cc/DefaultWhatsapp/AcceptOrDecline?id=" + guest.GuestId + guest.EventId;

                        }
                    }
                    else if (evnt.FailedSendingConfiramtionMessagesLinksLanguage == "English")
                    {
                        attentionMsg = attentionMessageEnglish;
                        if (evnt.ShowFailedSendingEventLocationLink == true)
                        {
                            msg = guest.FirstName + "\n\r" + evnt.FailedGuestsMessag;
                            //    +
                            //"\n\r" + "\r\n To Confirm or decline , Press the link below : \r\n https://www.EventPro.cc/DefaultWhatsapp/AcceptOrDecline?id=" + guest.GuestId + guest.EventId +
                            //"\r\n For Event Location , Press the link below :\r\n https://www.EventPro.cc/DefaultWhatsapp/EventLocation?id=" + guest.GuestId + guest.EventId;
                        }
                        else
                        {
                            msg = guest.FirstName + "\n\r" + evnt.FailedGuestsMessag;
                            //        +
                            //"\n\r" + "\r\n To Confirm or decline , Press the link below : \r\n https://www.EventPro.cc/DefaultWhatsapp/AcceptOrDecline?id=" + guest.GuestId + guest.EventId;

                        }
                    }
                    else
                    {
                        throw new Exception();
                    }

                    string mobile = guest.SecondaryContactNo + guest.PrimaryContactNo;
                    try
                    {
                        throw new Exception(); // to use reloading page
                        IWebElement addNewChatButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(addNewChat)));
                        addNewChatButton.Click();
                    }
                    catch
                    {
                        driver.Navigate().GoToUrl("https://web.whatsapp.com/send?phone=" + mobile + "&text=" + Uri.EscapeDataString(msg));
                        if (string.IsNullOrEmpty(evnt.MessageHeaderImage))
                        {
                            IWebElement sendingButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendTextButton)));
                            sendingButton.Click();

                        }
                        else
                        {
                            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                            var request = _httpContextAccessor.HttpContext.Request;
                            var baseUrl = $"{request.Scheme}://{request.Host}";
                            string imgUrl = $"{baseUrl}/upload/preview/" + evnt.MessageHeaderImage;
                            img = Path.Combine(Path.GetTempPath(), "tempImage.jpg");

                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFile(imgUrl, img);
                            }
                            IWebElement sendingOptionsButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingOptions)));
                            sendingOptionsButton.Click();
                            driver.FindElement(By.XPath(sendImage)).SendKeys(img);
                            System.Threading.Thread.Sleep(2000);
                            IWebElement button = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendImageButton)));
                            button.Click();
                        }

                        //IWebElement textArea2 = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingTextBox)));
                        //textArea2.SendKeys(attentionMsg);
                        //IWebElement sendButton2 = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendTextButton)));
                        //sendButton2.Click();

                        return;
                    }

                    IWebElement searchNewChatButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(searchNewChat)));
                    searchNewChatButton.SendKeys(mobile.Trim());
                    Thread.Sleep(1500);
                    try
                    {
                        IWebElement newChatContactButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(newChatContact)));
                        newChatContactButton.Click();
                    }
                    catch
                    {
                       // searchNewChatButton.SendKeys(Keys.Backspace);
                        Thread.Sleep(1500);
                        IWebElement newChatContactButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(newChatContact)));
                        newChatContactButton.Click();
                    }
                    Thread.Sleep(500);
                    IWebElement textButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingTextBox)));
                    //textButton.SendKeys(Keys.LeftControl + "A");
                    //textButton.SendKeys(Keys.Backspace);
                    if (string.IsNullOrEmpty(evnt.MessageHeaderImage))
                    {
                        textButton.SendKeys(msg);
                        IWebElement sendButton2 = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendTextButton)));
                        sendButton2.Click();
                    }
                    else
                    {
                        img = "H:\\Upload\\Prod\\preview\\" + evnt.MessageHeaderImage;
                        IWebElement sendingOptionsButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingOptions)));
                        sendingOptionsButton.Click();
                        driver.FindElement(By.XPath(sendImage)).SendKeys(img);
                        if (evnt.MessageHeaderImage.EndsWith(".mp4"))
                        {
                            var sendVideoTextButton1 = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendVideoTextButton)));
                            sendVideoTextButton1.SendKeys(msg);
                        }
                        else
                        {
                            IWebElement sendImageTextButton1 = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendImageTextButton)));
                            sendImageTextButton1.SendKeys(msg);
                        }

                        IWebElement button = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendImageButton)));
                        button.Click();
                    }

                    IWebElement textArea = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingTextBox)));
                    textArea.SendKeys(attentionMsg);
                    IWebElement sendButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendTextButton)));
                    sendButton.Click();
                }
            }
            catch (Exception ex)
            {
                driver.Navigate().GoToUrl("https://web.whatsapp.com/send");
                throw ex;
            }
            finally
            {
                driver.Dispose();
                driver.Quit();

                if (File.Exists(img))
                {
                    File.Delete(img);
                }
            }
        }

        public void SendReminderMessage(Events evnt, Guest guest)
        {
            UpdateDefaultWhatsAppSettings();
            ChromeOptions options = ChromeOptions();
            IWebDriver driver = new ChromeDriver(options);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            try
            {
                lock (driver)
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    string msg = guest.FirstName + "\n\r\n\r" + evnt.FailedGuestsReminderMessage;
                    string mobile = guest.SecondaryContactNo + guest.PrimaryContactNo;

                    try
                    {
                        throw new Exception(); // to use reloading page
                        IWebElement addNewChatButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(addNewChat)));
                        addNewChatButton.Click();
                    }
                    catch
                    {
                        driver.Navigate().GoToUrl("https://web.whatsapp.com/send?phone=" + mobile + "&text=" + Uri.EscapeDataString(msg));
                        IWebElement sendingButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendTextButton)));
                        sendingButton.Click();
                        return;
                    }

                    IWebElement searchNewChatButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(searchNewChat)));
                    searchNewChatButton.SendKeys(mobile.Trim());
                    Thread.Sleep(1500);
                    try
                    {
                        IWebElement newChatContactButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(newChatContact)));
                        newChatContactButton.Click();
                    }
                    catch
                    {
                       // searchNewChatButton.SendKeys(Keys.Backspace);
                        Thread.Sleep(1500);
                        IWebElement newChatContactButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(newChatContact)));
                        newChatContactButton.Click();
                    }
                    Thread.Sleep(500);
                    IWebElement textButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingTextBox)));
                    //textButton.SendKeys(Keys.LeftControl + "A");
                    //textButton.SendKeys(Keys.Backspace);

                    textButton.SendKeys(msg);
                    IWebElement sendButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendTextButton)));
                    sendButton.Click();
                }
            }
            catch (Exception ex)
            {
                driver.Navigate().GoToUrl("https://web.whatsapp.com/send");
                throw ex;
            }
            finally
            {
                driver.Quit();
            }
        }
    }
}
