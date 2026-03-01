using System;

﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using EventPro.Web.Services.DefaultWhatsappService.Interface;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Forms;

using EventPro.Business.Storage.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Services.DefaultWhatsappService.Interface;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

using Serilog;

using System.Threading.Tasks;
using System.Net.Http;
using OpenQA.Selenium.Interactions;
//using System.Windows.Forms;


namespace EventPro.Web.Services.DefaultWhatsappService.Implementation
{
    #region Class Summary
    /// <summary>
    /// DefaultWhatsappService - Legacy WhatsApp Web Automation Service
    ///
    /// PURPOSE:
    /// This service uses Selenium WebDriver to automate WhatsApp Web for sending messages
    /// when Twilio API fails or is unavailable. It's a FALLBACK mechanism.
    ///
    /// IMPORTANT NOTES:
    /// - This is LEGACY code that uses browser automation (Selenium + Chrome)
    /// - Requires Chrome browser running with remote debugging enabled
    /// - Less reliable than Twilio API - use only as fallback
    /// - WhatsApp Web UI changes can break XPath selectors
    ///
    /// PRIORITY ORDER OF METHODS:
    /// 1. ChromeOptions()              - [INFRASTRUCTURE] Required for all other methods
    /// 2. UpdateDefaultWhatsAppSettings() - [INFRASTRUCTURE] Loads XPath selectors from DB
    /// 3. SendMessage()                - [HIGH] Primary confirmation message sending
    /// 4. SendImage()                  - [HIGH] Card/invitation image sending
    /// 5. SendReminderMessage()        - [MEDIUM] Reminder message sending
    /// 6. SendCongratulationMessage()  - [LOW] Thank you/congratulation message sending
    ///
    /// WHEN TO USE:
    /// - When Twilio API fails for specific guests
    /// - When WhatsApp Business API is not available
    /// - For manual retry of failed messages via admin panel
    ///
    /// DEPENDENCIES:
    /// - Chrome browser with --remote-debugging-port=9222
    /// - Selenium.WebDriver NuGet package
    /// - DefaultWhatsappSettings table in database (stores XPath selectors)
    /// </summary>
    #endregion
    public class DefaultWhatsappService : IDefaultWhatsappService
    {
        #region Private Fields

        /// <summary>
        /// Application configuration for accessing appsettings.json values.
        /// </summary>
        protected readonly IConfiguration _configuration;

        /// <summary>
        /// XPath selector for the message text input box in WhatsApp Web.
        /// </summary>
        private string sendingTextBox;

        /// <summary>
        /// XPath selector for the attachment/media options button (+).
        /// </summary>
        private string sendingOptions;

        /// <summary>
        /// XPath selector for the image upload input element.
        /// </summary>
        private string sendImage;

        /// <summary>
        /// XPath selector for the send button after selecting an image.
        /// </summary>
        private string sendImageButton;

        /// <summary>
        /// XPath selector for the caption text box when sending an image.
        /// </summary>
        private string sendImageTextButton;

        /// <summary>
        /// XPath selector for the caption text box when sending a video.
        /// </summary>
        private string sendVideoTextButton;

        /// <summary>
        /// XPath selector for the send message button (paper plane icon).
        /// </summary>
        private string sendTextButton;

        /// <summary>
        /// XPath selector for the "New Chat" button.
        /// </summary>
        private string addNewChat;

        /// <summary>
        /// XPath selector for the search input in new chat dialog.
        /// </summary>
        private string searchNewChat;

        /// <summary>
        /// XPath selector for selecting a contact from search results.
        /// </summary>
        private string newChatContact;

        /// <summary>
        /// Arabic attention message text from configuration.
        /// </summary>
        private string attentionMessageArabic;

        /// <summary>
        /// English attention message text from configuration.
        /// </summary>
        private string attentionMessageEnglish;

