using System.Globalization;
using System.Text.RegularExpressions;

using EventPro.Business.DataProtector;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioConfirmationTemplates : TwilioMessagingConfiguration, IConfirmationMessageTemplates
    {
        private readonly EventProContext db;
        private readonly UrlProtector _urlProtector;
        private readonly ILogger<TwilioMessagingConfiguration> _logger;
        public TwilioConfirmationTemplates(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService, UrlProtector urlProtector, ILogger<TwilioMessagingConfiguration> logger) : base(configuration,
                memoryCacheStoreService,logger)
        {
            db = new EventProContext(configuration);
            _urlProtector = urlProtector;
            _logger = logger;
        }

        // Template Methods for Arabic Confirmation Messages without Guest Name
        public async Task SendArabicbasic(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendArabicbasic started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                             .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                             .AsNoTracking()
                             .FirstOrDefaultAsync();

            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendArabicbasic: ConfirmationButtonsType is QuickReplies");

                if (events.ParentTitleGender == "Female")
                {
                    _logger.LogInformation("SendArabicbasic: Parent Title Gender is Female .");
                    templateId = profileSettings?.ConfirmArabicFemaleWithoutGuestName!;
                }
                else
                {
                    _logger.LogInformation("SendArabicbasic: Parent Title Gender is Male .");
                    templateId = profileSettings?.ConfirmArabicMaleWithoutGuestName!;
                }
            }
            else 
            {
                _logger.LogInformation("SendArabicbasic: ConfirmationButtonsType is Links");
                if (events.ParentTitleGender == "Female")
                {
                    _logger.LogInformation("SendArabicbasic: Parent Title Gender is Female .");
                    templateId = profileSettings?.ConfirmArabicFemaleWithoutGuestNameWithLink!;
                }
                else
                {
                    _logger.LogInformation("SendArabicbasic: Parent Title Gender is Male .");
                    templateId = profileSettings?.ConfirmArabicMaleWithoutGuestNameWithLink!;
                }
            }

            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendArabicbasic: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                    events.ParentTitle.Trim(),
                    events.EventTitle.Trim(),
                    evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                    evntDate.ToString("dd/MM/yyyy"),
                    events.EventVenue.ToString().Trim(),
                    yesButtonId,
                    noButtonId,
                    eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicbasic: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending messages in SendArabicbasic.");
                throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages without Guest Name with Header Image
        public async Task SendArabicbasicHeaderImage(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                 .AsNoTracking()
                 .FirstOrDefaultAsync();
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendArabicbasicHeaderImage: ConfirmationButtonsType is QuickReplies");
                if (events.ParentTitleGender == "Female")
                {
                    _logger.LogInformation("SendArabicbasicHeaderImage: Parent Title Gender is Female");
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndWithoutGuestName;
                }
                else
                {
                    _logger.LogInformation("SendArabicbasicHeaderImage: Parent Title Gender is Male ");
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndWithoutGuestName;
                }
            }
            else
            {
                if (events.ParentTitleGender == "Female")
                {
                    _logger.LogInformation("SendArabicbasicHeaderImage: Parent Title Gender is Female");
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndWithoutGuestNameWithLink;
                }
                else
                {
                    _logger.LogInformation("SendArabicbasicHeaderImage: Parent Title Gender is Male ");
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndWithoutGuestNameWithLink;
                }
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation(
                   "SendArabicbasicHeaderImage started. EventId {EventId}, GuestsCount {GuestsCount}",
                   events.Id,
                   guests.Count);
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderImage.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicbasicHeaderImage: Completed sending messages in parallel.");  
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicbasicHeaderImage.");
                 throw;
            }
             _logger.LogInformation(
                   "SendArabicbasicHeaderImage completed. EventId {EventId}, GuestsCount {GuestsCount}",
                   events.Id,
                   guests.Count);
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages without Guest Name with Header Text
        public async Task SendArabicbasicHeaderText(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendArabicbasicHeaderText started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                               .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                               .AsNoTracking()
                               .FirstOrDefaultAsync();
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendArabicbasicHeaderText: ConfirmationButtonsType is QuickReplies");   
                if (events.ParentTitleGender == "Female")
                {
                    _logger.LogInformation("SendArabicbasicHeaderText: Parent Title Gender is Female");
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderTextAndWithoutGuestName;
                }
                else
                {
                    _logger.LogInformation("SendArabicbasicHeaderText: Parent Title Gender is Male ");
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderTextAndWithoutGuestName;
                }
            }
            else
            {
                _logger.LogInformation("SendArabicbasicHeaderText: ConfirmationButtonsType is Links");
                if (events.ParentTitleGender == "Female")
                {
                    _logger.LogInformation("SendArabicbasicHeaderText: Parent Title Gender is Female");
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderTextAndWithoutGuestNameWithLink;
                }
                else
                {
                    _logger.LogInformation("SendArabicbasicHeaderText: Parent Title Gender is Male ");
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderTextAndWithoutGuestNameWithLink;
                }

            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendArabicbasicHeaderText: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicbasicHeaderText.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages without Guest Name with Header Text and Image
        public async Task SendArabicbasicHeaderTextImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendArabicbasicHeaderTextImage started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                   .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                   .AsNoTracking()
                   .FirstOrDefaultAsync();

            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendArabicbasicHeaderTextImage: ConfirmationButtonsType is QuickReplies");
                if (events.ParentTitleGender == "Female")
                {
                    _logger.LogInformation("SendArabicbasicHeaderTextImage: Parent Title Gender is Female");
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithoutGuestName;
                }
                else
                {
                    _logger.LogInformation("SendArabicbasicHeaderTextImage: Parent Title Gender is Male");
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithoutGuestName;
                }
            }
            else
            {
                _logger.LogInformation("SendArabicbasicHeaderTextImage: ConfirmationButtonsType is Links");
                if (events.ParentTitleGender == "Female")
                {
                    _logger.LogInformation("SendArabicbasicHeaderTextImage: Parent Title Gender is Female");
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink;
                }
                else
                {
                    _logger.LogInformation("SendArabicbasicHeaderTextImage: Parent Title Gender is Male");
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink;
                }
            }
            int counter = SetSendingCounter(guests, events);
            try
            {
                _logger.LogInformation("SendArabicbasicHeaderTextImage: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderImage.ToString(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicbasicHeaderTextImage: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicbasicHeaderTextImage.");
                 throw;
            }
             _logger.LogInformation(
               "SendArabicbasicHeaderTextImage completed. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count);
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages with Guest Name
        public async Task SendArabicFemaleDefault(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendArabicFemaleDefault started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var profileSettings = await db.TwilioProfileSettings
                      .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                      .AsNoTracking()
                      .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies") 
            {
                _logger.LogInformation("SendArabicFemaleDefault: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmArabicFemaleWithGuestName;
            }
            else 
            {
                _logger.LogInformation("SendArabicFemaleDefault: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmArabicFemaleWithGuestNameWithLink;
            }
            var evntDate = Convert.ToDateTime(events.EventFrom);
            
            int counter = SetSendingCounter(guests, events);

            try 
            {
                _logger.LogInformation("SendArabicFemaleDefault: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                    guest.FirstName.Trim(),
                    events.ParentTitle.Trim(),
                    events.EventTitle.Trim(),
                    evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                    evntDate.ToString("dd/MM/yyyy"),
                    events.EventVenue.ToString().Trim(),
                    yesButtonId,
                    noButtonId,
                    eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicFemaleDefault: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicFemaleDefault.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages with Guest Name with Header Image
        public async Task SendArabicFemaleWithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendArabicFemaleWithHeaderImage started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = string.Empty;
            var profileSettings = await db.TwilioProfileSettings
                               .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                               .AsNoTracking()
                               .FirstOrDefaultAsync();
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendArabicFemaleWithHeaderImage: ConfirmationButtonsType is QuickReplies");
                if (events.MessageHeaderImage!.ToLower().EndsWith(".mp4"))
                {
                    _logger.LogInformation("SendArabicFemaleWithHeaderImage: Header is Video");
                    templateId = profileSettings.ConfirmArabicFemaleWithHeaderVideoAndWithGuestName;
                }
                else
                {
                    _logger.LogInformation("SendArabicFemaleWithHeaderImage: Header is Image");
                    templateId = profileSettings.ConfirmArabicFemaleWithHeaderImageAndWithGuestName;

                }
            }
            else
            {
                _logger.LogInformation("SendArabicFemaleWithHeaderImage: ConfirmationButtonsType is Links");
                if (events.MessageHeaderImage!.ToLower().EndsWith(".mp4"))
                {
                    _logger.LogInformation("SendArabicFemaleWithHeaderImage: Header is Video");
                    templateId = profileSettings.ConfirmArabicFemaleWithHeaderVideoAndWithGuestNameWithLink;
                }
                else
                {
                    _logger.LogInformation("SendArabicFemaleWithHeaderImage: Header is Image");
                    templateId = profileSettings.ConfirmArabicFemaleWithHeaderImageAndWithGuestNameWithLink;

                }
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendArabicFemaleWithHeaderImage: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderImage.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicFemaleWithHeaderImage: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicFemaleWithHeaderImage.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages with Guest Name with Header Image and Header Text
        public async Task SendArabicFemaleWithHeaderImageAndHeaderText(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendArabicFemaleWithHeaderImageAndHeaderText started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendArabicFemaleWithHeaderImageAndHeaderText: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithGuestName;
            }
            else
            {
                _logger.LogInformation("SendArabicFemaleWithHeaderImageAndHeaderText: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendArabicFemaleWithHeaderImageAndHeaderText: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                   
                    // Ali: Get BackgroundImage from CardInfo table and extract filename only
                    string imageFilename = string.Empty;
                    var cardInfo = await db.CardInfo.Where(c => c.EventId == events.Id).AsNoTracking().FirstOrDefaultAsync();
                    
                    if (cardInfo != null && !string.IsNullOrEmpty(cardInfo.BackgroundImage))
                    {
                        // BackgroundImage is a full URL: https://.../card/filename.jpg
                        // We need just "filename.jpg"
                        try 
                        {
                            var uri = new Uri(cardInfo.BackgroundImage);
                            string path = uri.LocalPath; // /.../card/filename.jpg
                            imageFilename = System.IO.Path.GetFileName(path);
                        }
                        catch
                        {
                            // Fallback if not a valid URI, try simple string split
                            var parts = cardInfo.BackgroundImage.Split('/');
                            imageFilename = parts.Last();
                        }
                    }

                    imageFilename = Uri.EscapeDataString(imageFilename ?? "");

                    var parameters = new string[]
                    {

                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.Trim(),
                imageFilename, // Ali: filename only from CardInfo
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);

                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicFemaleWithHeaderImageAndHeaderText: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicFemaleWithHeaderImageAndHeaderText.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages with Guest Name with Header Text
        public async Task SendArabicFemaleWithHeaderText(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendArabicFemaleWithHeaderText started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies") 
            {
                 _logger.LogInformation("SendArabicFemaleWithHeaderText: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmArabicFemaleWithHeaderTextAndWithGuestName;
            }
            else 
            {
                _logger.LogInformation("SendArabicFemaleWithHeaderText: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmArabicFemaleWithHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendArabicFemaleWithHeaderText: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {


                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderText,
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicFemaleWithHeaderText: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicFemaleWithHeaderText.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages with Guest Name
        public async Task SendArabicMaleDefault(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendArabicMaleDefault started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendArabicMaleDefault: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmArabicMaleWithGuestName;
            }
            else 
            {
                _logger.LogInformation("SendArabicMaleDefault: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmArabicMaleWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendArabicMaleDefault: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicMaleDefault: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicMaleDefault.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages with Guest Name with Header Image
        public async Task SendArabicMaleWithHeaderImage(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendArabicMaleWithHeaderImage started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies") 
            {
                _logger.LogInformation("SendArabicMaleWithHeaderImage: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndWithGuestName;
            }
            else 
            {
                _logger.LogInformation("SendArabicMaleWithHeaderImage: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendArabicMaleWithHeaderImage: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderImage,
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicMaleWithHeaderImage: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicMaleWithHeaderImage.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages with Guest Name with Header Image and Header Text
        public async Task SendArabicMaleWithHeaderImageAndHeaderText(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendArabicMaleWithHeaderImageAndHeaderText started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies") 
            {
                _logger.LogInformation("SendArabicMaleWithHeaderImageAndHeaderText: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithGuestName;
            }
            else 
            {
                _logger.LogInformation("SendArabicMaleWithHeaderImageAndHeaderText: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendArabicMaleWithHeaderImageAndHeaderText: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.Trim(),
                events.MessageHeaderImage,
                events.MessageHeaderText,
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicMaleWithHeaderImageAndHeaderText: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicMaleWithHeaderImageAndHeaderText.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Arabic Confirmation Messages with Guest Name with Header Text
        public async Task SendArabicMaleWithHeaderText(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendArabicMaleWithHeaderText started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies") 
            {
                _logger.LogInformation("SendArabicMaleWithHeaderText: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderTextAndWithGuestName;
            }
            else 
            {
                _logger.LogInformation("SendArabicMaleWithHeaderText: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendArabicMaleWithHeaderText: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderText,
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendArabicMaleWithHeaderText: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendArabicMaleWithHeaderText.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for English Confirmation Messages without Guest Name with Header Image
        public async Task SendbasicHeaderImageEnglish(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendbasicHeaderImageEnglish started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies") 
            {
                _logger.LogInformation("SendbasicHeaderImageEnglish: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndWithoutGuestName;
            }
            else 
            {
                _logger.LogInformation("SendbasicHeaderImageEnglish: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndWithoutGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendbasicHeaderImageEnglish: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd"),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderImage.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendbasicHeaderImageEnglish: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendbasicHeaderImageEnglish.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for English Confirmation Messages without Guest Name with Header Text
        public async Task SendbasicHeaderTextEnglish(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendbasicHeaderTextEnglish started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies") 
            {
                _logger.LogInformation("SendbasicHeaderTextEnglish: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmEnglishWithHeaderTextAndWithoutGuestName;
            }
            else 
            {
                _logger.LogInformation("SendbasicHeaderTextEnglish: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmEnglishWithHeaderTextAndWithoutGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendbasicHeaderTextEnglish: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {


                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd"),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendbasicHeaderTextEnglish: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendbasicHeaderTextEnglish.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for English Confirmation Messages without Guest Name with Header Image and Header Text
        public async Task SendbasicHeaderTextImageEnglish(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
               "SendbasicHeaderTextImageEnglish started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies") 
            {
                _logger.LogInformation("SendbasicHeaderTextImageEnglish: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndHeaderTextAndWithoutGuestName;
            }
            else 
            {
                _logger.LogInformation("SendbasicHeaderTextImageEnglish: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendbasicHeaderTextImageEnglish: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd"),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderImage.ToString(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendbasicHeaderTextImageEnglish: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendbasicHeaderTextImageEnglish.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Custom Confirmation Messages
        public async Task SendCustomBasic(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendCustomBasic started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendCustomBasic: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendCustomBasic: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendCustomBasic.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Custom Confirmation Messages with Header Image
        public async Task SendCustomBasicHeaderImage(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendCustomBasicHeaderImage started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendCustomBasicHeaderImage: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.MessageHeaderImage.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendCustomBasicHeaderImage: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendCustomBasicHeaderImage.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Custom Confirmation Messages with Header Text
        public async Task SendCustomBasicHeaderText(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendCustomBasicHeaderText started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendCustomBasicHeaderText: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendCustomBasicHeaderText: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendCustomBasicHeaderText.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Custom Confirmation Messages with Header Image and Header Text
        public async Task SendCustomBasicHeaderTextImage(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendCustomBasicHeaderTextImage started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendCustomBasicHeaderTextImage: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.MessageHeaderImage.ToString(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendCustomBasicHeaderTextImage: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendCustomBasicHeaderTextImage.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Custom Confirmation Messages with Guest Name
        public async Task SendCustomWithName(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendCustomWithName started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendCustomWithName: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendCustomWithName: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendCustomWithName.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Custom Confirmation Messages with Guest Name with Header Image
        public async Task SendCustomWithNameHeaderImage(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendCustomWithNameHeaderImage started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendCustomWithNameHeaderImage: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.MessageHeaderImage.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendCustomWithNameHeaderImage: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendCustomWithNameHeaderImage.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Custom Confirmation Messages with Guest Name with Header Text
        public async Task SendCustomWithNameHeaderText(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendCustomWithNameHeaderText started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendCustomWithNameHeaderText: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendCustomWithNameHeaderText: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendCustomWithNameHeaderText.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Custom Confirmation Messages with Guest Name with Header Image and Header Text
        public async Task SendCustomWithNameHeaderTextImage(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendCustomWithNameHeaderTextImage started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            try
            {
                _logger.LogInformation("SendCustomWithNameHeaderTextImage: Starting to send messages in parallel.");
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.MessageHeaderImage.ToString(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendCustomWithNameHeaderTextImage: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendCustomWithNameHeaderTextImage.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for English Confirmation Messages without Guest Name
        public async Task SendEnglishbasic(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendEnglishbasic started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendEnglishbasic: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmEnglishWithoutGuestName;
            }
            else
            {
                 _logger.LogInformation("SendEnglishbasic: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmEnglishWithoutGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);
            _logger.LogInformation("SendEnglishbasic: Initial counter set to {Counter}.", counter);
            _logger.LogInformation("SendEnglishbasic: Starting to send messages in parallel.");

            try
            {
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd"),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendEnglishbasic: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendEnglishbasic.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for English Confirmation Messages with Guest Name
        public async Task SendEnglishDefault(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendEnglishDefault started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendEnglishDefault: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmEnglishWithGuestName;
            }
            else
            {
                 _logger.LogInformation("SendEnglishDefault: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmEnglishWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);
            _logger.LogInformation("SendEnglishDefault: Initial counter set to {Counter}.", counter);
            _logger.LogInformation("SendEnglishDefault: Starting to send messages in parallel.");

            try
            {
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd"),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendEnglishDefault: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendEnglishDefault.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for English Confirmation Messages with Guest Name with Header Image
        public async Task SendEnglishWithHeaderImage(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendEnglishWithHeaderImage started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendEnglishWithHeaderImage: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndWithGuestName;
            }
            else
            {
                _logger.LogInformation("SendEnglishWithHeaderImage: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);
            _logger.LogInformation("SendEnglishWithHeaderImage: Initial counter set to {Counter}.", counter);
            _logger.LogInformation("SendEnglishWithHeaderImage: Starting to send messages in parallel.");

            try
            {
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd"),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderImage,
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendEnglishWithHeaderImage: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendEnglishWithHeaderImage.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for English Confirmation Messages with Guest Name with Header Image and Header Text
        public async Task SendEnglishWithHeaderImageAndHeaderText(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendEnglishWithHeaderImageAndHeaderText started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendEnglishWithHeaderImageAndHeaderText: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndHeaderTextAndWithGuestName;
            }
            else
            {
                _logger.LogInformation("SendEnglishWithHeaderImageAndHeaderText: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);
            _logger.LogInformation("SendEnglishWithHeaderImageAndHeaderText: Initial counter set to {Counter}.", counter);
            _logger.LogInformation("SendEnglishWithHeaderImageAndHeaderText: Starting to send messages in parallel.");

            try
            {
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);

                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd"),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderImage,
                events.MessageHeaderText,
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendEnglishWithHeaderImageAndHeaderText: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendEnglishWithHeaderImageAndHeaderText.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for English Confirmation Messages with Guest Name with Header Text
        public async Task SendEnglishWithHeaderText(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendEnglishWithHeaderText started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                 .AsNoTracking()
                 .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                _logger.LogInformation("SendEnglishWithHeaderText: ConfirmationButtonsType is QuickReplies");
                templateId = profileSettings?.ConfirmEnglishWithHeaderTextAndWithGuestName;
            }
            else
            {
                 _logger.LogInformation("SendEnglishWithHeaderText: ConfirmationButtonsType is Links");
                templateId = profileSettings?.ConfirmEnglishWithHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);
            _logger.LogInformation("SendEnglishWithHeaderText: Initial counter set to {Counter}.", counter);
            _logger.LogInformation("SendEnglishWithHeaderText: Starting to send messages in parallel.");

            try
            {
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                    var parameters = new string[]
                    {
                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd"),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.MessageHeaderText,
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                    };

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendEnglishWithHeaderText: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendEnglishWithHeaderText.");
                 throw;
            }
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }
        // Template Methods for Custom Confirmation Messages with Variables
        public async Task SendCustomTemplateWithVariables(List<Guest> guests, Events events)
        {
             _logger.LogInformation(
               "SendCustomTemplateWithVariables started. EventId {EventId}, GuestsCount {GuestsCount}",
               events.Id,
               guests.Count
           );
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);
            _logger.LogInformation("SendCustomTemplateWithVariables: Starting to send messages in parallel.");

            try
            {
                await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
                {
                    string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                    string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                    string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                    string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);

                    var matches = Regex.Matches(events.CustomConfirmationTemplateWithVariables, @"\{\{(.*?)\}\}");

                    List<string> templateParameters =
                         matches.Cast<Match>()
                        .Select(m =>
                        {
                            string propName = m.Groups[1].Value;
                            if (propName == "GuestCard")
                            {
                                return GetFullImageUrl(events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg", "cards");
                            }

                            if (propName == "CountOfAdditionalInvitations")
                            {
                                return (guest.NoOfMembers - 1)?.ToString() ?? "0";
                            }
                            var value = guest.GetType().GetProperty(propName)?
                                              .GetValue(guest, null)?.ToString();
                            if (value == null)
                            {
                                value = events.GetType().GetProperty(propName)?
                                              .GetValue(events, null)?.ToString();
                            }
                            return value ?? propName;
                        })
                        .ToList();

                    // Append button IDs at the end of the parameters list
                    templateParameters.Add(yesButtonId);
                    templateParameters.Add(noButtonId);
                    templateParameters.Add(eventLocationButtonId);
                    string[] parameters = templateParameters.ToArray();

                    await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                    counter = UpdateCounter(guests, events, counter);
                });
                _logger.LogInformation("SendCustomTemplateWithVariables: Completed sending messages in parallel.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while sending messages in SendCustomTemplateWithVariables.");
                 throw;
            }
            //update database and clear cache
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        // Common Methods
        private async Task updateDataBaseAndDisposeCache(List<Guest> guests, Events events)
        {
            _logger.LogInformation(
            "Updating database and disposing cache. EventId {EventId}, GuestsCount {GuestsCount}",
            events.Id,
            guests.Count);
            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();
            if (guests.Count > 1)
            {
                await Task.Delay(10000);
                _memoryCacheStoreService.delete(events.Id.ToString());
                foreach (var guest in guests)
                {
                    if (guest.MessageId != null)
                    {
                        _memoryCacheStoreService.delete(guest.MessageId);
                    }
                }
            }
        }

        private async Task SendMessageAndUpdateStatus(Events events, string templateId, Guest guest, string fullPhoneNumber, string yesButtonId, string noButtonId, string eventLocationButtonId, string[] parameters, List<Guest> guests, TwilioProfileSettings profileSettings)
        {
            try
            {
                _logger.LogInformation(
                    "Attempting to send WhatsApp message. EventId {EventId}, GuestId {GuestId}, TemplateId {TemplateId}, To {To}",
                    events.Id,
                    guest.GuestId,
                    templateId,
                    fullPhoneNumber
                );

                // Ali hani // Send the WhatsApp template message and get the message SID for tracking status , 
                string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);
                
                if (messageSid != null)
                {
                    _logger.LogInformation(
                        "WhatsApp message sent successfully. EventId {EventId}, GuestId {GuestId}, MessageSid {MessageSid}",
                        events.Id,
                        guest.GuestId,
                        messageSid
                    );

                    guest.MessageId = messageSid;
                    guest.Response = "Message Processed Successfully";
                    guest.YesButtonId = yesButtonId;
                    guest.NoButtonId = noButtonId;
                    guest.EventLocationButtonId = eventLocationButtonId;
                    guest.WasentOn = DateTime.Now.ToString();
                    guest.TextDelivered = null;
                    guest.TextRead = null;
                    guest.TextSent = null;
                    guest.TextFailed = null;
                    guest.ConguratulationMsgId = null;
                    guest.ConguratulationMsgFailed = null;
                    guest.ConguratulationMsgDelivered = null;
                    guest.ConguratulationMsgSent = null;
                    guest.ConguratulationMsgRead = null;
                    guest.ImgDelivered = null;
                    guest.ImgFailed = null;
                    guest.ImgRead = null;
                    guest.ImgSent = null;
                    guest.ImgSentMsgId = null;
                    guest.WaresponseTime = null;
                    guest.whatsappMessageEventLocationId = null;
                    guest.EventLocationSent = null;
                    guest.EventLocationRead = null;
                    guest.EventLocationDelivered = null;
                    guest.EventLocationFailed = null;
                    guest.waMessageEventLocationForSendingToAll = null;
                    guest.ReminderMessageId = null;
                    guest.ReminderMessageSent = null;
                    guest.ReminderMessageRead = null;
                    guest.ReminderMessageDelivered = null;
                    guest.ReminderMessageFailed = null;

                    if (guests.Count > 1)
                    {
                        _memoryCacheStoreService.save(messageSid, 0);
                    }
                }
                else
                {
                    _logger.LogError(
                        "WhatsApp message sending failed (SID is null). EventId {EventId}, GuestId {GuestId}",
                        events.Id,
                        guest.GuestId
                    );
                    guest.MessageId = null;
                    guest.Response = "WA Error";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception in SendMessageAndUpdateStatus. EventId {EventId}, GuestId {GuestId}",
                    events.Id,
                    guest.GuestId
                );
                guest.MessageId = null;
                guest.Response = "WA Error Exception";
            }
        }

    }
}
