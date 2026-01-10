using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Dto;
using EventPro.DAL.Models;
using Twilio.Base;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Api.V2010.Account.Message;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioMessagesSync : TwilioMessagingConfiguration, IMessagesSync
    {
        private readonly EventProContext db;
        public TwilioMessagesSync(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService) : base(configuration,
                memoryCacheStoreService)
        {
            db = new EventProContext(configuration);
        }

        public async Task UpdateMessagesStatusAsync(List<Guest> guests, Events events)
        {
            await SetTwilioAccountConfigurationAsync(events.choosenSendingWhatsappProfile);
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                if (!string.IsNullOrEmpty(guest.MessageId))
                {
                    await FetchAndProcessMessage(guest.MessageId, guest, "Text");
                }

                if (!string.IsNullOrEmpty(guest.waMessageEventLocationForSendingToAll))
                {
                    await FetchAndProcessMessage(guest.waMessageEventLocationForSendingToAll, guest, "EventLocation");
                }

                if (!string.IsNullOrEmpty(guest.ImgSentMsgId))
                {
                    await FetchAndProcessMessage(guest.ImgSentMsgId, guest, "QR");
                }

                if (!string.IsNullOrEmpty(guest.ReminderMessageId))
                {
                    await FetchAndProcessMessage(guest.ReminderMessageId, guest, "Reminder");
                }

                if (!string.IsNullOrEmpty(guest.ConguratulationMsgId))
                {
                    await FetchAndProcessMessage(guest.ConguratulationMsgId, guest, "Conguratulation");
                }
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        private async Task updateDataBaseAndDisposeCache(List<Guest> guests, Events events)
        {
            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();
            _memoryCacheStoreService.delete(events.Id.ToString());
        }

        private async Task FetchAndProcessMessage(string messageId, Guest guest, string messageType)
        {
            try
            {
                var message = await MessageResource.FetchAsync(pathSid: messageId);
                guest.WhatsappStatus = message.Status.ToString();

                switch (messageType)
                {
                    case "Text":
                        UpdateMessageStatus(guest, message.Status.ToString(), "Text");
                        break;
                    case "EventLocation":
                        UpdateMessageStatus(guest, message.Status.ToString(), "EventLocation");
                        break;
                    case "QR":
                        UpdateMessageStatus(guest, message.Status.ToString(), "QR");
                        break;
                    case "Reminder":
                        UpdateMessageStatus(guest, message.Status.ToString(), "Reminder");
                        break;
                    case "Conguratulation":
                        UpdateMessageStatus(guest, message.Status.ToString(), "Conguratulation");
                        break;
                }
            }
            catch { }
        }

        private void UpdateMessageStatus(Guest guest, string status, string messageType)
        {
            switch (status)
            {
                case "read":
                    SetMessageStatus(guest, messageType, "Read", true);
                    break;
                case "delivered":
                    SetMessageStatus(guest, messageType, "Delivered", true);
                    break;
                case "sent":
                    SetMessageStatus(guest, messageType, "Sent", true);
                    break;
                case "failed":
                    SetMessageStatus(guest, messageType, "Failed", true);
                    break;
                case "undelivered":
                    SetMessageStatus(guest, messageType, "Failed", true);
                    break;
            }
        }

        private void SetMessageStatus(Guest guest, string messageType, string statusType, bool status)
        {
            switch (messageType)
            {
                case "Text":
                    if (statusType == "Read") guest.TextRead = status;
                    if (statusType == "Delivered") guest.TextDelivered = status;
                    if (statusType == "Sent") guest.TextSent = status;
                    if (statusType == "Failed") guest.TextFailed = status;
                    break;
                case "EventLocation":
                    if (statusType == "Read") guest.EventLocationRead = status;
                    if (statusType == "Delivered") guest.EventLocationDelivered = status;
                    if (statusType == "Sent") guest.EventLocationSent = status;
                    if (statusType == "Failed") guest.EventLocationFailed = status;
                    break;
                case "QR":
                    if (statusType == "Read") guest.ImgRead = status;
                    if (statusType == "Delivered") guest.ImgDelivered = status;
                    if (statusType == "Sent") guest.ImgSent = status;
                    if (statusType == "Failed") guest.ImgFailed = status;
                    break;
                case "Reminder":
                    if (statusType == "Read") guest.ReminderMessageRead = status;
                    if (statusType == "Delivered") guest.ReminderMessageDelivered = status;
                    if (statusType == "Sent") guest.ReminderMessageSent = status;
                    if (statusType == "Failed") guest.ReminderMessageFailed = status;
                    break;
                case "Conguratulation":
                    if (statusType == "Read") guest.ConguratulationMsgRead = status;
                    if (statusType == "Delivered") guest.ConguratulationMsgDelivered = status;
                    if (statusType == "Sent") guest.ConguratulationMsgSent = status;
                    if (statusType == "Failed") guest.ConguratulationMsgFailed = status;
                    break;
            }
        }

        public async Task<List<MessageLog>> GetGuestMessagesAsync(string number, string profileName)
        {
            await SetTwilioAccountConfigurationAsync(profileName);

            var messageLogs = new List<MessageLog>();


            await Task.Run(async () =>
            {
                var messagesSent = await MessageResource.ReadAsync(
                                   to: $"whatsapp:+{number}",
                                   limit: 100
                                     );

                await GetMessagesMediaAndUpdateAsync(messageLogs, messagesSent,true);
            });


            await Task.Run(async () =>
             {
                 var messagesReceived = await MessageResource.ReadAsync(
                                       from: $"whatsapp:+{number}",
                                       limit: 100
                                        );

                 await GetMessagesMediaAndUpdateAsync(messageLogs, messagesReceived,false);
             });


            return messageLogs.OrderBy(e => e.DateSent).ToList();

        }

        private async Task GetMessagesMediaAndUpdateAsync(List<MessageLog> messageLogs, ResourceSet<MessageResource> messagesSent,bool IsEventProMessage)
        {
            await Parallel.ForEachAsync(messagesSent, parallelOptions, async (message, CancellationToken) =>
            {
                var mediaFile = new MediaFile();

                var log = new MessageLog()
                {
                    Body = message.Body,
                    ToPhoneNumber = message.To,
                    DateSent = message.DateSent,
                    EventProMessage = IsEventProMessage,
                    status = message.Status.ToString(),
                    HasMedia = false,
                    MediaUrl = string.Empty,
                    MediaExtention = string.Empty
                };

                if (Int32.Parse(message.NumMedia) > 0)
                {
                    mediaFile = await getMediaUrl(message.Sid);

                    log.HasMedia = true;
                    log.MediaUrl = mediaFile.Url;
                    log.MediaExtention = mediaFile.Extention;
                }

                messageLogs.Add(log);
            });
        }

        private async Task<MediaFile> getMediaUrl(string sid)
        {
            var mediaFile = new MediaFile();
            try
            {
                var mediaFiles = await MediaResource.ReadAsync(
                                            pathMessageSid: sid
                                              );

                mediaFile.Url = mediaFiles.FirstOrDefault().Uri.ToString().Replace(".json", "").Replace("\"", "");
                mediaFile.Url = "https://api.twilio.com" + mediaFile.Url;
                var Extention = mediaFiles.FirstOrDefault().ContentType.ToString();
                if (Extention.Contains("image"))
                {
                    mediaFile.Extention = "image";
                }
                if (Extention.Contains("video"))
                {
                    mediaFile.Extention = "video";
                }
                if (Extention.Contains("audio"))
                {
                    mediaFile.Extention = "audio";
                }
                if (Extention.Contains("text"))
                {
                    mediaFile.Extention = "text";
                }
                if (Extention.Contains("application"))
                {
                    mediaFile.Extention = "application";
                }

            }
            catch (Exception ex)
            {
                mediaFile.Url = "https://fakeimg.pl/600x400";
                mediaFile.Extention = "image";
            }

            return mediaFile;
        }
    }
}
