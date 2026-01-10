using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;
using EventPro.Services.WatiService.Interface;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EventPro.Services.WatiService.Implementation
{
    public class WatiService : IWatiService
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        public WatiService(IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
        }
        public async Task<string> SendArabicFemaleInvitaionTemplate(Guest guest, Events events)
        {

            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"female_template7\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendArabicFemaleInvitaionTemplateWithHeaderImage(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var imageUrl = imagePath + @"/" + events.MessageHeaderImage;
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"broadcast_name\":\"Media\",\"template_name\":\"female_template_with_header_image6\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"broadcast_name\":\"Media\",\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendArabicFemaleInvitaionTemplateWithHeaderImageAndHeaderText(Guest guest, Events events)
        {
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var imageUrl = imagePath + @"/" + events.MessageHeaderImage;
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"broadcast_name\":\"Media\",\"template_name\":\"female_template_with_header_image_and_header_6\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"broadcast_name\":\"Media\",\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendArabicFemaleInvitaionTemplateWithHeaderText(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"female_with_header_text6\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendArabicMaleInvitaionTemplate(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"male_template6\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendArabicMaleInvitaionTemplateWithHeaderImage(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var imageUrl = imagePath + @"/" + events.MessageHeaderImage;
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"broadcast_name\":\"Media\",\"template_name\":\"male_template_with_header_image6\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"broadcast_name\":\"Media\",\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendArabicMaleInvitaionTemplateWithHeaderImageAndHeaderText(Guest guest, Events events)
        {
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var imageUrl = imagePath + @"/" + events.MessageHeaderImage;
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"broadcast_name\":\"Media\",\"template_name\":\"male_template_with_header_image_and_header_6\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"broadcast_name\":\"Media\",\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendArabicMaleInvitaionTemplateWithHeaderText(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"male_with_header_text6\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"parameters\":[{\"name\":\"2\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"3\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }






        public async Task<string> SendEICHHOLTZInvitaionTemplate(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"template_name\":\"custom1254\",\"broadcast_name\":\"Media\"}", false);

            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }


        public async Task<string> SendCardInvitaionTemplate(Guest guest, Events events)
        {
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var url = imagePath + "/" + events.Id + @"/" + "E00000" + events.Id + "_" + guest.GuestId + @"_" + guest.NoOfMembers + ".png";
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomCardInvitationTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"custom_invitation_25\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"downloadrul\",\"value\":\"" + url + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomCardInvitationTemplateName + "\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"downloadrul\",\"value\":\"" + url + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            if (responseData.result == "true")
            {
                guestDB.ImgSent = true;
                guestDB.whatsappMessageImgId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }



        public async Task<string> SendCustomInvitaionTemplate(Guest guest, Events events)
        {
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"custom45\",\"broadcast_name\":\"None\"}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"None\"}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }



        public async Task<string> SendDuplicateAnswerMessageTemplate(Guest guest, Events events)
        {
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v1/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"template_name\":\"duplicate_answer_message_new_design4\",\"broadcast_name\":\"None\"}", false);
            var response = await client.PostAsync(request);
            return "sent";
        }

        public async Task<string> SendEnglishCardInvitaionTemplate(Guest guest, Events events)
        {
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var url = imagePath + "/" + events.Id + @"/" + "E00000" + events.Id + "_" + guest.GuestId + @"_" + guest.NoOfMembers + ".png";
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomCardInvitationTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"english_card_25\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"downloadrul\",\"value\":\"" + url + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomCardInvitationTemplateName + "\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"downloadrul\",\"value\":\"" + url + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);

            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            if (responseData.result == "true")
            {
                guestDB.ImgSent = true;
                guestDB.whatsappMessageImgId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendEnglishDuplicateAnswerMessageTemplate(Guest guest, Events events)
        {
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v1/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"template_name\":\"english_duplicate_message_2\",\"broadcast_name\":\"None\"}", false);
            var response = await client.PostAsync(request);
            return "sent";
        }

        public async Task<string> SendEnglishEventLocationTemplate(Guest guest, Events events)
        {
            string gmapCode = "https://maps.app.goo.gl/" + events.GmapCode;
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"template_name\":\"english_eventlocation_message\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + gmapCode + "\"}]}", false);
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            if (responseData.result == "true")
            {
                guestDB.EventLocationSent = true;
                guestDB.whatsappWatiEventLocationId = "notNull";
            }
            await db.SaveChangesAsync();
            return responseData.receivers[0].localmessageid;
        }



        public async Task<string> SendEnglishInvitaionTemplate(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            //  request.AddJsonBody("{\"template_name\":\"english_invitation_new_design2\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"3\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + evntDate.ToString("dddd") + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"6\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"english_invitation_template\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"3\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + evntDate.ToString("dddd") + " \"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"6\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"3\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + evntDate.ToString("dddd") + " \"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"6\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendEnglishInvitaionTemplateWihtHeaderTextAndHeaderImage(Guest guest, Events events)
        {
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var imageUrl = imagePath + @"/" + events.MessageHeaderImage;
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"english_invitation_template_with_headr_text_and_header_image\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"3\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + evntDate.ToString("dddd") + " \"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"6\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"},{\"name\":\"7\",\"value\":\"" + events.MessageHeaderText + "\"}]}", false);

            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"3\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + evntDate.ToString("dddd") + " \"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"6\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"},{\"name\":\"7\",\"value\":\"" + events.MessageHeaderText + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendEnglishInvitaionTemplateWithHeaderImage(Guest guest, Events events)
        {
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var imageUrl = imagePath + @"/" + events.MessageHeaderImage;
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"english_invitation_template_with_header_image\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"3\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + evntDate.ToString("dddd") + " \"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"6\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"3\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + evntDate.ToString("dddd") + " \"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"6\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendEnglishInvitaionTemplateWithHeaderText(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"english_invitation_template_with_header_text\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"3\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + evntDate.ToString("dddd") + " \"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"6\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"7\",\"value\":\"" + events.MessageHeaderText + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"3\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + evntDate.ToString("dddd") + " \"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"6\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"7\",\"value\":\"" + events.MessageHeaderText + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }



        public async Task<string> SendEventLocationTemplate(Guest guest, Events events)
        {
            string gmapCode = "https://maps.app.goo.gl/" + events.GmapCode;
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"template_name\":\"event_location_new_design_7\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + gmapCode + "\"}]}", false);
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            if (responseData.result == "true")
            {
                guestDB.EventLocationSent = true;
                guestDB.whatsappWatiEventLocationId = "notNull";
            }
            await db.SaveChangesAsync();
            return responseData.receivers[0].localmessageid;
        }

        public Task<string> SendGateKeeperCheckInImageTemplate(Guest guest, Events events)
        {
            throw new NotImplementedException();
        }

        public Task<string> SendGateKeeperCheckInTemplate(Guest guest, Events events)
        {
            throw new NotImplementedException();
        }

        public Task<string> SendGateKeeperCheckOutTemplate(Guest guest, Events events)
        {
            throw new NotImplementedException();
        }

        public async Task<string> SendEventProServiceTemplate(string phoneNumber)
        {

            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + phoneNumber);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"template_name\":\"EventPro_service_1_11_2024\",\"broadcast_name\":\"Media\"}", false);
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendSaudiFoodSaveInvitaionTemplate(Guest guest, Events events)
        {
            string imagePath2 = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            string url = imagePath2 + events.Id + @"/" + "E00000" + events.Id + "_" + guest.GuestId + @"_" + guest.NoOfMembers + ".png";
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            // request.AddJsonBody("{\"template_name\":\"female_invitation_new_design2\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + Convert.ToString(events.ParentTitle) + "\"},{\"name\":\"3\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"4\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"6\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            request.AddJsonBody("{\"template_name\":\"custom23\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"}]}", false);

            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            await db.SaveChangesAsync();
            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendUserAccountDetailsTemplate(Users user)
        {
            string androidUrl = _configuration.GetSection("PinacleSettings").GetSection("AppAndroidUrl").Value;
            string iosUrl = _configuration.GetSection("PinacleSettings").GetSection("AppIosUrl").Value;
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + user.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"template_name\":\"cred_account\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + user.UserName + "\"},{\"name\":\"2\",\"value\":\"" + user.Password + "\"},{\"name\":\"3\",\"value\":\"" + androidUrl + "\"},{\"name\":\"4\",\"value\":\"" + iosUrl + "\"}]}", false);
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            string msg = string.Empty;

            if (responseData.result == "true")
            {
                msg = "Message Processed Successfully";
            }

            return msg;
        }

        public async Task<string> SendWorkInvitationTemplate(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"work_template5\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            }

            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendWorkInvitationTemplateWithHeaderImage(Guest guest, Events events)
        {
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var imageUrl = imagePath + @"/" + events.MessageHeaderImage;
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"work_template_with_header_image4\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendWorkInvitationTemplateWithHeaderText(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"work_template_with_header_text4\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendWorkInvitationTemplateWithHeaderTextAndHeaderImage(Guest guest, Events events)
        {
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var imageUrl = imagePath + @"/" + events.MessageHeaderImage;
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"work_template_wiht_header_text_and_header_image4\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"4\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"5\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"6\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"7\",\"value\":\"" + events.EventVenue + "\"},{\"name\":\"1\",\"value\":\"" + events.MessageHeaderText + "\"},{\"name\":\"product_image_url\",\"value\":\"" + imageUrl + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendQRInvitaionTemplate(Guest guest, Events events)
        {
            string imagePath2 = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            string url = imagePath2 + events.Id + @"/" + "E00000" + events.Id + "_" + guest.GuestId + @"_" + guest.NoOfMembers + ".png";
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"custom23\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"1\",\"value\":\"" + guest.FirstName + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);

            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            if (responseData.result == "true")
            {
                guestDB.ImgSent = true;
                guestDB.whatsappMessageImgId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendCustomCardWithClientNameInvitaionTemplate(Guest guest, Events events)
        {
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var url = imagePath + "/" + events.Id + @"/" + "E00000" + events.Id + "_" + guest.GuestId + @"_" + guest.NoOfMembers + ".png";
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomCardInvitationTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"custom_with_variable_name_25\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"downloadrul\",\"value\":\"" + url + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomCardInvitationTemplateName + "\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"downloadrul\",\"value\":\"" + url + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            if (responseData.result == "true")
            {
                guestDB.ImgSent = true;
                guestDB.whatsappMessageImgId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendEnglishCustomCardWithClientNameInvitaionTemplate(Guest guest, Events events)
        {
            string imagePath = _configuration.GetSection("InterkatSettings").GetSection("ApplicationUrl").Value;
            var url = imagePath + "/" + events.Id + @"/" + "E00000" + events.Id + "_" + guest.GuestId + @"_" + guest.NoOfMembers + ".png";
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomCardInvitationTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"custom_with_variable_name_25\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"downloadrul\",\"value\":\"" + url + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomCardInvitationTemplateName + "\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"product_image_url\",\"value\":\"" + url + "\"},{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"downloadrul\",\"value\":\"" + url + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            if (responseData.result == "true")
            {
                guestDB.ImgSent = true;
                guestDB.whatsappMessageImgId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }



        public async Task<string> SendCustomInvitaionWithClientNameTemplate(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.CustomInvitationMessageTemplateName == null)
            {
                request.AddJsonBody("{\"template_name\":\"custom_invitation_with_client_name\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.CustomInvitationMessageTemplateName + "\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.Response = "Message Processed Successfully";
            if (responseData.result == "true")
            {
                guestDB.TextSent = true;
                guestDB.whatsappMessageId = "notNull";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendCongratulationMessageTemplate(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            var conguratulationId = Guid.NewGuid().ToString();
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.ConguratulationsMsgTemplateName != null)
            {
                if (events.ConguratulationsMsgTemplateName == "Custom")
                {
                    if (events.ThanksTempId == null)
                    {
                        request.AddJsonBody("{\"template_name\":\"custom_cong_msg_2\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + events.ThanksMessage + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"}]}", false);
                    }
                    else
                    {
                        request.AddJsonBody("{\"template_name\":\"" + events.ThanksTempId + "\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"}]}", false);
                    }

                }
                else if (events.ConguratulationsMsgTemplateName == "Custom With Guest name | wati")
                {
                    if (events.ThanksTempId == null)
                    {
                        request.AddJsonBody("{\"template_name\":\"custom_cong_with_guest_name_2\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + events.ThanksMessage + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"}]}", false);
                    }
                    else
                    {
                        request.AddJsonBody("{\"template_name\":\"" + events.ThanksTempId + "\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"}]}", false);
                    }
                }
                else if (events.ConguratulationsMsgTemplateName == "Template (1)")
                {
                    request.AddJsonBody("{\"template_name\":\"cong_temp_11\",\"broadcast_name\":\"Media\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"}]}", false);
                }
                else if (events.ConguratulationsMsgTemplateName == "Template (2)")
                {
                    request.AddJsonBody("{\"template_name\":\"cong_temp_22\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"parentname\",\"value\":\"" + events.ParentTitle + "\"}]}", false);
                }
                else if (events.ConguratulationsMsgTemplateName == "Template (3)")
                {
                    request.AddJsonBody("{\"template_name\":\"cong_temp_33\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"parentname\",\"value\":\"" + events.ParentTitle + "\"}]}", false);
                }
                else if (events.ConguratulationsMsgTemplateName == "Template (4)")
                {
                    request.AddJsonBody("{\"template_name\":\"cong_temp_44\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"parentname\",\"value\":\"" + events.ParentTitle + "\"}]}", false);
                }
                else if (events.ConguratulationsMsgTemplateName == "Template (5)")
                {
                    request.AddJsonBody("{\"template_name\":\"cong_temp_55\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"parentname\",\"value\":\"" + events.ParentTitle + "\"}]}", false);
                }
                else if (events.ConguratulationsMsgTemplateName == "Template (6)")
                {
                    request.AddJsonBody("{\"template_name\":\"cong_temp_66\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"}]}", false);
                }
                else if (events.ConguratulationsMsgTemplateName == "Template (7)")
                {
                    request.AddJsonBody("{\"template_name\":\"cong_temp_77\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"parentname\",\"value\":\"" + events.ParentTitle + "\"}]}", false);
                }
                else if (events.ConguratulationsMsgTemplateName == "Template (8)")
                {
                    request.AddJsonBody("{\"template_name\":\"cong_temp_88\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"parentname\",\"value\":\"" + events.ParentTitle + "\"}]}", false);
                }
                else if (events.ConguratulationsMsgTemplateName == "Template (9)")
                {
                    request.AddJsonBody("{\"template_name\":\"cong_temp_99\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"parentname\",\"value\":\"" + events.ParentTitle + "\"}]}", false);
                }
                else if (events.ConguratulationsMsgTemplateName == "Template (10)")
                {
                    request.AddJsonBody("{\"template_name\":\"cong_temp_1010\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"id\",\"value\":\"" + conguratulationId + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"parentname\",\"value\":\"" + events.ParentTitle + "\"}]}", false);
                }
            }

            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();
            guestDB.ConguratulationMsgId = responseData.receivers[0].localmessageid;
            guestDB.ConguratulationMsgLinkId = conguratulationId;
            if (responseData.result == "true")
            {
                guestDB.ConguratulationMsgCount = 1;
                guestDB.ConguratulationMsgSent = true;
                guestDB.WatiConguratulationMsgId = "not null";
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendCongratulationMessageToPrideTemplate(Guest guest, string message)
        {
            var evntId = guest.EventId;
            var evnt = await db.Events.FirstOrDefaultAsync(e => e.Id == evntId);
            var prideNumber = evnt.ConguratulationsMsgSentOnNumber;
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + prideNumber);
            var client = new RestClient(options);
            var request = new RestRequest("");
            //  var conguratulationId = Guid.NewGuid().ToString();
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"template_name\":\"cong_test33\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"2\",\"value\":\"" + message + "\"}]}", false);
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);
            var guestDB = await db.Guest.Where(e => e.GuestId == guest.GuestId).FirstOrDefaultAsync();

            if (responseData.result == "true")
            {
                guestDB.ConguratulationMsgCount = 0;
            }
            await db.SaveChangesAsync();

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendGateKeeperReminderMessage(GKWhatsRemiderMsgModel model)
        {
            var result = string.Empty;

            foreach (var gateKeeper in model.GkDetails)
            {
                if (!string.IsNullOrEmpty(gateKeeper.GKPhoneNumber))
                {
                    var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + gateKeeper.GKPhoneNumber);
                    var client = new RestClient(options);
                    var request = new RestRequest("");
                    request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
                    request.AddJsonBody("{\"template_name\":\"gatekeeper_reminder5\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + gateKeeper.GKFName + "\"},{\"name\":\"day\",\"value\":\"" + model.AttendanceTime?.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"date\",\"value\":\"" + model.AttendanceTime?.ToString("dd/MM/yyyy") + "\"},{\"name\":\"venue\",\"value\":\"" + model.EventVenue + "\"},{\"name\":\"time\",\"value\":\"" + model.AttendanceTime?.ToString("HH:mm") + "\"},{\"name\":\"eventname\",\"value\":\"" + model.EventTitle + "\"}]}", false);
                    var response = await client.PostAsync(request);
                    var sourceResponse = response.Content.ToLower();
                    dynamic responseData = JObject.Parse(sourceResponse);
                    result = responseData.receivers[0].localmessageid;
                }
            }
            return result;
        }

        public async Task<string> SendGateKeeperTodayReminderMessage(GKWhatsRemiderMsgModel model)
        {
            var result = string.Empty;

            foreach (var gateKeeper in model.GkDetails)
            {
                var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + gateKeeper.GKPhoneNumber);
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
                request.AddJsonBody("{\"template_name\":\"gatekeeper_reminder_6\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + gateKeeper.GKFName + "\"},{\"name\":\"day\",\"value\":\"" + model.AttendanceTime?.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"date\",\"value\":\"" + model.AttendanceTime?.ToString("dd/MM/yyyy") + "\"},{\"name\":\"venue\",\"value\":\"" + model.EventVenue + "\"},{\"name\":\"time\",\"value\":\"" + model.AttendanceTime?.ToString("HH:mm") + "\"},{\"name\":\"eventname\",\"value\":\"" + model.EventTitle + "\"}]}", false);
                var response = await client.PostAsync(request);
                var sourceResponse = response.Content.ToLower();
                dynamic responseData = JObject.Parse(sourceResponse);
                result = responseData.receivers[0].localmessageid;
            }
            return result;
        }

        public async Task<string> SendCustomReminderMessageTemplate(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.ReminderTempId == null)
            {
                request.AddJsonBody("{\"template_name\":\"custom_reminder_msg\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"1\",\"value\":\"" + events.ReminderMessage + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.ReminderTempId + "\",\"broadcast_name\":\"None\"}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendCustomReminderMessageWithGuesttNameTemplate(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            if (events.ReminderTempId == null)
            {
                request.AddJsonBody("{\"template_name\":\"custom_reminder_msg_with_guest_name\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"1\",\"value\":\"" + events.ReminderMessage + "\"}]}", false);
            }
            else
            {
                request.AddJsonBody("{\"template_name\":\"" + events.ReminderTempId + "\",\"broadcast_name\":\"None\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"}]}", false);
            }
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendReminderMessageTemplate1(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"reminder_msg_temp1_utility\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"day\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"date\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"location\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendReminderMessageTemplate2(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"reminder_msg_temp2_utility\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"day\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"parentname\",\"value\":\"" + events.ParentTitle + "\"},{\"name\":\"date\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"location\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);

            return responseData.receivers[0].localmessageid;
        }

        public async Task<string> SendReminderMessageTemplate3(Guest guest, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var options = new RestClientOptions("https://live-mt-server.wati.io/305517/api/v2/sendTemplateMessage?whatsappNumber=" + guest.SecondaryContactNo + guest.PrimaryContactNo);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIzNDI2OTk2Ni02ODU1LTQ3N2YtOTIxNS1iMTE0NzljZTg4YjEiLCJ1bmlxdWVfbmFtZSI6ImNzQG15aW52aXRlLm9yZyIsIm5hbWVpZCI6ImNzQG15aW52aXRlLm9yZyIsImVtYWlsIjoiY3NAbXlpbnZpdGUub3JnIiwiYXV0aF90aW1lIjoiMDIvMjgvMjAyNCAxOTozMzo1NCIsImRiX25hbWUiOiJtdC1wcm9kLVRlbmFudHMiLCJ0ZW5hbnRfaWQiOiIzMDU1MTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBRE1JTklTVFJBVE9SIiwiZXhwIjoyNTM0MDIzMDA4MDAsImlzcyI6IkNsYXJlX0FJIiwiYXVkIjoiQ2xhcmVfQUkifQ.-52r3HDJG0pgVsmulFj7Vua1iG0ok-mNE-IY55ptdF8");
            request.AddJsonBody("{\"broadcast_name\":\"None\",\"template_name\":\"reminder_msg_temp3_utility\",\"parameters\":[{\"name\":\"name\",\"value\":\"" + guest.FirstName + "\"},{\"name\":\"eventname\",\"value\":\"" + events.EventTitle + "\"},{\"name\":\"day\",\"value\":\"" + evntDate.ToString("dddd", new CultureInfo("ar-AE")) + "\"},{\"name\":\"parentname\",\"value\":\"" + events.ParentTitle + "\"},{\"name\":\"date\",\"value\":\"" + evntDate.ToString("dd/MM/yyyy") + "\"},{\"name\":\"location\",\"value\":\"" + events.EventVenue + "\"}]}", false);
            var response = await client.PostAsync(request);
            var sourceResponse = response.Content.ToLower();
            dynamic responseData = JObject.Parse(sourceResponse);

            return responseData.receivers[0].localmessageid;
        }
    }
}