        /// <summary>
        /// HTTP context accessor for getting current request information (base URL).
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Database context for accessing DefaultWhatsappSettings.
        /// </summary>
        private readonly EventProContext _db;
        private readonly IBlobStorage _blobStorage;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the DefaultWhatsappService with required dependencies.
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <param name="httpContextAccessor">HTTP context for URL generation</param>
        public DefaultWhatsappService(IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IBlobStorage blobStorage)
        {
            _configuration = configuration;
            _db = new EventProContext(configuration);
            _httpContextAccessor = httpContextAccessor;
            _blobStorage = blobStorage;
        }

        #endregion

        #region Infrastructure Methods (Priority: CRITICAL)

        /// <summary>
        /// [PRIORITY: CRITICAL - Required by all sending methods]
        ///
        /// Loads WhatsApp Web XPath selectors from the database.
        /// These selectors are used to find UI elements in WhatsApp Web.
        ///
        /// WHY DATABASE STORAGE:
        /// - WhatsApp Web UI changes frequently
        /// - XPath selectors need to be updated without code deployment
        /// - Admin can update selectors via database when WhatsApp changes
        ///
        /// SELECTORS LOADED:
        /// - MessageTextBox: Main chat input field
        /// - MediaOptions: Attachment button (+)
        /// - ImageOption: Photo/Image upload input
        /// - SendImageButton: Send button for media
        /// - ImageTextBox: Caption field for images
        /// - VideoTextBox: Caption field for videos
        /// - SendMessageButton: Send button for text
        /// - AddNewChatButton: New chat button
        /// - SearchNewChatButton: Search input
        /// - NewContactButton: Contact selection
        /// </summary>
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

        /// <summary>
        /// [PRIORITY: CRITICAL - Required by all sending methods]
        ///
        /// Configures Chrome WebDriver options for WhatsApp Web automation.
        ///
        /// REQUIREMENTS:
        /// - Chrome must be running with: chrome.exe --remote-debugging-port=9222
        /// - User must be logged into WhatsApp Web in that Chrome session
        ///
        /// OPTIONS EXPLAINED:
        /// - DebuggerAddress: Connect to existing Chrome instance
        /// - --headless: Run without visible browser window
        /// - --disable-gpu: Prevent GPU-related crashes
        /// - --no-sandbox: Required for some server environments
        /// - --disable-dev-shm-usage: Prevent shared memory issues
        /// - --log-level=3: Suppress verbose logging
        ///
        /// USAGE:
        /// ChromeOptions options = ChromeOptions();
        /// IWebDriver driver = new ChromeDriver(options);
        /// </summary>
        /// <returns>Configured ChromeOptions for Selenium WebDriver</returns>
        public virtual ChromeOptions ChromeOptions()
        {
            ChromeOptions options = new ChromeOptions();
            options.DebuggerAddress = "localhost:9333";
            options.AddArguments("--remote-debugging-port=9333");
            //options.AddArguments("--headless");
            //options.AddArguments("--headless=new");
            //options.AddArguments("--disable-gpu");
            //options.AddArgument("detach");
            //options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-renderer-backgrounding");
            options.AddArgument("--disable-background-timer-throttling");
            options.AddArgument("--disable-backgrounding-occluded-windows");
            options.AddArgument("--disable-client-side-phishing-detection");
            options.AddArgument("--disable-crash-reporter");
            //options.AddArgument("--disable-oopr-debug-crash-dump");
            //options.AddArgument("--no-crash-upload");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-low-res-tiling");
            //options.AddArgument("--log-level=3");
            //options.AddArgument("--silent");

            return options;
        }

        #endregion

        #region Primary Messaging Methods (Priority: HIGH)

