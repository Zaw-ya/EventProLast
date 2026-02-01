using EventPro.Business.MemoryCacheStore.Implementaiion;
using EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.Business.WhatsAppMessagesWebhook.Interface;
using EventPro.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace EventPro.Business.WhatsAppMessagesWebhook.Implementation
{
    public class TwilioWebhookService : ITwilioWebhookService
    {
        private readonly IConfiguration _configuration;
        public readonly IWhatsappSendingProviderService _whatsappSendingProviderService;
        private readonly DistributedLockHelper _distributedLockHelper;
        private readonly ILogger<TwilioCardTemplates> _logger;
        private readonly IDbContextFactory<EventProContext> _dbFactory;

        public TwilioWebhookService(IConfiguration configuration, IWhatsappSendingProviderService whatsappSendingProviderService,
            DistributedLockHelper distributedLockHelper,
            IDbContextFactory<EventProContext> dbFactory,
            ILogger<TwilioCardTemplates> logger)
        {
            _configuration = configuration;
            _whatsappSendingProviderService = whatsappSendingProviderService;
            _distributedLockHelper = distributedLockHelper;
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task ProcessStatusAsync(dynamic obj)
        {
            try
            {
                _logger.LogInformation($"ProcessStatusAsync Raw Data: {JsonConvert.SerializeObject(obj)}");
                MessageParameters parameters = GetMessageParameters(obj);

                if (string.IsNullOrEmpty(parameters.Status) || !string.IsNullOrEmpty(parameters.Messages))
                {
                    _logger.LogWarning("ProcessStatusAsync: Invalid parameters. Status is empty or Messages is not empty. Status={Status}, Messages={Messages}", parameters.Status, parameters.Messages);
                    return;
                }

                _logger.LogInformation("ProcessStatusAsync: Processing status update. MsgId={MsgId}, Status={Status}", parameters.MsgId, parameters.Status);

                int guestId;

                // First, find which guest this message belongs to (outside lock for performance)
                await using (var lookupDb = await _dbFactory.CreateDbContextAsync())
                {
                    guestId = await lookupDb.Guest
                        .Where(p => p.GuestArchieved == false &&
                            (p.MessageId == parameters.MsgId ||
                             p.ImgSentMsgId == parameters.MsgId ||
                             p.waMessageEventLocationForSendingToAll == parameters.MsgId ||
                             p.ConguratulationMsgId == parameters.MsgId ||
                             p.ReminderMessageId == parameters.MsgId))
                        .Select(p => p.GuestId)
                        .FirstOrDefaultAsync();
                }

                if (guestId == default)
                {
                    _logger.LogWarning("ProcessStatusAsync: Guest not found for MsgId={MsgId}", parameters.MsgId);
                    return;
                }

                _logger.LogInformation("ProcessStatusAsync: Guest found. GuestId={GuestId}, MsgId={MsgId}", guestId, parameters.MsgId);

                // Now lock and process with fresh data
                await _distributedLockHelper.RunWithLockAsync($"redLockKey{guestId}", async () =>
                {
                    await using var db = await _dbFactory.CreateDbContextAsync();
                    await using var transaction = await db.Database.BeginTransactionAsync();

                    try
                    {
                        var guest = await db.Guest
                            .Where(p => p.GuestId == guestId)
                            .FirstOrDefaultAsync();

                        if (guest == null)
                        {
                            _logger.LogWarning("ProcessStatusAsync: Guest not found inside lock. GuestId={GuestId}", guestId);
                            return;
                        }

                        bool updated = false;

                        // Check each message type and update accordingly
                        if (guest.MessageId == parameters.MsgId)
                        {
                            UpdateTextStatus(guest, parameters.Status);
                            updated = true;
                            _logger.LogInformation("ProcessStatusAsync: Updated TextStatus for GuestId={GuestId}, NewStatus={Status}", guestId, parameters.Status);
                        }
                        else if (guest.ImgSentMsgId == parameters.MsgId)
                        {
                            UpdateImgStatus(guest, parameters.Status);
                            updated = true;
                            _logger.LogInformation("ProcessStatusAsync: Updated ImgStatus for GuestId={GuestId}, NewStatus={Status}", guestId, parameters.Status);
                        }
                        else if (guest.waMessageEventLocationForSendingToAll == parameters.MsgId)
                        {
                            UpdateEventLocationStatus(guest, parameters.Status);
                            updated = true;
                            _logger.LogInformation("ProcessStatusAsync: Updated EventLocationStatus for GuestId={GuestId}, NewStatus={Status}", guestId, parameters.Status);
                        }
                        else if (guest.ConguratulationMsgId == parameters.MsgId)
                        {
                            UpdateCongratulationStatus(guest, parameters.Status);
                            updated = true;
                            _logger.LogInformation("ProcessStatusAsync: Updated CongratulationStatus for GuestId={GuestId}, NewStatus={Status}", guestId, parameters.Status);
                        }
                        else if (guest.ReminderMessageId == parameters.MsgId)
                        {
                            UpdateReminderStatus(guest, parameters.Status);
                            updated = true;
                            _logger.LogInformation("ProcessStatusAsync: Updated ReminderStatus for GuestId={GuestId}, NewStatus={Status}", guestId, parameters.Status);
                        }

                        if (updated)
                        {
                            await db.SaveChangesAsync();
                            await transaction.CommitAsync();
                            _logger.LogInformation("ProcessStatusAsync: Successfully received status update for GuestId={GuestId}, MsgId={MsgId}", guestId, parameters.MsgId);
                        }
                        else
                        {
                            _logger.LogWarning("ProcessStatusAsync: No matching message ID found inside lock for GuestId={GuestId}, MsgId={MsgId}", guestId, parameters.MsgId);
                            await transaction.RollbackAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ProcessStatusAsync: Error inside lock for GuestId={GuestId}", guestId);
                        try { await transaction.RollbackAsync(); } catch { }
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessStatusAsync: Unhandled error");
            }
        }

        /*
       Raw Data: {"ChannelPrefix":null,
        "MessagingServiceSid":"MG8e883e6a32f979dd798446dc0d401bd9",
        "ApiVersion":"2010-04-01",
        "MessageStatus":null,
        "SmsSid":"SM637fb5f3b6eb903917943546a5d61aa6",
        "SmsStatus":"received",
        "ChannelInstallSid":null,
        "To":"whatsapp:+201035460160",
        "From":"whatsapp:+201012042912",
        "MessageSid":"SM637fb5f3b6eb903917943546a5d61aa6",
        "AccountSid":"AC3a85f12b7d1cc23b1bfd205a1fdc15ec",
        "ChannelToAddress":null,
        "OriginalRepliedMessageSid":"MM0deda00296b0e4abc18c2352f270817f",
        "ButtonPayload":"قبول الدعوة",
        "ButtonText":"قبول الدعوة",
        "OriginalRepliedMessageSender":"whatsapp:+201035460160"
        ,"SmsMessageSid":"SM637fb5f3b6eb903917943546a5d61aa6"
        ,"NumMedia":"0",
        "ProfileName":"❤️",
        "WaId":"201012042912",
        "MessageType":"button",
        "Body":"قبول الدعوة",
        "NumSegments":"1",
        "ReferralNumMedia":"0"}
       */

        public async Task ProcessIncomingMessageAsync(dynamic obj)
        {
            try
            {
                _logger.LogInformation($"ProcessIncomingMessageAsync Raw Data: {JsonConvert.SerializeObject(obj)}");
                MessageParameters parameters = GetMessageParameters(obj);

                _logger.LogInformation("ProcessIncomingMessageAsync: MsgId={MsgId}, BtnText={BtnText}, Body={Body}, ButtonPayLoad={ButtonPayLoad}, OriginalMsgId={OriginalMsgId}, WaId={WaId}",
                    parameters.MsgId, parameters.BtnText, parameters.Messages, parameters.ButtonPayLoad, parameters.OriginalMsgId, parameters.Recepient);

                // For WhatsApp Card Quick Reply buttons, Twilio may send the button text
                // only in Body (not in ButtonText). Fall back to Body when ButtonText is empty.
                if (string.IsNullOrEmpty(parameters.BtnText) && !string.IsNullOrEmpty(parameters.Messages))
                    parameters.BtnText = parameters.Messages;

                if (string.IsNullOrEmpty(parameters.BtnText))
                {
                    _logger.LogWarning("ProcessIncomingMessageAsync: No BtnText or Body found, skipping. MsgId={MsgId}", parameters.MsgId);
                    return;
                }

                int guestId;
                bool isValidKeyword;

                await using (var lookupDb = await _dbFactory.CreateDbContextAsync())
                {
                    // Check if this is a valid button keyword
                    isValidKeyword = await lookupDb.ConfirmationMessageResponsesKeyword
                        .AnyAsync(e => e.KeywordValue == parameters.BtnText);

                    if (!isValidKeyword)
                    {
                        _logger.LogWarning("ProcessIncomingMessageAsync: BtnText '{BtnText}' is not a valid keyword, skipping. MsgId={MsgId}", parameters.BtnText, parameters.MsgId);
                        return;
                    }

                    _logger.LogInformation("ProcessIncomingMessageAsync: Keyword is valid. Looking up guest...");

                    guestId = await lookupDb.Guest
                        .Where(p => p.GuestArchieved == false &&
                            (
                                (!string.IsNullOrEmpty(parameters.ButtonPayLoad) &&
                                    (p.YesButtonId == parameters.ButtonPayLoad ||
                                     p.NoButtonId == parameters.ButtonPayLoad ||
                                     p.EventLocationButtonId == parameters.ButtonPayLoad))
                                ||
                                (!string.IsNullOrEmpty(parameters.OriginalMsgId) &&
                                    p.MessageId == parameters.OriginalMsgId)
                            ))
                        .Select(p => p.GuestId)
                        .FirstOrDefaultAsync();

                    if (guestId != default)
                        _logger.LogInformation("ProcessIncomingMessageAsync: Guest found by WaId fallback. GuestId={GuestId}", guestId);
                }


                if (guestId == default)
                {
                    _logger.LogWarning("ProcessIncomingMessageAsync: Guest not found by any strategy. MsgId={MsgId}, WaId={WaId}, OriginalMsgId={OriginalMsgId}, ButtonPayLoad={ButtonPayLoad}",
                        parameters.MsgId, parameters.Recepient, parameters.OriginalMsgId, parameters.ButtonPayLoad);
                    return;
                }

                _logger.LogInformation("ProcessIncomingMessageAsync: Guest found. GuestId={GuestId}", guestId);

                var whatsappProvider = _whatsappSendingProviderService?.SelectTwilioSendingProvider();
                if (whatsappProvider == null)
                {
                    _logger.LogError("ProcessIncomingMessageAsync: whatsappProvider is null!");
                    return;
                }
                _logger.LogInformation("ProcessIncomingMessageAsync: whatsappProvider selected. whatsappProvider={whatsappProviders}", whatsappProvider);

                // Lock and process with fresh data

                //await _distributedLockHelper.RunWithLockAsync($"redLockKey{guestId}", async () =>

                _logger.LogInformation("ProcessIncomingMessageAsync: Acquired lock for GuestId={GuestId}", guestId);
                await using var db = await _dbFactory.CreateDbContextAsync();
                await using var transaction = await db.Database.BeginTransactionAsync();
                _logger.LogInformation("ProcessIncomingMessageAsync: Started transaction for GuestId={GuestId}", guestId);

                try
                {
                    var guest = await db.Guest
                        .Where(p => p.GuestId == guestId)
                        .FirstOrDefaultAsync();

                    _logger.LogInformation("ProcessIncomingMessageAsync: Fetched guest inside lock. GuestId={GuestId}", guestId);
                    if (guest == null)
                    {
                        _logger.LogWarning("ProcessIncomingMessageAsync: Guest not found inside lock. GuestId={GuestId}", guestId);
                        await transaction.RollbackAsync();
                        return;
                    }
                    _logger.LogInformation("ProcessIncomingMessageAsync: Guest current response: {Response}", guest.Response);

                    var _event = await db.Events
                        .Where(p => p.Id == guest.EventId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                    _logger.LogInformation("ProcessIncomingMessageAsync: Fetched event inside lock. GuestId={GuestId}, EventId={EventId}", guestId, guest.EventId);
                    if (_event == null)
                    {
                        _logger.LogWarning("ProcessIncomingMessageAsync: Event not found for GuestId={GuestId}", guestId);
                        await transaction.RollbackAsync();
                        return;
                    }

                    _logger.LogInformation("ProcessIncomingMessageAsync: Inside lock. GuestId={GuestId}, EventId={EventId}, CurrentResponse={Response}", guestId, _event.Id, guest.Response);

                    guest.TextRead = true;
                    guest.MsgResponse = parameters.BtnText;

                    var btnKeyword = await db.ConfirmationMessageResponsesKeyword
                        .Where(e => e.KeywordValue == parameters.BtnText)
                        .Select(e => e.KeywordKey)
                        .FirstOrDefaultAsync();

                    _logger.LogInformation("ProcessIncomingMessageAsync: BtnKeyWord={BtnKeyWord}", btnKeyword);

                    if (btnKeyword == "confirm_button" || btnKeyword == "decline_button")
                    {
                        bool isFirstResponse = guest.Response == "Message Processed Successfully";
                        _logger.LogInformation("ProcessIncomingMessageAsync: isFirstResponse={IsFirstResponse}", isFirstResponse);

                        if (isFirstResponse)
                        {
                            _logger.LogInformation("ProcessIncomingMessageAsync: Processing confirmation response if isFirstResponse. GuestId={GuestId}, BtnKeyWord={BtnKeyWord}", guestId, btnKeyword);
                            await ProcessConfirmationResponse(db, whatsappProvider, guest, _event, btnKeyword);
                            await db.SaveChangesAsync();
                            await transaction.CommitAsync();
                            _logger.LogInformation("ProcessIncomingMessageAsync: Confirmation processed and saved. GuestId={GuestId}, Response={Response}", guestId, guest.Response);
                        }
                        else
                        {
                            _logger.LogInformation("ProcessIncomingMessageAsync: Duplicate confirmation response received. GuestId={GuestId}, BtnKeyWord={BtnKeyWord}", guestId, btnKeyword);
                            await db.SaveChangesAsync();
                            await transaction.CommitAsync();
                            var guests = new List<Guest> { guest };
                            _logger.LogInformation("ProcessIncomingMessageAsync: Sending duplicate response. GuestId={GuestId}", guestId);
                            await SendDuplicateResponse(db, whatsappProvider, guest, guests, _event, btnKeyword, parameters.BtnText);
                        }
                        return;
                    }
                    else if (btnKeyword == "eventlocation_button")
                    {
                        _logger.LogInformation("ProcessIncomingMessageAsync: Processing event location request. GuestId={GuestId}", guestId);
                        if (guest.waMessageEventLocationForSendingToAll == null)
                        {
                            _logger.LogInformation("ProcessIncomingMessageAsync: Sending Event Location for the first time. GuestId={GuestId}", guestId);
                            var guests = new List<Guest> { guest };
                            _logger.LogInformation("ProcessIncomingMessageAsync: Sending Event Location. GuestId={GuestId}", guestId);
                            await whatsappProvider.SendEventLocationAsync(guests, _event);
                            await db.SaveChangesAsync();
                            await transaction.CommitAsync();

                        }
                        else
                        {
                            _logger.LogInformation("ProcessIncomingMessageAsync: Location already sent. GuestId={GuestId}", guestId);
                            await db.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        return;
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    _logger.LogInformation("ProcessIncomingMessageAsync: Successfully processed incoming message for GuestId={GuestId}, MsgId={MsgId}", guestId, parameters.MsgId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ProcessIncomingMessageAsync: Error inside lock. GuestId={GuestId}", guestId);
                    try { await transaction.RollbackAsync(); } catch { }
                    throw;
                }
            
                

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessIncomingMessageAsync: Unhandled error");
            }
        }

        private void UpdateTextStatus(Guest guest, string status)
        {
            switch (status)
            {
                case "read":
                    guest.TextRead = true;
                    break;
                case "delivered":
                    guest.TextDelivered = true;
                    break;
                case "sent":
                    guest.TextSent = true;
                    break;
                case "failed":
                case "undelivered":
                    guest.TextFailed = true;
                    break;
            }
        }

        private void UpdateImgStatus(Guest guest, string status)
        {
            switch (status)
            {
                case "read":
                    guest.ImgRead = true;
                    break;
                case "delivered":
                    guest.ImgDelivered = true;
                    break;
                case "sent":
                    guest.ImgSent = true;
                    break;
                case "failed":
                case "undelivered":
                    guest.ImgFailed = true;
                    break;
            }
        }

        private void UpdateEventLocationStatus(Guest guest, string status)
        {
            switch (status)
            {
                case "read":
                    guest.EventLocationRead = true;
                    break;
                case "delivered":
                    guest.EventLocationDelivered = true;
                    break;
                case "sent":
                    guest.EventLocationSent = true;
                    break;
                case "failed":
                case "undelivered":
                    guest.EventLocationFailed = true;
                    break;
            }
        }

        private void UpdateCongratulationStatus(Guest guest, string status)
        {
            switch (status)
            {
                case "read":
                    guest.ConguratulationMsgRead = true;
                    break;
                case "delivered":
                    guest.ConguratulationMsgDelivered = true;
                    break;
                case "sent":
                    guest.ConguratulationMsgSent = true;
                    break;
                case "failed":
                case "undelivered":
                    guest.ConguratulationMsgFailed = true;
                    break;
            }
        }

        private void UpdateReminderStatus(Guest guest, string status)
        {
            switch (status)
            {
                case "read":
                    guest.ReminderMessageRead = true;
                    break;
                case "delivered":
                    guest.ReminderMessageDelivered = true;
                    break;
                case "sent":
                    guest.ReminderMessageSent = true;
                    break;
                case "failed":
                case "undelivered":
                    guest.ReminderMessageFailed = true;
                    break;
            }
        }

        private async Task SendDuplicateResponse(EventProContext db, IMessagesSendingFactory whatsappProvider,
            Guest guest, List<Guest> guests, Events _event, string btnKeyword,string buttonText)
        {
             _logger.LogInformation("SendDuplicateResponse: Started. btnKeyword={BtnKeyword}, buttonText={ButtonText}", btnKeyword, buttonText);

             if (guest == null)
             {
                 _logger.LogWarning("SendDuplicateResponse: Guest is null");
                 return;
             }

             if (guest.Response == null)
             {
                  _logger.LogWarning("SendDuplicateResponse: Guest.Response is null");
             }

            var guestResponseKeyword = await db.ConfirmationMessageResponsesKeyword
                 .Where(e => e.KeywordValue == guest.Response)
                 .Select(e => e.KeywordKey)
                 .FirstOrDefaultAsync();
            
            _logger.LogInformation("SendDuplicateResponse: guestResponseKeyword determined as {GuestResponseKeyword}", guestResponseKeyword);

            if (guestResponseKeyword != btnKeyword)
            {
                var langCode = await db.ConfirmationMessageResponsesKeyword
                    .Where(e => e.KeywordValue == buttonText)
                    .Select(e => e.LanguageCode).FirstOrDefaultAsync();
                
                 _logger.LogInformation("SendDuplicateResponse: langCode={LangCode}", langCode);

                if (langCode == "en")
                {
                     var templates = whatsappProvider.GetDuplicateAnswerTemplates();
                     if (templates == null) _logger.LogError("SendDuplicateResponse: GetDuplicateAnswerTemplates returned null");
                    await templates.SendEnglishDuplicateAnswer(guests, _event);
                }
                else
                {
                    var templates = whatsappProvider.GetDuplicateAnswerTemplates();
                     if (templates == null) _logger.LogError("SendDuplicateResponse: GetDuplicateAnswerTemplates returned null");
                    await templates.SendArabicDuplicateAnswer(guests, _event);
                }
            }
            return;
        }

        private async Task ProcessConfirmationResponse(EventProContext db, IMessagesSendingFactory whatsappProvider,
            Guest guest, Events _event, string btnKeyword)
        {
            _logger.LogInformation("ProcessConfirmationResponse: Started. BtnKeyword={BtnKeyword}", btnKeyword);
            var updatedGuests = new List<Guest> { guest };

            if (btnKeyword == "confirm_button")
            {
                guest.Response = "Confirm";
                guest.WaresponseTime = DateTime.Now;
                db.Update(guest);

                if (_event.SendInvitation)
                {
                    try
                    {
                        await whatsappProvider.SendCardMessagesAsync(updatedGuests, _event);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error sending card message: {ex.Message}");
                    }
                }
            }
            else if (btnKeyword == "decline_button")
            {
                guest.Response = "Decline";
                guest.WaresponseTime = DateTime.Now;
                db.Update(guest);

                if (_event.SendInvitation)
                {
                    try
                    {
                        await whatsappProvider.SendDeclineMessageAsync(updatedGuests, _event);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error sending decline message: {ex.Message}");
                    }
                }
            }
        }

        private MessageParameters GetMessageParameters(dynamic Request)
        {
            return new MessageParameters
            {
                Status = Request.MessageStatus,
                Id = Request.MessageSid,
                Messages = Request.Body,
                BtnText = Request.ButtonText,
                Recepient = Request.WaId,
                OurNumber = Request.To,
                ButtonPayLoad = Request.ButtonPayload,
                Timestamp = DateTime.UtcNow,
                MsgId = Convert.ToString(Request.MessageSid),
                OriginalMsgId = Request.OriginalRepliedMessageSid
            };
        }

        private class MessageParameters
        {
            public string Status { get; set; }
            public string Id { get; set; }
            public string Messages { get; set; }
            public string BtnText { get; set; }
            public string Recepient { get; set; }
            public string OurNumber { get; set; }
            public string ButtonPayLoad { get; set; }
            public DateTime Timestamp { get; set; }
            public string MsgId { get; set; }
            public string OriginalMsgId { get; set; }
        }
    }
}