using Microsoft.Extensions.Configuration;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Dto;
using EventPro.DAL.Models;
using EventPro.Web.Services;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioMessagesSendingFactory : IMessagesSendingFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheStoreService _memoryCacheStoreService;
        private readonly UrlProtector _urlProtector;
        public TwilioMessagesSendingFactory(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService, UrlProtector urlProtector)
        {
            _configuration = configuration;
            _memoryCacheStoreService = memoryCacheStoreService;
            messagesTemplates = new TwilioMessageTemplates(configuration,
                memoryCacheStoreService, urlProtector);
            messagesSync = new Lazy<IMessagesSync>(() =>
            new TwilioMessagesSync(configuration, _memoryCacheStoreService));
            TemplatesSync = new Lazy<ITemplateSync>(() =>
            new TwilioTemplateSync(configuration, _memoryCacheStoreService));
            _urlProtector = urlProtector;
        }

        protected IMessageTemplates messagesTemplates { get; set; }
        protected Lazy<ITemplateSync> TemplatesSync { get; set; }
        private Lazy<IMessagesSync> messagesSync { get; set; }

        public async Task SendCongratulationMessageAsync(List<Guest> guests, Events _event)
        {
            if (_event.ReminderMessageTempName == "TemplateWithVariables" && !string.IsNullOrEmpty(_event.ThanksTempId))
            {
                // .Value Get the instance from Lazy<> 
                await messagesTemplates.CongratulationsMessageTemplate.Value.SendCustomTemplateWithVariables(guests, _event);
            }
            else if (string.IsNullOrEmpty(_event.CongratulationMsgHeaderImg))
            {
                if (_event.ConguratulationsMsgTemplateName == "Template (1)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp1(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (2)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp2(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (3)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp3(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (4)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp4(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (5)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp5(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (6)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp6(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (7)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp7(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (8)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp8(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (9)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp9(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (10)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp10(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Custom" &&
                  string.IsNullOrEmpty(_event.ThanksTempId))
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendThanksCustom(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Custom" &&
                                    !string.IsNullOrEmpty(_event.ThanksTempId))
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendThanksById(guests, _event);
                }
            }
            else
            {
                if (_event.ConguratulationsMsgTemplateName == "Template (1)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp1WithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (2)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp2WithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (3)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp3WithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (4)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp4WithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (5)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp5WithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (6)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp6WithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (7)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp7WithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (8)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp8WithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (9)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp9WithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Template (10)")
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendTemp10WithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Custom" &&
                  string.IsNullOrEmpty(_event.ThanksTempId))
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendThanksCustomWithHeaderImage(guests, _event);
                }
                else if (_event.ConguratulationsMsgTemplateName == "Custom" &&
                                    !string.IsNullOrEmpty(_event.ThanksTempId))
                {
                    await messagesTemplates.CongratulationsMessageTemplate.Value
                        .SendThanksByIdWithHeaderImage(guests, _event);
                }
            }
        }
        public ICongratulationsMessageTemplates GetCongratulatioinMessageTemplates()
        {
            return new TwilioCongratulationTemplates(_configuration, _memoryCacheStoreService);
        }

        public async Task SendCardMessagesAsync(List<Guest> guests, Events _event)
        {
            bool isArabic = System.Text.RegularExpressions.Regex.IsMatch(_event.ParentTitle, @"\p{IsArabic}");

            if (_event.CardInvitationTemplateType == "Twilio | TemplateWithVariables" &&
                !string.IsNullOrEmpty(_event.CustomCardInvitationTemplateName))
            {
                await messagesTemplates.CardMessageTemplate.Value.SendCustomTemplateWithVariables(guests, _event);
            }
            else if (_event.MessageLanguage == "Twilio | Arabic" &&
                string.IsNullOrEmpty(_event.CustomCardInvitationTemplateName) ||
                _event.MessageLanguage == "Twilio | Custom" &&
                string.IsNullOrEmpty(_event.CustomCardInvitationTemplateName) ||
                _event.MessageLanguage == "Twilio | TemplateWithVariables" &&
                string.IsNullOrEmpty(_event.CustomCardInvitationTemplateName) &&
                isArabic)
            {
                await SendArabicCardTemplates(guests, _event);
            }
            else if (_event.MessageLanguage == "Twilio | English" &&
                string.IsNullOrEmpty(_event.CustomCardInvitationTemplateName) ||
                _event.MessageLanguage == "Twilio | Custom" &&
                string.IsNullOrEmpty(_event.CustomCardInvitationTemplateName) ||
                _event.MessageLanguage == "Twilio | TemplateWithVariables" &&
                string.IsNullOrEmpty(_event.CustomCardInvitationTemplateName) &&
                !isArabic)
            {
                await SendEnglishCardTemplates(guests, _event);
            }
            else if (!string.IsNullOrEmpty(_event.CustomCardInvitationTemplateName))
            {
                await SendCustomCardTemplates(guests, _event);
            }
        }
        public ICardMessageTemplates GetCardMessageTemplates()
        {
            return new TwilioCardTemplates(_configuration, _memoryCacheStoreService);
        }

        public async Task SendConfirmationMessagesAsync(List<Guest> guests, Events evt)
        {
            if (evt.MessageLanguage == "Twilio | TemplateWithVariables")
            {
                await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomTemplateWithVariables(guests, evt);
            }
            else if (evt.ParentTitleGender == "Female")
            {
                if (evt.SendingType == "Basic")
                {
                    await SendFemaleBasicTemplates(guests, evt);
                }
                else
                {
                    await SendFemaleDefualtTemplates(guests, evt);
                }
            }
            else if (evt.ParentTitleGender == "Male")
            {
                if (evt.SendingType == "Basic")
                {
                    await SendMaleBasicTemplates(guests, evt);
                }
                else
                {
                    await SendMaleDefaultTemplates(guests, evt);
                }
            }

            return;
        }
        public IConfirmationMessageTemplates GetConfirmationMessageTemplates()
        {
            return new TwilioConfirmationTemplates(_configuration, _memoryCacheStoreService, _urlProtector);
        }

        public async Task SendEventLocationAsync(List<Guest> guests, Events evt)
        {
            bool isArabic = System.Text.RegularExpressions.Regex.IsMatch(evt.ParentTitle, @"\p{IsArabic}");
            if (evt.MessageLanguage == "Twilio | Arabic" || isArabic)
            {
                await messagesTemplates.EventLocationMessageTemplate.Value.SendArabicEventLocation(guests, evt);

            }
            else if (evt.MessageLanguage == "Twilio | English" || !isArabic)
            {
                await messagesTemplates.EventLocationMessageTemplate.Value.SendEnglishEventLocation(guests, evt);

            }
        }
        public IEventLocationMessageTemplates GetEventLocationTemplates()
        {
            return new TwilioEventLocatioinTemplates(_configuration, _memoryCacheStoreService);
        }

        // Ghrabawy : not implmented yet (19/1)
        public Task SendDuplicateAnswerAsync(List<Guest> guests, Events events)
        {
            throw new NotImplementedException();
        }
        public IDuplicateMessageTemplates GetDuplicateAnswerTemplates()
        {
            return new TwilioDuplicateMessageTemplates(_configuration, _memoryCacheStoreService);
        }

        public async Task SendReminderMessageAsync(List<Guest> guests, Events events)
        {
            if (events.ReminderMessageTempName == "TemplateWithVariables" && !string.IsNullOrEmpty(events.ReminderTempId))
            {
                await messagesTemplates.ReminderMessageTemplate.Value.SendCustomTemplateWithVariables(guests, events);
            }

            else if (string.IsNullOrEmpty(events.ReminderMsgHeaderImg))
            {
                if (events.ReminderMessageTempName == "Custom" &&
                                        !string.IsNullOrEmpty(events.ReminderMessage) &&
                                        string.IsNullOrEmpty(events.ReminderTempId))
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                           .SendReminderCustom(guests, events);
                }
                else if (events.ReminderMessageTempName == "Template (1)")
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                        .SendRTemp1(guests, events);
                }
                else if (events.ReminderMessageTempName == "Template (1) With Calender ICS")
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                        .SendRTemp1WithCalenderICS(guests, events);
                }

                else if (events.ReminderMessageTempName == "Template (2)" ||
                    events.ReminderMessageTempName == "Template (3)")
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                       .SendRTemp2or3(guests, events);
                }
                else if (events.ReminderMessageTempName == "Template (2) With Calender ICS" ||
                         events.ReminderMessageTempName == "Template (3) With Calender ICS")
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                       .SendRTemp2or3WithCalenderICS(guests, events);
                }
                else if (!string.IsNullOrEmpty(events.ReminderTempId))
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                        .SendReminderWithTempId(guests, events);
                }
            }
            else
            {
                if (events.ReminderMessageTempName == "Custom" &&
                                       !string.IsNullOrEmpty(events.ReminderMessage) &&
                                       string.IsNullOrEmpty(events.ReminderTempId))
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                           .SendReminderCustomWithHeaderImage(guests, events);
                }
                else if (events.ReminderMessageTempName == "Template (1)")
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                        .SendRTemp1WithHeaderImage(guests, events);
                }
                else if (events.ReminderMessageTempName == "Template (1) With Calender ICS")
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                        .SendRTemp1WithHeaderImageWithCalenderICS(guests, events);
                }

                else if (events.ReminderMessageTempName == "Template (2)" ||
                    events.ReminderMessageTempName == "Template (3)")
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                       .SendRTemp2or3WithHeaderImage(guests, events);
                }
                else if (events.ReminderMessageTempName == "Template (2) With Calender ICS" ||
                    events.ReminderMessageTempName == "Template (3) With Calender ICS")
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                       .SendRTemp2or3WithHeaderImageWithCalenderIcs(guests, events);
                }
                else if (!string.IsNullOrEmpty(events.ReminderTempId))
                {
                    await messagesTemplates.ReminderMessageTemplate.Value
                        .SendReminderWithTempIdWithHeaderImage(guests, events);
                }
            }
        }
        public IReminderMessageTemplates GetReminderMessageTemplates()
        {
            return new TwilioReminderMessageTemplates(_configuration, _memoryCacheStoreService);
        }

        public async Task SendDeclineMessageAsync(List<Guest> guests, Events events)
        {
            if (string.IsNullOrEmpty(events.DeclineTempId))
            {
                await messagesTemplates.DeclineMessageTemplate.Value
                      .SendDeclineTemplate(guests, events);
            }
            else if (!string.IsNullOrEmpty(events.DeclineTempId))
            {
                await messagesTemplates.DeclineMessageTemplate.Value
                   .SendCustomDeclineTemplate(guests, events);
            }
        }
        public IDeclineMessageTemplates GetDeclineMessageTemplates()
        {
            return new TwilioDeclineMessageTemplates(_configuration, _memoryCacheStoreService);
        }

        public async Task UpdateMessagesStatus(List<Guest> guests, Events _events)
        {
            await messagesSync.Value.UpdateMessagesStatusAsync(guests, _events);
        }
        public ITemplateSync GetTemplatesSync()
        {
            return new TwilioTemplateSync(_configuration, _memoryCacheStoreService);
        }
        public async Task<List<MessageLog>> GetGuestMessagesAsync(string number, string profileName)
        {
            List<MessageLog> messages = await messagesSync.Value.GetGuestMessagesAsync(number, profileName);

            return messages;
        }
        public IGateKeeperMessageTemplates GetGateKeeperMessageTemplates()
        {
            return new TwilioGateKeeperMessageTemplates(_configuration, _memoryCacheStoreService);
        }


        private async Task SendCustomCardTemplates(List<Guest> guests, Events _event)
        {
            if (_event.SendingType == "Basic")
            {
                await messagesTemplates.CardMessageTemplate.Value.SendCardByIDBasic(guests, _event);
            }
            else
            {
                await messagesTemplates.CardMessageTemplate.Value.SendCardByIDWithGusetName(guests, _event);

            }
        }
        private async Task SendEnglishCardTemplates(List<Guest> guests, Events _event)
        {
            if (_event.SendingType == "Basic")
            {
                await messagesTemplates.CardMessageTemplate.Value.SendEnglishCard(guests, _event);
            }
            else
            {
                await messagesTemplates.CardMessageTemplate.Value.SendEnglishCardwithname(guests, _event);
            }
        }
        private async Task SendArabicCardTemplates(List<Guest> guests, Events _event)
        {
            if (_event.SendingType == "Basic")
            {
                await messagesTemplates.CardMessageTemplate.Value.SendArabicCard(guests, _event);
            }
            else
            {
                await messagesTemplates.CardMessageTemplate.Value.SendArabicCardwithname(guests, _event);

            }
        }
        private async Task SendMaleDefaultTemplates(List<Guest> guests, Events evt)
        {
            if (evt.MessageLanguage == "Twilio | Arabic")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicMaleDefault(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {

                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicMaleWithHeaderText(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicMaleWithHeaderImage(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicMaleWithHeaderImageAndHeaderText(guests, evt);
                }
            }
            else if (evt.MessageLanguage == "Twilio | English")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendEnglishDefault(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendEnglishWithHeaderText(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendEnglishWithHeaderImage(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendEnglishWithHeaderImageAndHeaderText(guests, evt);
                }
            }
            else if (evt.MessageLanguage == "Twilio | Custom")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomWithName(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomWithNameHeaderText(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomWithNameHeaderImage(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomWithNameHeaderTextImage(guests, evt);
                }
            }
        }
        private async Task SendMaleBasicTemplates(List<Guest> guests, Events evt)
        {
            if (evt.MessageLanguage == "Twilio | Arabic")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicbasic(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicbasicHeaderText(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicbasicHeaderImage(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicbasicHeaderTextImage(guests, evt);
                }
            }
            else if (evt.MessageLanguage == "Twilio | English")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendEnglishbasic(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendbasicHeaderTextEnglish(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendbasicHeaderImageEnglish(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendbasicHeaderTextImageEnglish(guests, evt);
                }
            }
            else if (evt.MessageLanguage == "Twilio | Custom")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomBasic(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomBasicHeaderText(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomBasicHeaderImage(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomBasicHeaderTextImage(guests, evt);
                }
            }
        }
        private async Task SendFemaleDefualtTemplates(List<Guest> guests, Events evt)
        {
            if (evt.MessageLanguage == "Twilio | Arabic")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) && (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicFemaleDefault(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) && (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicFemaleWithHeaderText(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) && (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicFemaleWithHeaderImage(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) && (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicFemaleWithHeaderImageAndHeaderText(guests, evt);
                }
            }
            else if (evt.MessageLanguage == "Twilio | English")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) && (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendEnglishDefault(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) && (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendEnglishWithHeaderText(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) && (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendEnglishWithHeaderImage(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) && (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendEnglishWithHeaderImageAndHeaderText(guests, evt);
                }
            }
            else if (evt.MessageLanguage == "Twilio | Custom")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) && (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomWithName(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) && (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomWithNameHeaderText(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) && (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomWithNameHeaderImage(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) && (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomWithNameHeaderTextImage(guests, evt);
                }
            }
        }
        private async Task SendFemaleBasicTemplates(List<Guest> guests, Events evt)
        {
            if (evt.MessageLanguage == "Twilio | Arabic")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicbasic(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicbasicHeaderText(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicbasicHeaderImage(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendArabicbasicHeaderTextImage(guests, evt);
                }
            }
            else if (evt.MessageLanguage == "Twilio | English")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendEnglishbasic(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendbasicHeaderTextEnglish(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendbasicHeaderImageEnglish(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendbasicHeaderTextImageEnglish(guests, evt);
                }
            }
            else if (evt.MessageLanguage == "Twilio | Custom")
            {
                if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomBasic(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage == null || evt.MessageHeaderImage == string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomBasicHeaderText(guests, evt);
                }
                else if ((evt.MessageHeaderText == null || evt.MessageHeaderText == string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomBasicHeaderImage(guests, evt);
                }
                else if ((evt.MessageHeaderText != null || evt.MessageHeaderText != string.Empty) &&
                    (evt.MessageHeaderImage != null || evt.MessageHeaderImage != string.Empty))
                {
                    await messagesTemplates.ConfirmationMessageTemplate.Value.SendCustomBasicHeaderTextImage(guests, evt);
                }
            }
        }
    }

}