        /// <summary>
        /// [PRIORITY: HIGH - Primary confirmation message]
        ///
        /// Sends a confirmation/invitation message to a guest via WhatsApp Web.
        /// This is used when Twilio API fails to send the initial invitation.
        ///
        /// MESSAGE FLOW:
        /// 1. Load XPath selectors from database
        /// 2. Initialize Chrome WebDriver
        /// 3. Navigate to WhatsApp Web with pre-filled message
        /// 4. Optionally attach header image if configured
        /// 5. Click send button
        ///
        /// MESSAGE CONTENT:
        /// - Guest name + Event's FailedGuestsMessage
        /// - Language determined by FailedSendingConfiramtionMessagesLinksLanguage
        /// - Optional: Header image from event settings
        ///
        /// ERROR HANDLING:
        /// - On failure: Navigates to blank WhatsApp send page
        /// - Throws exception to caller for logging
        /// - Cleans up temporary image files
        ///
        /// CALLED FROM:
        /// - DefaultWhatsappController.sendMessage()
        /// - Admin panel "Send Failed Message" button
        /// </summary>
        /// <param name="evnt">Event containing message templates and settings</param>
        /// <param name="guest">Guest to send message to</param>


        public async Task SendMessage(Events evnt, Guest guest)
        {
            UpdateDefaultWhatsAppSettings();
            var options = ChromeOptions();

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            using IWebDriver driver = new ChromeDriver(service, options, TimeSpan.FromMinutes(3));

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
            string tempFilePath = null;

            try
            {
                string msg = string.Empty;
                string attentionMsg = string.Empty;


                if (evnt.FailedSendingConfiramtionMessagesLinksLanguage == "Arabic")
                {
                    attentionMsg = attentionMessageArabic;
                    if (evnt.ShowFailedSendingEventLocationLink == true)
                    {
                        msg = guest.FirstName + "\n\r" + evnt.FailedGuestsMessag;
                    }
                    else
                    {
                        msg = guest.FirstName + "\n\r" + evnt.FailedGuestsMessag;
                    }
                }
                else if (evnt.FailedSendingConfiramtionMessagesLinksLanguage == "English")
                {
                    attentionMsg = attentionMessageEnglish;
                    if (evnt.ShowFailedSendingEventLocationLink == true)
                    {
                        msg = guest.FirstName + "\n\r" + evnt.FailedGuestsMessag;
                    }
                    else
                    {
                        msg = guest.FirstName + "\n\r" + evnt.FailedGuestsMessag;                        
                    }
                }
                else
                {
                    throw new Exception("Unsupported language");
                }

                string mobile = (guest.SecondaryContactNo + guest.PrimaryContactNo).TrimStart('+', '0');

                driver.Navigate().GoToUrl($"https://web.whatsapp.com/send?phone={mobile}&text={Uri.EscapeDataString(msg)}");

                var chatBox = wait.Until(drv => drv.FindElement(By.XPath(sendingTextBox)));

                if (string.IsNullOrEmpty(evnt.MessageHeaderImage))
                {
                    var sendBtn = wait.Until(drv => drv.FindElement(By.XPath(sendTextButton)));
                    sendBtn.Click();
                }
                else
                {
                    string imgUrl = evnt.MessageHeaderImage;
                    tempFilePath = Path.GetTempFileName() + ".jpg";

                    using (var httpClient = new HttpClient())
                    {
                        var bytes = await httpClient.GetByteArrayAsync(imgUrl);
                        await File.WriteAllBytesAsync(tempFilePath, bytes);
                    }

                    Bitmap bmp;
                    using (var stream = new MemoryStream(await File.ReadAllBytesAsync(tempFilePath)))
                    {
                        bmp = new Bitmap(stream);
                    }

                    var t = new Thread(() =>
                    {
                        //Clipboard.SetImage(bmp);
                    });
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    t.Join();

                    chatBox.Click();

                    Actions actions = new Actions(driver);
                    actions.KeyDown(OpenQA.Selenium.Keys.Control)
                           .SendKeys("v")
                           .KeyUp(OpenQA.Selenium.Keys.Control)
                           .Perform();

                    IWebElement sendBtn = wait.Until(drv => drv.FindElement(By.XPath(sendImageButton)));
                    sendBtn.Click();
                }
            }
            catch
            {
                try { driver.Navigate().GoToUrl("https://web.whatsapp.com/send"); } catch { }
                throw;
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
                    File.Delete(tempFilePath);

                driver.Quit();
            }
        }


