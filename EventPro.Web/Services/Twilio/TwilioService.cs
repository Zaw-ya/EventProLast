using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using EventPro.Services.TwilioService.Interface;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace EventPro.Services.TwilioService
{
    public class TwilioService : ITwilioService
    {
        #region .............
        private readonly IConfiguration _configuration;
        private IConfiguration CountrySendingProfileConfig;
        private IConfiguration choosenSendingWhatsappProfileSettingsConfig;
        private readonly IEmailSender _emailSender;
        private readonly EventProContext _db;
        public TwilioService(IConfiguration configuration, IEmailSender emailSender)
        {
            _configuration = configuration;
            _db = new EventProContext(configuration);
            _emailSender = emailSender;
        }
        #endregion

        private void setCountrySendingProfile(string profile)
        {
            if (profile == "Da3wty")
            {
                CountrySendingProfileConfig = _configuration.GetSection("Da3wty");
            }
            else if (profile == "Da3wty[2]")
            {
                CountrySendingProfileConfig = _configuration.GetSection("Da3wty[2]");
            }
            else if (profile == "EventProKuwait")
            {
                CountrySendingProfileConfig = _configuration.GetSection("EventProKuwait");
            }
            else if (profile == "EstablishmentOfDa3wati")
            {
                CountrySendingProfileConfig = _configuration.GetSection("EstablishmentOfDa3wati");
            }
            else if (profile == "EstablishmentOfDa3wati[2]")
            {
                CountrySendingProfileConfig = _configuration.GetSection("EstablishmentOfDa3wati[2]");
            }
            else if (profile == "Da3watiGulf")
            {
                CountrySendingProfileConfig = _configuration.GetSection("Da3watiGulf");
            }
            else
            {
                CountrySendingProfileConfig = _configuration.GetSection("EventProBackup");
            }
        }


        #region   ????? ???? ???????  

        // Female and Male + Basic => Arabic
        public async Task<Guest> SendArabicbasic(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            if (events.ParentTitleGender == "Female")
            {
                templateId = CountrySendingProfileConfig["Templates:SendArabicbasic(Female)"];
            }
            else
            {
                templateId = CountrySendingProfileConfig["Templates:SendArabicbasic(male)"];
            }

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendArabicbasicHeaderText(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            if (events.ParentTitleGender == "Female")
            {
                templateId = CountrySendingProfileConfig["Templates:SendArabicbasicHeaderText(Female)"];
            }
            else
            {
                templateId = CountrySendingProfileConfig["Templates:SendArabicbasicHeaderText(male)"];
            }

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendArabicbasicHeaderImage(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            if (events.ParentTitleGender == "Female")
            {
                templateId = CountrySendingProfileConfig["Templates:SendArabicbasicHeaderImage(female)"];
            }
            else
            {
                templateId = CountrySendingProfileConfig["Templates:SendArabicbasicHeaderImage(male)"];
            }

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendArabicbasicHeaderTextImage(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            if (events.ParentTitleGender == "Female")
            {
                templateId = CountrySendingProfileConfig["Templates:SendArabicbasicHeaderTextImage(female)"];
            }
            else
            {
                templateId = CountrySendingProfileConfig["Templates:SendArabicbasicHeaderTextImage(male)"];
            }

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        // Female and Male + Basic => English
        public async Task<Guest> SendEnglishbasic(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendEnglishbasic"];
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendbasicHeaderTextEnglish(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendbasicHeaderTextEnglish"];
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }

            return guest;
        }
        public async Task<Guest> SendbasicHeaderImageEnglish(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendbasicHeaderImageEnglish"];


            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }

            return guest;
        }
        public async Task<Guest> SendbasicHeaderTextImageEnglish(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendbasicHeaderTextImageEnglish"];
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }

            return guest;
        }

        // Female + Guest Name => Arabic
        public async Task<Guest> SendArabicFemaleDefault(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendArabicFemaleDefault"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                // Update Guest table with Twilio response details       
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendArabicFemaleWithHeaderImage(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendArabicFemaleWithHeaderImage"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendArabicFemaleWithHeaderText(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendArabicFemaleWithHeaderText"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendArabicFemaleWithHeaderImageAndHeaderText(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var templateId = CountrySendingProfileConfig["Templates:SendArabicFemaleWithHeaderImageAndHeaderText"];

            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
            var parameters = new string[]
            {

                guest.FirstName.Trim(),
                events.ParentTitle.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.Trim(),
                events.MessageHeaderImage.ToString(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        // Male + Guest Name => Arabic
        public async Task<Guest> SendArabicMaleDefault(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendArabicMaleDefault"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendArabicMaleWithHeaderImage(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendArabicMaleWithHeaderImage"];
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendArabicMaleWithHeaderText(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendArabicMaleWithHeaderText"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {

                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendArabicMaleWithHeaderImageAndHeaderText(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendArabicMaleWithHeaderImageAndHeaderText"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        // Female and Male + Guest Name => English
        public async Task<Guest> SendEnglishDefault(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendEnglishDefault"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendEnglishWithHeaderImage(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendEnglishWithHeaderImage"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendEnglishWithHeaderText(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendEnglishWithHeaderText"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendEnglishWithHeaderImageAndHeaderText(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = CountrySendingProfileConfig["Templates:SendEnglishWithHeaderImageAndHeaderText"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;

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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {

                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        #endregion

        #region ????? ?? ????????????

        //Female Or Male + English Or Arabic => Guest name
        public async Task<Guest> SendCustomWithName(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = events.CustomInvitationMessageTemplateName;
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId

            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                // Update Guest table with Twilio response details        
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        public async Task<Guest> SendCustomWithNameHeaderText(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = events.CustomInvitationMessageTemplateName;
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                // Update Guest table with Twilio response details    
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }

            return guest;
        }

        public async Task<Guest> SendCustomWithNameHeaderImage(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = events.CustomInvitationMessageTemplateName; //Marketing
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.MessageHeaderImage.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                // Update Guest table with Twilio response details

                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        public async Task<Guest> SendCustomWithNameHeaderTextImage(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = events.CustomInvitationMessageTemplateName;
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.MessageHeaderImage.ToString(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                // Update Guest table with Twilio response details   
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }


            return guest;
        }

        //Female Or Male + English Or Arabic => Basic
        public async Task<Guest> SendCustomBasic(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = events.CustomInvitationMessageTemplateName;
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
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

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                // Update Guest table with Twilio response details        
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }

            return guest;
        }

        public async Task<Guest> SendCustomBasicHeaderText(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = events.CustomInvitationMessageTemplateName;
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
            var parameters = new string[]
            {
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                // Update Guest table with Twilio response details     
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }

            return guest;
        }

        public async Task<Guest> SendCustomBasicHeaderImage(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = events.CustomInvitationMessageTemplateName; //Marketing
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
            var parameters = new string[]
            {
                events.MessageHeaderImage.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                // Update Guest table with Twilio response details     
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        public async Task<Guest> SendCustomBasicHeaderTextImage(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = events.CustomInvitationMessageTemplateName;
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string yesButtonId = "yesButton" + events.Id + guest.GuestId;
            string noButtonId = "noButton" + events.Id + guest.GuestId;
            string eventLocationButtonId = "eventLocationButton" + events.Id + guest.GuestId;
            var parameters = new string[]
            {
                events.MessageHeaderImage.ToString(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                // Update Guest table with Twilio response details     
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
            }

            return guest;
        }

        #endregion

        #region  ????? ??????

        public async Task<Guest> SendArabicCard(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:ArabicCard"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
            var parameters = new string[]
            {
               imagePathSegment.ToString(),
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.ImgSentMsgId = messageSid;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendArabicCardwithname(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:ArabicCardWithName"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
            var parameters = new string[]
            {
                imagePathSegment.ToString(),
                guest.FirstName.Trim(),
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {

                guest.ImgSentMsgId = messageSid;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        public async Task<Guest> SendEnglishCard(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:EnglishCard"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
            var parameters = new string[]
            {
               imagePathSegment.ToString(),
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.ImgSentMsgId = messageSid;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendEnglishCardwithname(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:EnglishCardWithName"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
            var parameters = new string[]
            {
                imagePathSegment.ToString(),
                guest.FirstName.Trim(),
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {

                guest.ImgSentMsgId = messageSid;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }


        public async Task<Guest> SendCardByIDBasic(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = events.CustomCardInvitationTemplateName;
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
            var parameters = new string[]
            {
               imagePathSegment.ToString(),
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {

                guest.ImgSentMsgId = messageSid;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendCardByIDWithGusetName(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = events.CustomCardInvitationTemplateName;
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string imagePathSegment = events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
            var parameters = new string[]
            {
               imagePathSegment.ToString(),
               guest.FirstName.Trim(),
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {

                guest.ImgSentMsgId = messageSid;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        #endregion

        #region  ??? ?????   

        public async Task<Guest> SendThanksById(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var templateId = events.ThanksTempId;
            string[] parameters;
            if (events.SendingType == "Basic")
            {
                parameters = new string[] { conguratulationId, };

            }
            else
            {
                parameters = new string[] { guest.FirstName.Trim(), conguratulationId, };
            }
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);


            if (messageSid != null)
            {
                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendThanksCustom(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var templateId = "";
            string[] parameters;
            if (events.SendingType == "Basic")
            {
                templateId = CountrySendingProfileConfig["Templates:SendThanksCustomBasic"];
                parameters = new string[] { events.ThanksMessage, conguratulationId, };

            }
            else
            {
                templateId = CountrySendingProfileConfig["Templates:SendThanksCustom"];
                parameters = new string[] { guest.FirstName.Trim(), events.ThanksMessage, conguratulationId, };
            }
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);


            if (messageSid != null)
            {
                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendTemp1(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendTemp1"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,

            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {

                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendTemp2(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendTemp2"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,
                events.ParentTitle.Trim(),

            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            if (messageSid != null)
            {

                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendTemp3(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendTemp3"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,
                events.ParentTitle.Trim(),

            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {

                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendTemp4(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendTemp4"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,
                events.ParentTitle.Trim(),


            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {

                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendTemp5(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendTemp5"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,
                events.ParentTitle.Trim(),


            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {

                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendTemp6(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendTemp6"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,

            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {
                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendTemp7(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendTemp7"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,
                events.ParentTitle.Trim(),

            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {
                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendTemp8(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendTemp8"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,
                events.ParentTitle.Trim(),

            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {
                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendTemp9(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendTemp9"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,
                events.ParentTitle.Trim(),


            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {
                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendTemp10(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendTemp10"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var conguratulationId = Guid.NewGuid().ToString();
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                conguratulationId,
                events.ParentTitle.Trim(),


            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {
                guest.ConguratulationMsgId = messageSid;
                guest.ConguratulationMsgLinkId = conguratulationId;
                guest.ConguratulationMsgCount = 1;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        #endregion

        #region ????? ????? ????? ?????? 
        public async Task<Guest> SendCongratulationMessageToOwner(Guest guest, string message)
        {
            CountrySendingProfileConfig = _configuration.GetSection("EventProBackup");
            var templateId = CountrySendingProfileConfig["Templates:SendCongratulationMessageToOwner"];
            var eventSentOnNum = await _db.Events.Where(x => x.Id == guest.EventId).Select(x => x.ConguratulationsMsgSentOnNumber).FirstOrDefaultAsync();
            var detectTheEvet = await _db.Events.Where(x => x.Id == guest.EventId).FirstOrDefaultAsync();
            string fullPhoneNumber = $"+{eventSentOnNum}";
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                message,
            };
            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, detectTheEvet.CityId, 1, "EventProBackup", "SAUDI");
            if (messageSid != null)
            {
                guest.ConguratulationMsgCount = 0;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendCongratulationMessageToOwnerEnglish(Guest guest, string message)
        {
            var templateId = CountrySendingProfileConfig["Templates:SendCongratulationMessageToOwnerEnglish"];

            var eventSentOnNum = await _db.Events.Where(x => x.Id == guest.EventId).Select(x => x.ConguratulationsMsgSentOnNumber).FirstOrDefaultAsync();
            var detectTheEvet = await _db.Events.Where(x => x.Id == guest.EventId).FirstOrDefaultAsync();
            string fullPhoneNumber = $"+{eventSentOnNum}";
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                message,
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, detectTheEvet.CityId, 1, "EventPro", "SAUDI");

            if (messageSid != null)
            {
                guest.ConguratulationMsgCount = 0;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        #endregion

        #region ??????? ????????? 
        public async Task<Guest> SendReminderWithTempIdold(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = events.ReminderTempId;
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string[] parameters;
            if (events.SendingType == "Basic")
            {
                parameters = new string[] { };
            }
            else
            {
                parameters = new string[] { guest.FirstName.Trim() };
            }
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {
                //
                //
                //
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }
        public async Task<Guest> SendReminderCustomold(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var templateId = "";
            string[] parameters;
            if (events.SendingType == "Basic")
            {
                templateId = "HX98d822e1d9fc179e366f1ae969fef330";
                parameters = new string[] { events.ReminderMessage };
            }
            else
            {
                templateId = "HXb0c250b584788c009d10c8c6b61016c2";
                parameters = new string[] { guest.FirstName.Trim(), events.ReminderMessage };

            }

            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);


            if (messageSid != null)
            {
                //
                //
                //
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }

        public async Task<Guest> SendRTemp1old(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = "HXb13a0429a298b105589edcf84c969fa2";
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),

            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            // Database operations
            if (messageSid != null)
            {
                //
                //
                //
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        public async Task<Guest> SendRTemp2or3old(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = "";
            if (events.ParentTitleGender == "Female")
            {
                templateId = "HXb94514e82e1218b74f45e46b1d614bc4";
            }
            else
            {
                templateId = "HXde6c602c1f78bdf15141dcb92334fb3a";
            }
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
                events.EventTitle.Trim(),
                evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                evntDate.ToString("dd/MM/yyyy"),
                events.EventVenue.ToString().Trim(),
                events.ParentTitle.Trim(),

            };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            if (messageSid != null)
            {
                //
                //
                //
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }
        #endregion

        #region ????? ??????? ??? ???????
        public async Task<string> SendArabicDuplicateAnswer(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendArabicDuplicateAnswer"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
            };

            await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            return "sent"; ;
        }
        public async Task<string> SendEnglishDuplicateAnswer(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendEnglishDuplicateAnswer"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var parameters = new string[]
            {
                guest.FirstName.Trim(),
            };

            await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            return "sent"; ;
        }

        #endregion

        #region  ???? ???????? 
        public async Task<Guest> SendArabicEventLocation(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendArabicEventLocation"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var location = "https://maps.app.goo.gl/" + events.GmapCode;
            var parameters = new string[]
            {
               location,
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            if (messageSid != null)
            {
                guest.waMessageEventLocationForSendingToAll = messageSid;
            }
            else
            {
                throw new Exception();
            }
            return guest;
        }
        public async Task<Guest> SendEnglishEventLocation(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            var templateId = CountrySendingProfileConfig["Templates:SendEnglishEventLocation"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var location = "https://maps.app.goo.gl/" + events.GmapCode;
            var parameters = new string[]
            {
               location,
            };

            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            if (messageSid != null)
            {
                guest.waMessageEventLocationForSendingToAll = messageSid;
            }
            else
            {
                throw new Exception();
            }
            return guest;
        }
        #endregion

        #region  ???? ????????
        //KSA
        public async Task<string> SendEventProArabicService(string phoneNumber)
        {
            var templateId = CountrySendingProfileConfig["Templates:SendEventProArabicService"];
            string fullPhoneNumber = $"+{phoneNumber}";
            string EventProLogo = "EventProBackground.jpg";
            var parameters = new string[]
            {
               EventProLogo,
            };
            int cityid = 1;
            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, cityid, 1, "EventPro", "SAUDI");

            return messageSid;
        }
        //Kuwait
        public async Task<string> SendEventProArabicServiceKuwait(string phoneNumber)
        {
            var templateId = CountrySendingProfileConfig["Templates:SendEventProArabicServiceKuwait"];
            string fullPhoneNumber = $"+{phoneNumber}";
            string EventProLogo = "EventProBackground.jpg";
            var parameters = new string[]
            {
               EventProLogo,
            };

            int cityid = 6;
            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, cityid, 1, "EventPro", "SAUDI");

            return messageSid;
        }
        //Bahrain 
        public async Task<string> SendEventProArabicServiceBahrain(string phoneNumber)
        {
            var templateId = CountrySendingProfileConfig["Templates:SendEventProArabicServiceBahrain"];
            string fullPhoneNumber = $"+{phoneNumber}";
            string EventProLogo = "EventProBackground.jpg";
            var parameters = new string[]
            {
               EventProLogo,
            };

            int cityid = 59;
            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, cityid, 1, "EventPro", "SAUDI");
            return messageSid;
        }
        //English Not Used 
        public async Task<string> SendEventProEnglishService(string phoneNumber)
        {

            var templateId = CountrySendingProfileConfig["Templates:SendEventProEnglishService"];
            string fullPhoneNumber = $"+{phoneNumber}";
            string EventProLogo = "EventProBackground.jpg";
            var parameters = new string[]
            {
               EventProLogo,
            };
            //If the message came from kuwait number => send the message from our kuwait number
            int cityid = phoneNumber.StartsWith("965") ? 6 : 1;
            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, cityid, 1, "EventPro", "SAUDI");

            return messageSid;
        }
        #endregion

        #region ???? ??? ?????? ?????


        public async Task<Guest> SendDeclineTemp(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var templateId = events.DeclineTempId;
            string[] parameters;
            if (events.SendingType == "Basic")
            {
                parameters = new string[] { };

            }
            else
            {
                parameters = new string[] { guest.FirstName.Trim(), };
            }
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            return guest;

        }
        public async Task<Guest> SendDeclineTempFixedTemp(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var templateId = CountrySendingProfileConfig["Templates:SendDeclineTempFixed"];

            var parameters = new string[] { };
            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            return guest;

        }



        #endregion

        #region ???? ?????????????????????
        private async Task<string> SendWhatsAppTemplateMessageAsync(string toPhoneNumber, string contentSid, object[] parameters, int? cityId, int ChoosenNumberWithinCountry, string choosenSendingWhatsappProfile, string choosenCountryNumber)
        {


            if (choosenSendingWhatsappProfile == "Da3wty")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("Da3wtySettings");
            }
            else if (choosenSendingWhatsappProfile == "Da3wty[2]")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("Da3wty[2]Settings");
            }
            else if (choosenSendingWhatsappProfile == "EventProKuwait")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EventProKuwaitSettings");
            }
            else if (choosenSendingWhatsappProfile == "EstablishmentOfDa3wati")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EstablishmentOfDa3watiSettings");
            }
            else if (choosenSendingWhatsappProfile == "EstablishmentOfDa3wati[2]")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EstablishmentOfDa3wati[2]Settings");
            }
            else if (choosenSendingWhatsappProfile == "Da3watiGulf")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("Da3watiGulfSettings");
            }          
            else
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EventProBackupSettings");
            }

            try
            {
                string fromWhatsAppNumberSaudiChoosen = string.Empty;
                string fromWhatsAppNumberKuwaitChoosen = string.Empty;
                string FromWhatsAppNumberBahrainChoosen = string.Empty;
                string ChoosenSendingNumber = string.Empty;
                var accountSid = choosenSendingWhatsappProfileSettingsConfig["AccountSid"];
                var authToken = choosenSendingWhatsappProfileSettingsConfig["AuthToken"];
                var fromWhatsAppNumberSaudi1 = choosenSendingWhatsappProfileSettingsConfig["FromWhatsAppNumberSaudi1"];
                var fromWhatsAppNumberSaudi2 = choosenSendingWhatsappProfileSettingsConfig["FromWhatsAppNumberSaudi2"];
                var fromWhatsAppNumberKuwait1 = choosenSendingWhatsappProfileSettingsConfig["FromWhatsAppNumberKuwait1"];
                var fromWhatsAppNumberKuwait2 = choosenSendingWhatsappProfileSettingsConfig["FromWhatsAppNumberKuwait2"];
                var FromWhatsAppNumberBahrain1 = choosenSendingWhatsappProfileSettingsConfig["FromWhatsAppNumberBahrain1"];
                var FromWhatsAppNumberBahrain2 = choosenSendingWhatsappProfileSettingsConfig["FromWhatsAppNumberBahrain2"];
                var messagingServiceSid = choosenSendingWhatsappProfileSettingsConfig["MessagingServiceSid"];
                var statusCallbackUrl = choosenSendingWhatsappProfileSettingsConfig["StatusCallbackUrl"];

                if (ChoosenNumberWithinCountry == 2)
                {
                    fromWhatsAppNumberSaudiChoosen = fromWhatsAppNumberSaudi2;
                    fromWhatsAppNumberKuwaitChoosen = fromWhatsAppNumberKuwait2;
                    FromWhatsAppNumberBahrainChoosen = FromWhatsAppNumberBahrain2;
                }
                else
                {
                    fromWhatsAppNumberSaudiChoosen = fromWhatsAppNumberSaudi1;
                    fromWhatsAppNumberKuwaitChoosen = fromWhatsAppNumberKuwait1;
                    FromWhatsAppNumberBahrainChoosen = FromWhatsAppNumberBahrain1;
                }

                if (choosenCountryNumber == "BAHRAIN")
                {
                    ChoosenSendingNumber = FromWhatsAppNumberBahrainChoosen;
                }
                else if (choosenCountryNumber == "KUWAIT")
                {
                    ChoosenSendingNumber = fromWhatsAppNumberKuwaitChoosen;
                }
                else
                {
                    ChoosenSendingNumber = fromWhatsAppNumberSaudiChoosen;
                }

                TwilioClient.Init(accountSid, authToken);

                // Create the content variables dictionary
                var contentVariables = parameters.Select((p, index) => new KeyValuePair<string, string>((index + 1).ToString(), p.ToString()))
                                                 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // Serialize the content variables dictionary to JSON
                var contentVariablesJson = JsonConvert.SerializeObject(contentVariables);

                // Initialize RestClient and RestRequest
                var client = new RestClient($"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json");
                var request = new RestRequest("", Method.Post)
                    .AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}")))
                    .AddHeader("Content-Type", "application/x-www-form-urlencoded");

                // Add parameters to the request
                request.AddParameter("ContentSid", contentSid);
                request.AddParameter("To", $"whatsapp:{toPhoneNumber}");
                //============================================================================================================================================
                request.AddParameter("From", $"whatsapp:{ChoosenSendingNumber}");
                //=============================================================================================================================================
                request.AddParameter("ContentVariables", contentVariablesJson);
                request.AddParameter("MessagingServiceSid", messagingServiceSid);
                //request.AddParameter("StatusCallback", statusCallbackUrl);

                // Execute the request
                var response = await client.ExecuteAsync(request);
                var logResponse = response.Content.ToString();

                dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content);
                return jsonResponse.sid;

            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        #endregion

        #region ?????? ?? ???? ???????? 
        private async Task<string> WhichCountry(int cityId)
        {
            //Samples: Kuwait City ID = 6 ,
            //Bahrain City ID = 59 ,
            //KSA City ID = 1
            var City = await _db.City.FirstOrDefaultAsync(c => c.Id == cityId);

            var country = await _db.Country.FirstOrDefaultAsync(x => x.Id == City.CountryId);

            return country.CountryName;
        }

        #endregion

        #region ????? ????????????

        // Get the status of various messages for a guest from Twilio and update the guest's record in the database.
        public async Task<string> GetMessageStatusAndUpdateGuestAsync(Guest guest)
        {
            var choosenSendingWhatsappProfile = await _db.Events.Where(e => e.Id == guest.EventId)
                .Select(e => e.choosenSendingWhatsappProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (choosenSendingWhatsappProfile == "Da3wty")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("Da3wtySettings");
            }
            else if (choosenSendingWhatsappProfile == "Da3wty[2]")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("Da3wty[2]Settings");
            }
            else if (choosenSendingWhatsappProfile == "EventProKuwait")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EventProKuwaitSettings");
            }
            else if (choosenSendingWhatsappProfile == "EstablishmentOfDa3wati")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EstablishmentOfDa3watiSettings");
            }
            else if (choosenSendingWhatsappProfile == "EstablishmentOfDa3wati[2]")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EstablishmentOfDa3wati[2]Settings");
            }
            else
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EventProBackupSettings");
            }

            var accountSid = choosenSendingWhatsappProfileSettingsConfig["AccountSid"];
            var authToken = choosenSendingWhatsappProfileSettingsConfig["AuthToken"];

            TwilioClient.Init(accountSid, authToken);

            var guestDb = await _db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            if (guestDb == null)
            {
                return "Guest not found.";
            }

            // Check and update the main message
            if (!string.IsNullOrEmpty(guestDb.MessageId))
            {
                await FetchAndProcessMessage(guestDb.MessageId, guestDb, "Text");
            }

            // Check and update the event location message
            if (!string.IsNullOrEmpty(guestDb.waMessageEventLocationForSendingToAll))
            {
                await FetchAndProcessMessage(guestDb.waMessageEventLocationForSendingToAll, guestDb, "EventLocation");
            }

            // Check and update the QR message
            if (!string.IsNullOrEmpty(guestDb.ImgSentMsgId))
            {
                await FetchAndProcessMessage(guestDb.ImgSentMsgId, guestDb, "QR");
            }

            // Check and update the reminder message
            if (!string.IsNullOrEmpty(guestDb.ReminderMessageId))
            {
                await FetchAndProcessMessage(guestDb.ReminderMessageId, guestDb, "Reminder");
            }

            // Check and update the congratulation/thank you message
            if (!string.IsNullOrEmpty(guestDb.ConguratulationMsgId))
            {
                await FetchAndProcessMessage(guestDb.ConguratulationMsgId, guestDb, "Conguratulation");
            }

            await _db.SaveChangesAsync();
            return "Guest status updated successfully";
        }

        // Get the status of all messages related to a specific event's guests and update their records in the database.
        public async Task<string> GetMessagesAndUpdateEventGuestsAsync(Events events)
        {
            var choosenSendingWhatsappProfile = events.choosenSendingWhatsappProfile;

            if (choosenSendingWhatsappProfile == "Da3wty")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("Da3wtySettings");
            }
            else if (choosenSendingWhatsappProfile == "Da3wty[2]")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("Da3wty[2]Settings");
            }
            else if (choosenSendingWhatsappProfile == "EventProKuwait")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EventProKuwaitSettings");
            }
            else if (choosenSendingWhatsappProfile == "EstablishmentOfDa3wati")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EstablishmentOfDa3watiSettings");
            }
            else if (choosenSendingWhatsappProfile == "EstablishmentOfDa3wati[2]")
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EstablishmentOfDa3wati[2]Settings");
            }
            else
            {
                choosenSendingWhatsappProfileSettingsConfig = _configuration.GetSection("EventProBackupSettings");
            }

            var accountSid = choosenSendingWhatsappProfileSettingsConfig["AccountSid"];
            var authToken = choosenSendingWhatsappProfileSettingsConfig["AuthToken"];

            TwilioClient.Init(accountSid, authToken);

            // Fetch all guests associated with the specified event
            var eventGuests = await _db.Guest.Where(g => g.EventId == events.Id).ToListAsync();

            foreach (var guest in eventGuests)
            {
                // Process and update status for each type of message
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
            }

            await _db.SaveChangesAsync();

            return "Event guests' statuses updated successfully";
        }

        // Fetch the status of a specific message from Twilio and update the guest's record based on the type of message.
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

        // Update the guest's record based on the message type and status received from Twilio.
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

        // Set specific status fields in the guest's record (e.g., Read, Delivered) based on the message type.
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

        #endregion

        #region ??????? ?????? ???????? ??????????

        #region ????? ??? ??????? ?????
        public string GenerateCalendarEventICS(string eventName, DateTime eventDate, string eventLocation)
        {

            // Define the Middle East Timezone (UTC+3)
            TimeZoneInfo middleEastTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");

            // Define the attendance time (7:00 PM)
            DateTime attendanceTime = eventDate.Date.AddHours(19); // 7:00 PM

            // Validate the attendanceTime in the Middle East Timezone
            if (middleEastTimeZone.IsInvalidTime(attendanceTime))
            {
                // Adjust to the next valid time
                attendanceTime = attendanceTime.AddMinutes(1);
            }

            // Convert to Middle East Time
            var middleEastAttendanceTime = TimeZoneInfo.ConvertTime(attendanceTime, middleEastTimeZone);

            // Calculate the alert time (5 hours before the event starts)
            var alertTime = middleEastAttendanceTime.AddHours(-5);

            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("METHOD:PUBLISH");
            sb.AppendLine("BEGIN:VTIMEZONE");
            sb.AppendLine("TZID:Arab Standard Time");
            sb.AppendLine("BEGIN:STANDARD");
            sb.AppendLine("DTSTART:16010101T000000");
            sb.AppendLine("TZOFFSETFROM:+0300");
            sb.AppendLine("TZOFFSETTO:+0300");
            sb.AppendLine("END:STANDARD");
            sb.AppendLine("END:VTIMEZONE");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{Guid.NewGuid()}");
            sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            sb.AppendLine($"DTSTART;TZID=Arab Standard Time:{middleEastAttendanceTime:yyyyMMddTHHmmss}"); // 7:00 PM
            sb.AppendLine($"SUMMARY:{eventName}");
            sb.AppendLine($"LOCATION:{eventLocation}");
            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine("TRIGGER:-PT5H"); // 5 hours before
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine("DESCRIPTION:Reminder");
            sb.AppendLine("END:VALARM");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        public async Task<string> SaveCalendarEventToFileAsync(string icsContent, string fileName)
        {
            //var directoryPath = Path.Combine(@"H:\Upload\Prod\Calendar");
            var directoryPath = Path.Combine(@"H:\Upload\prod\Calendar");

            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, fileName);

            try
            {
                // Check if the file already exists
                if (File.Exists(filePath))
                {
                    // If it exists, delete the old file
                    File.Delete(filePath);
                }

                // Save the new ICS file
                await File.WriteAllTextAsync(filePath, icsContent);
            }
            catch (IOException ioEx)
            {
                throw;
            }

            return $"{fileName}";
        }

        #endregion

        public async Task<Guest> SendReminderWithTempId(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);

            var templateId = events.ReminderTempId;
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";

            // Generate ICS file and save it
            var icsContent = GenerateCalendarEventICS(events.EventTitle, events.EventFrom.Value, events.EventVenue);

            var icsFileName = $"event_{events.Id}.ics";
            var icsFileUrl = await SaveCalendarEventToFileAsync(icsContent, icsFileName);

            string[] parameters;
            if (events.SendingType == "Basic")
            {
                parameters = new string[] { icsFileUrl };
            }
            else
            {
                parameters = new string[] { icsFileUrl, guest.FirstName.Trim() };
            }

            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);

            if (messageSid != null)
            {
                guest.ReminderMessageId = messageSid;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        public async Task<Guest> SendReminderCustom(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            string templateId = "";

            // Generate ICS file and save it
            var icsContent = GenerateCalendarEventICS(events.EventTitle, events.EventFrom.Value, events.EventVenue);

            var icsFileName = $"event_{events.Id}.ics";
            var icsFileUrl = await SaveCalendarEventToFileAsync(icsContent, icsFileName);

            string[] parameters;
            if (events.SendingType == "Basic")
            {
                //======================================================TEMP NAME || copy_copy_copy_remember_temp_custombasic_calender_www
                //templateId = "HXf977018a7cb7d2119cc392287981aa9e";
                templateId = CountrySendingProfileConfig["Templates:SendReminderCustomBasic"];
                parameters = new string[] { events.ReminderMessage, icsFileUrl };
            }
            else
            {
                //======================================================TEMP NAME | copy_copy_remember_temp_custombasic_calender_www
                //templateId = "HX469c107376c77b299601663a186ead02";
                templateId = CountrySendingProfileConfig["Templates:SendReminderCustom"];
                parameters = new string[] { guest.FirstName.Trim(), events.ReminderMessage, icsFileUrl };
            }

            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);


            if (messageSid != null)
            {
                guest.ReminderMessageId = messageSid;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }

        public async Task<Guest> SendRTemp1(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);

            //======================================================TEMP NAME || copy_remember_temp_1_calender_www
            //var templateId = "HXa179f751b01f9cd6c2dea1d950f0cc8b";
            var templateId = CountrySendingProfileConfig["Templates:SendRTemp1"];

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var evntDate = Convert.ToDateTime(events.EventFrom);

            // Generate ICS file and save it
            var icsContent = GenerateCalendarEventICS(events.EventTitle, events.EventFrom.Value, events.EventVenue);
            var icsFileName = $"event_{events.Id}.ics";
            var icsFileUrl = await SaveCalendarEventToFileAsync(icsContent, icsFileName);

            var parameters = new string[]
            {
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        evntDate.ToString("dddd", new CultureInfo("ar-SA")),
                        evntDate.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        icsFileUrl
            };

            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.ReminderMessageId = messageSid;
            }
            else
            {
                throw new Exception();
            }

            return guest;

        }

        public async Task<Guest> SendRTemp2or3(Guest guest, Events events)
        {
            setCountrySendingProfile(events.choosenSendingWhatsappProfile);

            //==============TEMP NAME : remember_temp_2_calender_www11 
            //string templateId = "HXf8208000e4b8b7f93e124d9a2a834672";
            string templateId = CountrySendingProfileConfig["Templates:SendRTemp2or3female"];

            if (events.ParentTitleGender != "Female")
            {
                // remember_temp_3_calender_www22
                //templateId = "HX04fb9063090b1d16d17062b39b09438c";
                templateId = CountrySendingProfileConfig["Templates:SendRTemp2or3male"];
            }
            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
            var evntDate = Convert.ToDateTime(events.EventFrom);

            var icsContent = GenerateCalendarEventICS(events.EventTitle, events.EventFrom.Value, events.EventVenue);
            var icsFileName = $"event_{events.Id}.ics";
            var icsFileUrl = await SaveCalendarEventToFileAsync(icsContent, icsFileName);

            var parameters = new string[]
            {
                        guest.FirstName.Trim(),
                        events.EventTitle.Trim(),
                        events.EventFrom.Value.ToString("dddd", new CultureInfo("ar-SA")),
                        events.EventFrom.Value.ToString("dd/MM/yyyy"),
                        events.EventVenue.ToString().Trim(),
                        events.ParentTitle.Trim(),
                        icsFileUrl
            };

            var messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, events.choosenSendingWhatsappProfile, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.ReminderMessageId = messageSid;
            }
            else
            {
                throw new Exception();
            }

            return guest;
        }


        #endregion

        #region ?????? ?? ????? ??? ???????

        public async Task<bool> ValidatePhoneNumberAsync(Guest guest)
        {

            string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";

            var client = new RestClient($"https://lookups.twilio.com/v1/PhoneNumbers/{fullPhoneNumber}?Type=carrier");
            var request = new RestRequest()
            {
                Method = Method.Get,
            };
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_configuration["Twilio:AccountSid"]}:{_configuration["Twilio:AuthToken"]}")));

            var response = await client.ExecuteAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                dynamic lookupResult = JsonConvert.DeserializeObject(response.Content);
                return lookupResult.carrier != null;
            }

            return false;
        }

        #endregion

        public async Task CheckSingleAccountAsync(TwilioProfileSettings acc)
        {
            try
            {
                var response = await GetBalanceAsync(acc.AccountSid, acc.AuthToken);
                var limitBalance = _db.AppSettings.Select(s => s.TwilioBalanceEmailAlertThreshold).FirstOrDefault();
                var recipients = _db.Users
                .Where(u => u.SendNotificationsOrEmails && (bool)u.IsActive && u.Email != null && u.Role==1)
                .Select(u => u.Email)
                .ToList();
                if (response == null)
                {
                    return;
                }

                if (Convert.ToDecimal(response.Balance) <= limitBalance)
                {
                    var body = $@"
                <html>
                <body style='font-family:Arial,sans-serif;font-size:14px;'>
                    <p>Dear Team,</p>
                    <p>The Twilio account <strong>{acc.Name}</strong> (SID: {acc.AccountSid}) currently has a balance of 
                    <strong>{response.Balance} USD</strong>.</p>                  
                    <p>Thank you,<br/>
                    EventPro Notification Service</p>
                </body>
                </html>";
                    try
                    {

                        foreach (var email in recipients)
                        {
                            await _emailSender.SendEmailAsync(email,
                            $"Low Twilio Balance for {acc.Name}", body);
                        }
                    }
                    catch (Exception emailEx)
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<BalanceResponse?> GetBalanceAsync(string accountSid, string authToken)
        {
            try
            {
                if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
                    return null;

                var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Balance.json";

                using var client = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes($"{accountSid}:{authToken}");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                // deserialize Twilio's response
                using var doc = JsonDocument.Parse(json);
                var balance = doc.RootElement.GetProperty("balance").GetString();
                var currency = doc.RootElement.GetProperty("currency").GetString();

                var balanceResponse = new BalanceResponse
                {
                    Balance = balance,
                    Currency = currency
                };
                return balanceResponse;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}

