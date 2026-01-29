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
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<TwillioCallbackController> _logger;
        public TwillioCallbackController(ITwilioWebhookService twilioWebhookService,
            IWebHookBulkMessagingQueueProducerService webhookBulkMessagingQueueProducerService,
            IWebHookSingleMessagingQueueProducerService webHookSingleMessagingQueueProducerService,
            IMemoryCacheStoreService memoryCacheStoreService,
            ILogger<TwillioCallbackController> logger)
        {
            _twilioWebhookService = twilioWebhookService;
            _webhookBulkMessagingQueueProducerService = webhookBulkMessagingQueueProducerService;
            _webHookSingleMessagingQueueProducerService = webHookSingleMessagingQueueProducerService;
            _memoryCacheStoreService = memoryCacheStoreService;
            _logger = logger;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("~/api/TwillioCallbackController/Post")]
        public IActionResult UpdateMessages()
        {
            _logger.LogInformation("Twilio webhook callback received from IP: {RemoteIp}", HttpContext.Connection.RemoteIpAddress);

            var form = Request.Form;

            _logger.LogDebug("Twilio form data: MessageSid={MessageSid}, SmsStatus={SmsStatus}, MessageStatus={MessageStatus}, From={From}, To={To}",
            form["MessageSid"], form["SmsStatus"], form["MessageStatus"], form["From"], form["To"]);


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

            if (string.IsNullOrEmpty(message.MessageSid))
            {
                _logger.LogWarning("Twilio callback missing MessageSid → ignoring");
                return BadRequest("Missing MessageSid");
            }

            try
            {
                var policy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                // Ghrarabwy :
                // NOTE:
                // Twilio webhooks are not guaranteed to be delivered exactly once.
                // Duplicate callbacks can occur and must be handled idempotently.

                // There is 2 scenarios here
                // 1) This MessageSid already exists in cache
                //    → duplicate Twilio callback (same message sent more than once)
                //    → (possible reasone for reciving dublicate twilio callbacks i will explain it) -> send to Bulk queue for processing
                // 2) This message is a reply to an existing message
                //    (OriginalRepliedMessageSid exists in cache)
                //    → reply to a previous/bulk message
                //
                // Otherwise, route it to SINGLE queue (new, standalone message)

                /// Possible reasons for receiving duplicate Twilio webhook callbacks:
                // 1) The webhook endpoint did not return HTTP 2xx fast enough
                //    → Twilio retries the same callback.
                //
                // 2) The server returned a 4xx or 5xx response
                //    → Twilio assumes failure and sends the callback again.
                //
                // 3) Temporary network issues between Twilio and the server
                //    → Callback delivery is retried.

                if ((!string.IsNullOrEmpty(message.MessageSid) &&
                    _memoryCacheStoreService.IsExist(message.MessageSid)) || // dublicate twilio callback

                   (!string.IsNullOrEmpty(message.OriginalRepliedMessageSid) && // reply to a previous/bulk message
                   _memoryCacheStoreService.IsExist(message.OriginalRepliedMessageSid)))
                {
                    policy.ExecuteAsync(async () =>
                    {
                        await _webhookBulkMessagingQueueProducerService.SendingMessageAsync(message);
                        _logger.LogInformation("Published to Bulk webhook queue → MessageSid={MessageSid}", message.MessageSid);
                    });
                }
                else
                {
                    policy.ExecuteAsync(async () =>
                    {
                        await _webHookSingleMessagingQueueProducerService.SendingMessageAsync(message);
                        _logger.LogInformation("Published to Single webhook queue → MessageSid={MessageSid}", message.MessageSid);
                    });
                }
                return Ok(new { message = "Ack Received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish Twilio webhook to RabbitMQ → MessageSid={MessageSid}", message.MessageSid);
                return StatusCode(500, "Internal error processing webhook");
            }
        }
    }
}