        /// <summary>
        /// [PRIORITY: HIGH - Card/Invitation image sending]
        ///
        /// Sends an invitation card image to a guest via WhatsApp Web.
        /// Used when Twilio API fails to send the QR code/card image.
        ///
        /// IMAGE SOURCE:
        /// - Downloads from: /upload/preview/{EventId}/E00000{EventId}_{GuestId}_{NoOfMembers}.jpg
        /// - Saves to temp folder, sends via WhatsApp, then deletes
        ///
        /// SENDING MECHANISM:
        /// Uses JavaScript clipboard paste simulation:
        /// 1. Convert image to Base64
        /// 2. Create a synthetic ClipboardEvent in JavaScript
        /// 3. Dispatch paste event to WhatsApp chat box
        /// 4. WhatsApp processes pasted image
        /// 5. Click send button
        ///
        /// WHY PASTE INSTEAD OF FILE INPUT:
        /// - WhatsApp Web's file input is hidden/dynamic
        /// - Paste method is more reliable in headless mode
        /// - Works around WhatsApp's DOM changes
        ///
        /// CALLED FROM:
        /// - DefaultWhatsappController.sendImage()
        /// - Admin panel "Send Failed Image" button
        /// </summary>
        /// <param name="evnt">Event containing card settings</param>
        /// <param name="guest">Guest to send card to</param>
        public async Task SendImage(Events evnt, Guest guest)
        {
            Log.Information("Starting SendImage fallback - GuestId: {GuestId}, EventId: {EventId}", guest.GuestId, evnt.Id);

            UpdateDefaultWhatsAppSettings();
            var options = ChromeOptions();

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            using IWebDriver driver = new ChromeDriver(
                service,
                options,
                TimeSpan.FromMinutes(3));

            string tempFilePath = null;

            try
            {
                string msg = evnt.FailedGuestsCardText ?? "";
                string mobile = (guest.SecondaryContactNo + guest.PrimaryContactNo).TrimStart('+', '0');
                Log.Debug("Preparing to send to mobile: {Mobile}, Message length: {Length}", mobile, msg.Length);

                Log.Information("Navigating to WhatsApp Web chat - Mobile: {Mobile}", mobile);
                driver.Navigate().GoToUrl($"https://web.whatsapp.com/send?phone={mobile}&text={Uri.EscapeDataString(msg)}");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));

                Log.Debug("Waiting for main chat box - XPath: {XPath}", sendingTextBox);
                var chatBox = wait.Until(drv => drv.FindElement(By.XPath(sendingTextBox)));
                Log.Information("Chat box loaded");

                if (!string.IsNullOrWhiteSpace(msg))
                {
                    Log.Debug("Typing message text into chat box");
                    chatBox.SendKeys(msg);
                    Log.Information("Message text typed successfully");
                }

                string publicId = $"cards/{evnt.Id}/E00000{evnt.Id}_{guest.GuestId}_{guest.NoOfMembers}.jpg";
                Log.Information("Fetching card image from Blob Storage - PublicId: {PublicId}", publicId);

                string imageUrl = _blobStorage.GetFileUrl(publicId);
                if (string.IsNullOrEmpty(imageUrl))
                    throw new Exception($"Card image not found: {publicId}");
                Log.Information("Card image URL retrieved - URL: {ImageUrl}", imageUrl);

                tempFilePath = Path.GetTempFileName() + ".jpg";
                Log.Debug("Downloading image to temporary file - Path: {TempFile}", tempFilePath);
                using (var httpClient = new HttpClient())
                {
                    var bytes = await httpClient.GetByteArrayAsync(imageUrl);
                    await File.WriteAllBytesAsync(tempFilePath, bytes);
                }
                Log.Information("Image downloaded successfully");


