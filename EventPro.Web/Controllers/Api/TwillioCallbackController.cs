using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.RabbitMQMessaging.Interface;
using EventPro.Business.WhatsAppMessagesWebhook.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Filters;
using Polly;
using Serilog;
using System;

namespace EventPro.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("OneAtATimePolicy")]
    [ServiceFilter(typeof(TwilioRoutingRuleForVms))]
    public class TwillioCallbackController : ControllerBase
    {
        private readonly ITwilioWebhookService _twilioWebhookService;
        private readonly IWebHookBulkMessagingQueueProducerService _webhookBulkMessagingQueueProducerService;
        private readonly IWebHookSingleMessagingQueueProducerService _webHookSingleMessagingQueueProducerService;
        private readonly IMemoryCacheStoreService _memoryCacheStoreService;
        public TwillioCallbackController(ITwilioWebhookService twilioWebhookService,
            IWebHookBulkMessagingQueueProducerService webhookBulkMessagingQueueProducerService,
            IWebHookSingleMessagingQueueProducerService webHookSingleMessagingQueueProducerService,
            IMemoryCacheStoreService memoryCacheStoreService)
        {
            _twilioWebhookService = twilioWebhookService;
            _webhookBulkMessagingQueueProducerService = webhookBulkMessagingQueueProducerService;
            _webHookSingleMessagingQueueProducerService = webHookSingleMessagingQueueProducerService;
            _memoryCacheStoreService = memoryCacheStoreService;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("~/api/TwillioCallbackController/Post")]
        public IActionResult UpdateMessages()
        {
            MessageMetadataRequest message = new MessageMetadataRequest
            {
                ChannelPrefix = Request.Form["ChannelPrefix"],
                MessagingServiceSid = Request.Form["MessagingServiceSid"],
                ApiVersion = Request.Form["ApiVersion"],
                MessageStatus = Request.Form["MessageStatus"],
                SmsSid = Request.Form["SmsSid"],
                SmsStatus = Request.Form["SmsStatus"],
                ChannelInstallSid = Request.Form["ChannelInstallSid"],
                To = Request.Form["To"],
                From = Request.Form["From"],
                MessageSid = Request.Form["MessageSid"],
                AccountSid = Request.Form["AccountSid"],
                ChannelToAddress = Request.Form["ChannelToAddress"],
                OriginalRepliedMessageSid = Request.Form["OriginalRepliedMessageSid"],
                ButtonPayload = Request.Form["ButtonPayload"],
                ButtonText = Request.Form["ButtonText"],
                OriginalRepliedMessageSender = Request.Form["OriginalRepliedMessageSender"],
                SmsMessageSid = Request.Form["SmsMessageSid"],
                NumMedia = Request.Form["NumMedia"],
                ProfileName = Request.Form["ProfileName"],
                WaId = Request.Form["WaId"],
                MessageType = Request.Form["MessageType"],
                Body = Request.Form["Body"],
                NumSegments = Request.Form["NumSegments"],
                ReferralNumMedia = Request.Form["ReferralNumMedia"],
            };

            try
            {
                var policy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                if ((!string.IsNullOrEmpty(message.MessageSid) &&
                    _memoryCacheStoreService.IsExist(message.MessageSid)) ||
                   (!string.IsNullOrEmpty(message.OriginalRepliedMessageSid) &&
                   _memoryCacheStoreService.IsExist(message.OriginalRepliedMessageSid)))
                {
                    policy.ExecuteAsync(async () =>
                    {
                        await _webhookBulkMessagingQueueProducerService.SendingMessageAsync(message);
                    });
                }
                else
                {
                    policy.ExecuteAsync(async () =>
                    {
                        await _webHookSingleMessagingQueueProducerService.SendingMessageAsync(message);
                    });
                }

            }
            catch (Exception ex)
            {
                Log.Error($"rabbitmq producer error {ex}");
                return BadRequest();
            }
            return Ok(new { message = "Ack Received" });
        }

    }
}
