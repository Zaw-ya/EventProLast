using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Extensions;
using EventPro.DAL.Models;
using Newtonsoft.Json.Linq;
using SpecFlow.Internal.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Rest.Content.V1;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioTemplateSync : TwilioMessagingConfiguration, ITemplateSync
    {

        private readonly EventProContext db;
        public TwilioTemplateSync(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService) : base(configuration,
                memoryCacheStoreService)
        {
            db = new EventProContext(configuration);
        }
        public async Task<string> GetCustomTemplateWithVariablesAsync(Events events, int typeId)
        {

            string template = string.Empty;
            if (typeId == 1)
            {
                template = events.CustomInvitationMessageTemplateName ;
            }
            else if (typeId == 2)
            {
                template = events.CustomCardInvitationTemplateName ;
            }
            else if (typeId == 3)
            {
                template = events.ReminderTempId ;
            }
            else if (typeId == 4)
            {
                template = events.ThanksTempId ;
            }

            await SetTwilioAccountConfigurationAsync(events.choosenSendingWhatsappProfile);
            var content = await ContentResource.FetchAsync(pathSid: template);
            var obj = JObject.Parse(content.Types.ToString());

            if (obj["twilio/text"] != null)
            {
                // Simple text
                return obj["twilio/text"]?["body"]?.ToString();
            }
            else if (obj["twilio/media"] != null)
            {
                var media = obj["twilio/media"]?["media"]?[0]?.ToString();
                string mediaVar = media?.Split('/').Last();
                string body = obj["twilio/media"]?["body"]?.ToString();
                return $"Media Content {mediaVar}\n{body}";
            }
            else if (obj["twilio/card"] != null)
            {
                var media = obj["twilio/card"]?["media"]?[0]?.ToString();
                string mediaVar = media?.Split('/').Last();
                string body = obj["twilio/card"]?["body"]?.ToString();

                if (string.IsNullOrWhiteSpace(body))
                {
                    body = obj["twilio/card"]?["title"]?.ToString()
                        ?? obj["twilio/card"]?["subtitle"]?.ToString();
                }
                return $"Media Content {mediaVar}\n{body}";
            }
            else if (obj["twilio/call-to-action"] != null)
            {
                string body = obj["twilio/call-to-action"]?["body"]?.ToString();

                return body;
            }
            else if (obj["twilio/quick-reply"] != null)
            {
                string body = obj["twilio/quick-reply"]?["body"]?.ToString();
                return body;
            }
            else if (obj["whatsapp/card"] != null)
            {
                var media = obj["whatsapp/card"]?["media"]?[0]?.ToString();
                string mediaVar = media?.Split('/').Last();
                string body = obj["whatsapp/card"]?["body"]?.ToString();

                return $"Media Content {mediaVar}\n{body}";
            }
            else
            {
                return "Unknown content type";
            }


        }
    }
}