                Bitmap bmp;
                using (var stream = new MemoryStream(await File.ReadAllBytesAsync(tempFilePath)))
                {
                    bmp = new Bitmap(stream);
                }
                var t = new Thread(() =>
                {
                    //Clipboard.SetImage(bmp);
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();

                // focus chat box
                chatBox.Click();

                // paste (Ctrl+V)
                Actions actions = new Actions(driver);
                actions.KeyDown(OpenQA.Selenium.Keys.Control)
                       .SendKeys("v")
                       .KeyUp(OpenQA.Selenium.Keys.Control)
                       .Perform();

                Log.Information("Image + caption sent successfully");

                var sendBtn = wait.Until(drv => drv.FindElement(By.XPath(sendImageButton)));
                sendBtn.Click();

                await Task.Delay(3000);
                Log.Information("SendImage completed successfully - GuestId: {GuestId}", guest.GuestId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SendImage failed - GuestId: {GuestId}, EventId: {EventId}", guest.GuestId, evnt.Id);
                try { driver.Navigate().GoToUrl("https://web.whatsapp.com/send"); } catch { }
                throw;
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                        Log.Debug("Temporary image file deleted - Path: {TempFile}", tempFilePath);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to delete temporary image file - Path: {TempFile}", tempFilePath);
                    }
                }

                Log.Debug("Disposing ChromeDriver instance");
                driver.Quit();
            }
        }
        #endregion

        #region Secondary Messaging Methods (Priority: MEDIUM)

        /// <summary>
        /// [PRIORITY: MEDIUM - Reminder message sending]
        ///
        /// Sends a reminder message to a guest via WhatsApp Web.
        /// Used to remind guests about the event before it starts.
        ///
        /// MESSAGE CONTENT:
        /// - Guest name + Event's FailedGuestsReminderMessage
        /// - Simple text message (no image attachment)
        ///
        /// TYPICAL USE CASE:
        /// - Day before event reminder
        /// - Retry for guests whose Twilio reminder failed
        ///
        /// CALLED FROM:
        /// - DefaultWhatsappController.sendRminderMessage()
        /// - Admin panel "Send Failed Reminder Msg" button
        /// </summary>
        /// <param name="evnt">Event containing reminder message template</param>
        /// <param name="guest">Guest to send reminder to</param>
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

        #endregion

        #region Tertiary Messaging Methods (Priority: LOW)

        /// <summary>
        /// [PRIORITY: LOW - Thank you/congratulation message]
        ///
        /// Sends a congratulation/thank you message to a guest via WhatsApp Web.
        /// Typically sent after the event to thank guests for attending.
        ///
        /// MESSAGE CONTENT:
        /// - Guest name + Event's FailedGuestsCongratulationMsg
        /// - Optional: Feedback link if ShowFailedSendingCongratulationLink is true
        ///   Format: https://www.EventPro.cc/feedback?id={linkId}
        ///
        /// TYPICAL USE CASE:
        /// - Post-event thank you messages
        /// - Requesting feedback from attendees
        /// - Retry for guests whose Twilio congratulation failed
        ///
        /// CALLED FROM:
        /// - DefaultWhatsappController.sendCongratularionMessage()
        /// - Admin panel "Send Failed Congratulation Msg" button
        /// </summary>
        /// <param name="evnt">Event containing congratulation message template</param>
        /// <param name="guest">Guest to send message to</param>
        /// <param name="linkId">Unique link ID for feedback URL</param>
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

                    //IWebElement searchNewChatButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(searchNewChat)));
                    //searchNewChatButton.SendKeys(mobile.Trim());
                    //Thread.Sleep(1500);
                    //try
                    //{
                    //    IWebElement newChatContactButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(newChatContact)));
                    //    newChatContactButton.Click();
                    //}
                    //catch
                    //{
                    //    //searchNewChatButton.SendKeys(Keys.Backspace);
                    //    //Thread.Sleep(1500);
                    //    //IWebElement newChatContactButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(newChatContact)));
                    //    //newChatContactButton.Click();
                    //}
                    //Thread.Sleep(500);
                    //IWebElement textButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendingTextBox)));
                    ////textButton.SendKeys(Keys.LeftControl + "A");
                    ////textButton.SendKeys(Keys.Backspace);

                    //textButton.SendKeys(msg);
                    //IWebElement sendButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(sendTextButton)));
                    //sendButton.Click();

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

        #endregion
    }
}
