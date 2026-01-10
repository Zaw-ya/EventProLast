using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Common;
using EventPro.DAL.Models;
using EventPro.Services.WatiService.Interface;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatiCallbackController : ControllerBase
    {

        private readonly EventProContext db;
        private readonly IConfiguration _configuration;
        private readonly IWatiService _watiService;

        public WatiCallbackController(IConfiguration configuration, IWatiService watiService)
        {
            db = new EventProContext(configuration);
            _configuration = configuration;
            _watiService = watiService;
        }


        private PinacleMediaBody SendCard2(Guest guest, Events evt)
        {

            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            string _fromNo = _configuration.GetSection("PinacleSettings").GetSection("FromNo").Value;
            string _templateid = _configuration.GetSection("PinacleSettings").GetSection("Template_Image").Value;

            PinacleMediaMessage msg = new PinacleMediaMessage
            {
                templateid = _templateid,
                url = imagePath + @"/" + evt.Id + @"/" + "E00000" + evt.Id + "_" + guest.GuestId + @"_" + guest.NoOfMembers + ".png"
            };
            PinacleMediaBody ikart = new PinacleMediaBody
            {
                from = _fromNo,
                to = guest.SecondaryContactNo + "" + guest.PrimaryContactNo,
                message = msg,
                type = "template"
            };
            return ikart;
        }

        private PinacleMediaBody SendOurService(string guestNo)
        {
            string _fromNo = _configuration.GetSection("PinacleSettings").GetSection("FromNo").Value;
            string _templateid = _configuration.GetSection("PinacleSettings").GetSection("Template_EventProServies").Value;
            PinacleMediaMessage msg = new PinacleMediaMessage
            {
                templateid = _templateid,
                url = "https://www.EventPro.me/upload/EventProBackground.jpg"
            };
            PinacleMediaBody ikart = new PinacleMediaBody
            {
                from = _fromNo,
                to = guestNo,
                message = msg,
                type = "template"
            };
            return ikart;
        }


        private PinacleBody SendInvalidResponse(Guest guest, Events evt)
        {
            string[] bodyValue = { };

            string _fromNo = _configuration.GetSection("PinacleSettings").GetSection("FromNo").Value;
            string _templateid = _configuration.GetSection("PinacleSettings").GetSection("Template_Duplicate").Value;
            PinacleButtons button = new PinacleButtons
            {
                index = 0,
                type = "quick_reply"
            };
            List<PinacleButtons> _buttons = new List<PinacleButtons>
            {
                button
            };

            PinacleMessage msg = new PinacleMessage
            {
                templateid = _templateid,
                placeholders = bodyValue,
                buttons = _buttons
            };
            PinacleBody ikart = new PinacleBody
            {
                from = _fromNo,
                to = guest.SecondaryContactNo + "" + guest.PrimaryContactNo,
                message = msg,
                type = "template",
                gotomodule = 0
            };
            return ikart;
        }

        private PinacleBody SendLocation(Guest guest, Events evt)
        {
            string[] gmapCode = { "https://maps.app.goo.gl/" + evt.GmapCode };

            string _fromNo = _configuration.GetSection("PinacleSettings").GetSection("FromNo").Value;
            string _templateid = _configuration.GetSection("PinacleSettings").GetSection("Template_Location").Value;
            PinacleButtons button = new PinacleButtons
            {
                index = 0,
                type = "quick_reply"
            };
            List<PinacleButtons> _buttons = new List<PinacleButtons>();
            _buttons.Add(button);
            PinacleMessage msg = new PinacleMessage
            {
                templateid = _templateid,
                placeholders = gmapCode,
                buttons = _buttons
            };
            PinacleBody ikart = new PinacleBody
            {
                from = _fromNo,
                to = guest.SecondaryContactNo + "" + guest.PrimaryContactNo,
                message = msg,
                type = "template",
                gotomodule = 0
            };
            return ikart;
        }

        private PinacleMediaBody SendLocation2(Guest guest, Events evt)
        {
            string[] gmapCode = { "https://maps.app.goo.gl/" + evt.GmapCode };

            string _fromNo = _configuration.GetSection("PinacleSettings").GetSection("FromNo").Value;
            string _templateid = _configuration.GetSection("PinacleSettings").GetSection("Template_Location2").Value;
            string location = "https://maps.app.goo.gl/" + evt.GmapCode;
            PinacleMediaMessage msg = new PinacleMediaMessage
            {
                templateid = _templateid,
                url = location,
                placeholders = gmapCode
            };
            PinacleMediaBody ikart = new PinacleMediaBody
            {
                from = _fromNo,
                to = guest.SecondaryContactNo + "" + guest.PrimaryContactNo,
                message = msg,
                type = "template"
            };
            return ikart;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/WatiCallbackController/post")]
        [Route("~/api/WatiCallbackController")]
        public async Task<IActionResult> Post([FromBody] dynamic data)
        {
            string body = string.Empty;
            try
            {
                bool routeRequest = Convert.ToBoolean(_configuration.GetSection("InterkatSettings").GetSection("RouteRequest").Value);
                string auth = _configuration.GetSection("InterkatSettings").GetSection("SecurityKey").Value;
                string syncPath = _configuration.GetSection("PinacleSettings").GetSection("LogPath").Value;
                string Trace = _configuration.GetSection("PinacleSettings").GetSection("Trace").Value;
                string _yesText_Ar = _configuration.GetSection("PinacleSettings").GetSection("TextYes_Ar").Value;
                string _noText_Ar = _configuration.GetSection("PinacleSettings").GetSection("TextNo_Ar").Value;
                string _locationText_Ar = _configuration.GetSection("PinacleSettings").GetSection("TextLocation_Ar").Value;
                string _yesText_Eng = _configuration.GetSection("PinacleSettings").GetSection("TextYes_Eng").Value;
                string _noText_Eng = _configuration.GetSection("PinacleSettings").GetSection("TextNo_Eng").Value;
                string _locationText_Eng = _configuration.GetSection("PinacleSettings").GetSection("TextLocation_Eng").Value;
                bool foundText = false;
                bool foundImg = false;

                data = JsonConvert.DeserializeObject<dynamic>(data.ToString());
                var status = data.statusString;
                var id = data.localMessageId;
                var timestamp = data.timestamp;
                string msgId = Convert.ToString(id);
                string whatsappMessageId = Convert.ToString(data.whatsappMessageId);
                var replyContextId = data.replyContextId;

                if (replyContextId == null)
                {

                    if (db.Guest.Where(p => p.MessageId == msgId).Any())
                    {
                        foundText = true;
                        var guest = db.Guest.Where(p => p.MessageId == msgId).FirstOrDefault();
                        guest.whatsappMessageId = whatsappMessageId.ToString();
                        switch (Convert.ToString(status))
                        {
                            case "Read":
                                guest.TextRead = true;
                                break;
                            case "Delivered":
                                guest.TextDelivered = true;
                                break;
                            case "SENT":
                                guest.TextSent = true;
                                break;
                        }
                        db.SaveChanges();
                    }

                    if (db.Guest.Where(p => p.ImgSentMsgId == msgId).Any())
                    {
                        foundImg = true;
                        var guest = db.Guest.Where(p => p.ImgSentMsgId == msgId).FirstOrDefault();
                        guest.whatsappMessageImgId = whatsappMessageId;
                        switch (Convert.ToString(status))
                        {
                            case "Read":
                                guest.ImgRead = true;
                                break;
                            case "Delivered":
                                guest.ImgDelivered = true;
                                break;
                            case "SENT":
                                guest.ImgSent = true;
                                break;
                        }
                        db.SaveChanges();
                    }

                    if (db.Guest.Where(p => p.waMessageEventLocationForSendingToAll == msgId).Any())
                    {

                        var guest = db.Guest.Where(p => p.waMessageEventLocationForSendingToAll == msgId).FirstOrDefault();
                        guest.whatsappWatiEventLocationId = whatsappMessageId;
                        switch (Convert.ToString(status))
                        {
                            case "Read":
                                guest.EventLocationRead = true;
                                break;
                            case "Delivered":
                                guest.EventLocationDelivered = true;
                                break;
                            case "SENT":
                                guest.EventLocationSent = true;
                                break;
                        }
                        db.SaveChanges();
                    }

                    if (db.Guest.Where(p => p.ConguratulationMsgId == msgId).Any())
                    {

                        var guest = db.Guest.Where(p => p.ConguratulationMsgId == msgId).FirstOrDefault();
                        guest.WatiConguratulationMsgId = whatsappMessageId;
                        switch (Convert.ToString(status))
                        {
                            case "Read":
                                guest.ConguratulationMsgRead = true;
                                break;
                            case "Delivered":
                                guest.ConguratulationMsgDelivered = true;
                                break;
                            case "SENT":
                                guest.ConguratulationMsgSent = true;
                                break;
                        }
                        db.SaveChanges();
                    }

                    if (db.Guest.Where(p => p.ReminderMessageId == msgId).Any())
                    {

                        var guest = db.Guest.Where(p => p.ReminderMessageId == msgId).FirstOrDefault();
                        switch (Convert.ToString(status))
                        {
                            case "Read":
                                guest.ReminderMessageRead = true;
                                break;
                            case "Delivered":
                                guest.ReminderMessageDelivered = true;
                                break;
                            case "SENT":
                                guest.ReminderMessageSent = true;
                                break;
                        }
                        db.SaveChanges();
                    }


                }
                else
                {
                    //else if (jbody.entry[0].changes[0].value?.messages[0]?.text != null)
                    //    {
                    //        var messages = jbody.entry[0].changes[0].value.messages[0];
                    //        var recepient = messages.from;

                    //        string msg = string.Empty;
                    //        string guestNo = recepient.ToString();

                    //        var guest = await db.GuestsDeliveredourServiceMessage.FirstOrDefaultAsync(p => p.PhoneNumber == guestNo);
                    //        if (guest == null)
                    //        {
                    //            Interkartmsg.Scheduled(SendOurService(guestNo), auth, ref msg);
                    //            var guestsDeliveredourServiceMessage = new GuestsDeliveredourServiceMessage()
                    //            {
                    //                PhoneNumber = guestNo,
                    //                AddedDate = DateTime.UtcNow.ToShortDateString(),
                    //            };
                    //            await db.GuestsDeliveredourServiceMessage.AddAsync(guestsDeliveredourServiceMessage);
                    //            await db.SaveChangesAsync();
                    //        }

                    //    }

                    var messages = data.text;
                    var btnText = data.text;
                    id = data.replyContextId;
                    var recepient = data.waId;
                    timestamp = data.timestamp;
                    string response = Convert.ToString(btnText);

                    if (btnText == _locationText_Ar ||
                        Convert.ToString(btnText) == _locationText_Eng ||
                        response.ToLower().Equals(_yesText_Ar) ||
                        response.Equals(_yesText_Eng) ||
                        response.Equals("???? ??????") ||
                            response.ToLower().Equals(_noText_Ar) ||
                        response.Equals(_noText_Eng) ||
                        response.Equals("???????? ?? ??????"))
                    {


                        WhatsappResponseLogs logs = new WhatsappResponseLogs
                        {
                            Wakey = id,
                            Type = "response",
                            RecepientNo = recepient,
                            Response = btnText,
                            CreatedOn = System.DateTime.Now,
                            Ticks = Convert.ToInt64(timestamp)
                        };
                        db.WhatsappResponseLogs.Add(logs);
                        msgId = Convert.ToString(id);

                        if (db.Guest.Where(p => p.whatsappMessageId == msgId).Any())
                        {
                            foundText = true;

                            var guest = db.Guest.Where(p => p.whatsappMessageId == msgId).FirstOrDefault();
                            Events _event = db.Events.Where(p => p.Id == guest.EventId).FirstOrDefault();
                            guest.TextRead = true;
                            {
                                if (guest.whatsappMessageEventLocationId == null)
                                {
                                    if (Convert.ToString(btnText) == _locationText_Ar)
                                    {
                                        string msg = string.Empty;


                                        guest.waMessageEventLocationForSendingToAll = await _watiService.SendEventLocationTemplate(guest, _event);

                                    }
                                    if (Convert.ToString(btnText) == _locationText_Eng)
                                    {
                                        string msg = string.Empty;

                                        guest.waMessageEventLocationForSendingToAll = await _watiService.SendEnglishEventLocationTemplate(guest, _event);


                                    }

                                    guest.whatsappMessageEventLocationId = guest.waMessageEventLocationForSendingToAll;
                                }


                                if (btnText != _locationText_Ar && Convert.ToString(btnText) != _locationText_Eng)
                                {
                                    if (guest.Response == "Message Processed Successfully")
                                    {
                                        guest.Response = btnText;
                                        guest.WaresponseTime = DateTime.Now;
                                        string resp = Convert.ToString(btnText);
                                        if ((resp.ToLower().Equals(_yesText_Ar) || resp.Equals("???? ??????")) &&
                                                  guest.Event.SendInvitation)
                                        {
                                            string msg = string.Empty;

                                            if (_event.CardInvitationTemplateType == "Default")
                                            {
                                                guest.ImgSentMsgId = await _watiService.SendCardInvitaionTemplate(guest, _event);

                                            }
                                            else if (_event.CardInvitationTemplateType == "Custom with client name")
                                            {
                                                guest.ImgSentMsgId = await _watiService.SendCustomCardWithClientNameInvitaionTemplate(guest, _event);
                                            }
                                            else
                                            {
                                                guest.ImgSentMsgId = await _watiService.SendCardInvitaionTemplate(guest, _event);
                                            }

                                            guest.ImgSenOn = DateTime.Now.ToString();
                                            resp = _yesText_Ar;
                                        }
                                        if (resp.Equals(_yesText_Eng) &&
                                                 guest.Event.SendInvitation)
                                        {
                                            string msg = string.Empty;

                                            if (_event.CardInvitationTemplateType == "Default")
                                            {
                                                guest.ImgSentMsgId = await _watiService.SendEnglishCardInvitaionTemplate(guest, _event);

                                            }
                                            else if (_event.CardInvitationTemplateType == "Custom with client name")
                                            {
                                                guest.ImgSentMsgId = await _watiService.SendEnglishCustomCardWithClientNameInvitaionTemplate(guest, _event);
                                            }
                                            else
                                            {
                                                guest.ImgSentMsgId = await _watiService.SendEnglishCardInvitaionTemplate(guest, _event);
                                            }

                                            guest.ImgSenOn = DateTime.Now.ToString();
                                            resp = _yesText_Ar;
                                        }
                                    }
                                    else
                                    {

                                        string msg = "";
                                        string resp = Convert.ToString(btnText);

                                        if (guest.Response != resp)
                                        {
                                            if ((resp.Equals(_yesText_Eng) || resp.Equals(_noText_Eng)) &&
                                                 guest.Event.SendInvitation)
                                            {
                                                await _watiService.SendEnglishDuplicateAnswerMessageTemplate(guest, _event);
                                            }
                                            else
                                            {
                                                await _watiService.SendDuplicateAnswerMessageTemplate(guest, _event);
                                            }
                                        }

                                    }
                                }
                            }
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        string guestNo = recepient.ToString();

                        var guest = await db.GuestsDeliveredourServiceMessage.FirstOrDefaultAsync(p => p.PhoneNumber == guestNo && p.ProviderName == "Wati");
                        if (guest == null)
                        {
                            await _watiService.SendEventProServiceTemplate(guestNo);
                            var guestsDeliveredourServiceMessage = new GuestsDeliveredourServiceMessage()
                            {
                                PhoneNumber = guestNo,
                                AddedDate = DateTime.UtcNow.ToString(),
                                ProviderName = "Wati"
                            };
                            await db.GuestsDeliveredourServiceMessage.AddAsync(guestsDeliveredourServiceMessage);
                        }
                        else
                        {
                            var sentPeriod = DateTime.UtcNow.Subtract(DateTime.Parse(guest.AddedDate));
                            if (sentPeriod.Days > 0)
                            {
                                await _watiService.SendEventProServiceTemplate(guestNo);
                                guest.AddedDate = DateTime.UtcNow.ToString();
                                guest.ProviderName = "Wati";

                                db.GuestsDeliveredourServiceMessage.Update(guest);

                            }
                        }
                        await db.SaveChangesAsync();
                    }
                }


                return Ok(new { message = "Ack Received" });

            }
            catch (Exception ex)
            {
                Log.Error($"error exured in wacallback :{ex.Message},inner:{ex.InnerException}, body:{body}");
                return Ok(new { message = "Ack Received" });
            }
        }
    }
}
