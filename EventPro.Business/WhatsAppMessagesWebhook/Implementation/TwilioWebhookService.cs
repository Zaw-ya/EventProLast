using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Implementaiion;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.Business.WhatsAppMessagesWebhook.Interface;
using EventPro.DAL.Models;
using Serilog;

namespace EventPro.Business.WhatsAppMessagesWebhook.Implementation
{
    public class TwilioWebhookService : ITwilioWebhookService
    {
        private readonly IConfiguration _configuration;
        public readonly IWhatsappSendingProviderService _whatsappSendingProviderService;
        private readonly DistributedLockHelper _distributedLockHelper;
        private readonly IDbContextFactory<EventProContext> _dbFactory;

        public TwilioWebhookService(IConfiguration configuration, IWhatsappSendingProviderService whatsappSendingProviderService,
            DistributedLockHelper distributedLockHelper,
            IDbContextFactory<EventProContext> dbFactory)
        {
            _configuration = configuration;
            _whatsappSendingProviderService = whatsappSendingProviderService;
            _distributedLockHelper = distributedLockHelper;
            _dbFactory = dbFactory;
        }

        public async Task ProcessStatusAsync(dynamic obj)
        {
            try
            {
                MessageParameters parameters = GetMessageParameters(obj);

                if (string.IsNullOrEmpty(parameters.Status) || !string.IsNullOrEmpty(parameters.Messages))
                    return;

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
                    return;

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
                            return;

                        bool updated = false;

                        // Check each message type and update accordingly
                        if (guest.MessageId == parameters.MsgId)
                        {
                            UpdateTextStatus(guest, parameters.Status);
                            updated = true;
                        }
                        else if (guest.ImgSentMsgId == parameters.MsgId)
                        {
                            UpdateImgStatus(guest, parameters.Status);
                            updated = true;
                        }
                        else if (guest.waMessageEventLocationForSendingToAll == parameters.MsgId)
                        {
                            UpdateEventLocationStatus(guest, parameters.Status);
                            updated = true;
                        }
                        else if (guest.ConguratulationMsgId == parameters.MsgId)
                        {
                            UpdateCongratulationStatus(guest, parameters.Status);
                            updated = true;
                        }
                        else if (guest.ReminderMessageId == parameters.MsgId)
                        {
                            UpdateReminderStatus(guest, parameters.Status);
                            updated = true;
                        }

                        if (updated)
                        {
                            await db.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                        }
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error($"Error in ProcessStatusAsync: {ex.Message}");
            }
        }

        public async Task ProcessIncomingMessageAsync(dynamic obj)
        {
            try
            {
                MessageParameters parameters = GetMessageParameters(obj);

                if (string.IsNullOrEmpty(parameters.BtnText))
                    return;

                int guestId;
                bool isValidKeyword;

                await using (var lookupDb = await _dbFactory.CreateDbContextAsync())
                {
                    // Check if this is a valid button keyword
                    isValidKeyword = await lookupDb.ConfirmationMessageResponsesKeyword
                        .AnyAsync(e => e.KeywordValue == parameters.BtnText);

                    if (!isValidKeyword)
                        return;

                    // Find the guest (outside lock for performance)
                    guestId = await lookupDb.Guest
                        .Where(p => p.GuestArchieved == false &&
                            !string.IsNullOrEmpty(parameters.ButtonPayLoad) &&
                            ((!string.IsNullOrEmpty(parameters.MsgId) && p.MessageId == parameters.MsgId) ||
                             p.YesButtonId == parameters.ButtonPayLoad ||
                             p.NoButtonId == parameters.ButtonPayLoad ||
                             p.EventLocationButtonId == parameters.ButtonPayLoad))
                        .Select(p => p.GuestId)
                        .FirstOrDefaultAsync();
                }

                if (guestId == default)
                    return;

                var whatsappProvider = _whatsappSendingProviderService.SelectTwilioSendingProvider();

                // Lock and process with fresh data
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
                            await transaction.RollbackAsync();
                            return;
                        }

                        var _event = await db.Events
                            .Where(p => p.Id == guest.EventId)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();

                        if (_event == null)
                        {
                            await transaction.RollbackAsync();
                            return;
                        }

                        guest.TextRead = true;

                        var btnKeyword = await db.ConfirmationMessageResponsesKeyword
                            .Where(e => e.KeywordValue == parameters.BtnText)
                            .Select(e => e.KeywordKey)
                            .FirstOrDefaultAsync();

                        if (btnKeyword == "confirm_button" || btnKeyword == "decline_button")
                        {
                            // Check if already processed (inside the lock with fresh data)
                            bool isFirstResponse = guest.Response == "Message Processed Successfully";

                            if (isFirstResponse)
                            {
                                await ProcessConfirmationResponse(db, whatsappProvider, guest, _event, btnKeyword);
                                await db.SaveChangesAsync();
                                await transaction.CommitAsync();
                            }
                            else
                            {
                                await transaction.CommitAsync();
                                // Send duplicate response (no DB changes needed)
                                var guests = new List<Guest> { guest };
                                await SendDuplicateResponse(db, whatsappProvider, guest, guests, _event, btnKeyword, parameters.BtnText);
                            }
                            return;
                        }
                        else if (btnKeyword == "eventlocation_button")
                        {
                            // Check if location already sent (inside the lock with fresh data)
                            if (guest.waMessageEventLocationForSendingToAll == null)
                            {
                                var guests = new List<Guest> { guest };
                                await whatsappProvider.SendEventLocationAsync(guests, _event);
                                await db.SaveChangesAsync();
                                await transaction.CommitAsync();
                            }
                            else
                            {
                                await transaction.CommitAsync();
                            }
                            return;
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error($"Error in ProcessIncomingMessageAsync: {ex.Message}");
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
            var guestResponseKeyword = await db.ConfirmationMessageResponsesKeyword
                 .Where(e => e.KeywordValue == guest.Response)
                 .Select(e => e.KeywordKey)
                 .FirstOrDefaultAsync();

            if (guestResponseKeyword != btnKeyword)
            {
                if (await db.ConfirmationMessageResponsesKeyword
                    .Where(e => e.KeywordValue == buttonText)
                    .Select(e => e.LanguageCode).FirstOrDefaultAsync() == "en")
                {
                    await whatsappProvider.GetDuplicateAnswerTemplates()
                        .SendEnglishDuplicateAnswer(guests, _event);
                }
                else
                {
                    await whatsappProvider.GetDuplicateAnswerTemplates()
                        .SendArabicDuplicateAnswer(guests, _event);
                }
            }
            return;
        }

        private async Task ProcessConfirmationResponse(EventProContext db, IMessagesSendingFactory whatsappProvider,
            Guest guest, Events _event, string btnKeyword)
        {
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
                        Log.Error($"Error sending card message: {ex.Message}");
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
                        Log.Error($"Error sending decline message: {ex.Message}");
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