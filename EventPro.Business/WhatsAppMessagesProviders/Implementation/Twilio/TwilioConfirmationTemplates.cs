using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.Business.DataProtector;
using EventPro.Business.MemoryCacheStore.Interface;
using EventPro.Business.WhatsAppMessagesProviders.Interface;
using EventPro.DAL.Models;
using EventPro.Web.Services;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EventPro.Business.WhatsAppMessagesProviders.Implementation.Twilio
{
    public class TwilioConfirmationTemplates : TwilioMessagingConfiguration, IConfirmationMessageTemplates
    {
        private readonly EventProContext db;
        private readonly UrlProtector _urlProtector;
        public TwilioConfirmationTemplates(IConfiguration configuration,
            IMemoryCacheStoreService memoryCacheStoreService, UrlProtector urlProtector) : base(configuration,
                memoryCacheStoreService)
        {
            db = new EventProContext(configuration);
            _urlProtector = urlProtector;
        }

        public async Task SendArabicbasic(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                             .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                             .AsNoTracking()
                             .FirstOrDefaultAsync();

            if (events.ConfirmationButtonsType == "QuickReplies")
            {

                if (events.ParentTitleGender == "Female")
                {
                    templateId = profileSettings?.ConfirmArabicFemaleWithoutGuestName;
                }
                else
                {
                    templateId = profileSettings?.ConfirmArabicMaleWithoutGuestName;
                }
            }
            else
            {
                if (events.ParentTitleGender == "Female")
                {
                    templateId = profileSettings?.ConfirmArabicFemaleWithoutGuestNameWithLink;
                }
                else
                {
                    templateId = profileSettings?.ConfirmArabicMaleWithoutGuestNameWithLink;
                }
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }



        public async Task SendArabicbasicHeaderImage(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                 .AsNoTracking()
                 .FirstOrDefaultAsync();
            if (events.ConfirmationButtonsType == "QuickReplies")
            {

                if (events.ParentTitleGender == "Female")
                {
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndWithoutGuestName;
                }
                else
                {
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndWithoutGuestName;
                }
            }
            else
            {
                if (events.ParentTitleGender == "Female")
                {
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndWithoutGuestNameWithLink;
                }
                else
                {
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndWithoutGuestNameWithLink;
                }
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendArabicbasicHeaderText(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                               .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                               .AsNoTracking()
                               .FirstOrDefaultAsync();
            if (events.ConfirmationButtonsType == "QuickReplies")
            {

                if (events.ParentTitleGender == "Female")
                {
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderTextAndWithoutGuestName;
                }
                else
                {
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderTextAndWithoutGuestName;
                }
            }
            else
            {
                if (events.ParentTitleGender == "Female")
                {
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderTextAndWithoutGuestNameWithLink;
                }
                else
                {
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderTextAndWithoutGuestNameWithLink;
                }

            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendArabicbasicHeaderTextImage(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            string templateId;
            var profileSettings = await db.TwilioProfileSettings
                   .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                   .AsNoTracking()
                   .FirstOrDefaultAsync();

            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                if (events.ParentTitleGender == "Female")
                {
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithoutGuestName;
                }
                else
                {
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithoutGuestName;
                }
            }
            else
            {
                if (events.ParentTitleGender == "Female")
                {
                    templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink;
                }
                else
                {
                    templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink;
                }
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendArabicFemaleDefault(List<Guest> guests, Events events)
        {
            var profileSettings = await db.TwilioProfileSettings
                      .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                      .AsNoTracking()
                      .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmArabicFemaleWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmArabicFemaleWithGuestNameWithLink;
            }
            var evntDate = Convert.ToDateTime(events.EventFrom);
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendArabicFemaleWithHeaderImage(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var templateId = string.Empty;
            var profileSettings = await db.TwilioProfileSettings
                               .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                               .AsNoTracking()
                               .FirstOrDefaultAsync();
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                if (events.MessageHeaderImage!.ToLower().EndsWith(".mp4"))
                {
                    templateId = profileSettings.ConfirmArabicFemaleWithHeaderVideoAndWithGuestName;
                }
                else
                {
                    templateId = profileSettings.ConfirmArabicFemaleWithHeaderImageAndWithGuestName;

                }
            }
            else
            {
                if (events.MessageHeaderImage!.ToLower().EndsWith(".mp4"))
                {
                    templateId = profileSettings.ConfirmArabicFemaleWithHeaderVideoAndWithGuestNameWithLink;
                }
                else
                {
                    templateId = profileSettings.ConfirmArabicFemaleWithHeaderImageAndWithGuestNameWithLink;

                }
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {


                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendArabicFemaleWithHeaderImageAndHeaderText(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);

                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendArabicFemaleWithHeaderText(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmArabicFemaleWithHeaderTextAndWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmArabicFemaleWithHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {


                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendArabicMaleDefault(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmArabicMaleWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmArabicMaleWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendArabicMaleWithHeaderImage(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendArabicMaleWithHeaderImageAndHeaderText(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendArabicMaleWithHeaderText(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderTextAndWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmArabicMaleWithHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendbasicHeaderImageEnglish(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndWithoutGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndWithoutGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendbasicHeaderTextEnglish(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderTextAndWithoutGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderTextAndWithoutGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {


                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendbasicHeaderTextImageEnglish(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndHeaderTextAndWithoutGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCustomBasic(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCustomBasicHeaderImage(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                var parameters = new string[]
                {
                events.MessageHeaderImage.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                };

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCustomBasicHeaderText(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                var parameters = new string[]
                {
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                };

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCustomBasicHeaderTextImage(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                var parameters = new string[]
                {
                events.MessageHeaderImage.ToString(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                };

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCustomWithName(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                      .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                      .AsNoTracking()
                      .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                var parameters = new string[]
                {
                guest.FirstName.Trim(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId

                };

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCustomWithNameHeaderImage(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {


                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                var parameters = new string[]
                {
                guest.FirstName.Trim(),
                events.MessageHeaderImage.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                };

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCustomWithNameHeaderText(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                var parameters = new string[]
                {
                guest.FirstName.Trim(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                };

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCustomWithNameHeaderTextImage(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                var parameters = new string[]
                {
                guest.FirstName.Trim(),
                events.MessageHeaderImage.ToString(),
                events.MessageHeaderText.ToString(),
                yesButtonId,
                noButtonId,
                eventLocationButtonId
                };

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);

                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendEnglishbasic(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmEnglishWithoutGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmEnglishWithoutGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {

                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendEnglishDefault(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmEnglishWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmEnglishWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendEnglishWithHeaderImage(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendEnglishWithHeaderImageAndHeaderText(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndHeaderTextAndWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderImageAndHeaderTextAndWithGuestNameWithLink;
            }
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);

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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendEnglishWithHeaderText(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                 .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                 .AsNoTracking()
                 .FirstOrDefaultAsync();
            var templateId = string.Empty;
            if (events.ConfirmationButtonsType == "QuickReplies")
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderTextAndWithGuestName;
            }
            else
            {
                templateId = profileSettings?.ConfirmEnglishWithHeaderTextAndWithGuestNameWithLink;
            }

            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
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

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        public async Task SendCustomTemplateWithVariables(List<Guest> guests, Events events)
        {
            var evntDate = Convert.ToDateTime(events.EventFrom);
            var profileSettings = await db.TwilioProfileSettings
                                  .Where(e => e.Name == events.choosenSendingWhatsappProfile)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();
            var templateId = events.CustomInvitationMessageTemplateName;
            int counter = SetSendingCounter(guests, events);

            await Parallel.ForEachAsync(guests, parallelOptions, async (guest, CancellationToken) =>
            {
                string fullPhoneNumber = $"+{guest.SecondaryContactNo}{guest.PrimaryContactNo}";
                string yesButtonId = UrlEncryptionHelper.Encrypt("yesButton" + events.Id + guest.GuestId);
                string noButtonId = UrlEncryptionHelper.Encrypt("noButton" + events.Id + guest.GuestId);
                string eventLocationButtonId = UrlEncryptionHelper.Encrypt("eventLocationButton" + events.Id + guest.GuestId);
                var matches = Regex.Matches(events.CustomConfirmationTemplateWithVariables, @"\{\{(.*?)\}\}");
                List<string> templateParameters = matches
                    .Cast<Match>()
                    .Select(m =>
                    {
                        string propName = m.Groups[1].Value;
                        if(propName == "GuestCard")
                        {
                            return events.Id + "/E00000" + events.Id + "_" + guest.GuestId + "_" + guest.NoOfMembers + ".jpg";
                        }

                        if (propName == "CountOfAdditionalInvitations")
                        {
                            return (guest.NoOfMembers - 1)?.ToString() ?? "0";
                        }
                        var value = guest.GetType().GetProperty(propName)?
                                          .GetValue(guest, null)?.ToString();
                        if (value == null)
                        {
                            value = events.GetType().GetProperty(propName)?
                                          .GetValue(events, null)?.ToString();
                        }
                        return value ?? propName;
                    })
                    .ToList();

                templateParameters.Add(yesButtonId);
                templateParameters.Add(noButtonId);
                templateParameters.Add(eventLocationButtonId);
                string[] parameters = templateParameters.ToArray();

                await SendMessageAndUpdateStatus(events, templateId, guest, fullPhoneNumber, yesButtonId, noButtonId, eventLocationButtonId, parameters, guests, profileSettings);
                counter = UpdateCounter(guests, events, counter);
            });
            await updateDataBaseAndDisposeCache(guests, events);
            return;
        }

        private async Task updateDataBaseAndDisposeCache(List<Guest> guests, Events events)
        {
            db.Guest.UpdateRange(guests);
            await db.SaveChangesAsync();
            if (guests.Count > 1)
            {
                await Task.Delay(10000);
                _memoryCacheStoreService.delete(events.Id.ToString());
                foreach (var guest in guests)
                {
                    if (guest.MessageId != null)
                    {
                        _memoryCacheStoreService.delete(guest.MessageId);
                    }
                }
            }
        }

        private async Task SendMessageAndUpdateStatus(Events events, string templateId, Guest guest, string fullPhoneNumber, string yesButtonId, string noButtonId, string eventLocationButtonId, string[] parameters, List<Guest> guests, TwilioProfileSettings profileSettings)
        {
            string messageSid = await SendWhatsAppTemplateMessageAsync(fullPhoneNumber, templateId, parameters, events.CityId, events.ChoosenNumberWithinCountry, profileSettings, events.choosenSendingCountryNumber);
            if (messageSid != null)
            {
                guest.MessageId = messageSid;
                guest.Response = "Message Processed Successfully";
                guest.YesButtonId = yesButtonId;
                guest.NoButtonId = noButtonId;
                guest.EventLocationButtonId = eventLocationButtonId;
                guest.WasentOn = DateTime.Now.ToString();
                guest.TextDelivered = null;
                guest.TextRead = null;
                guest.TextSent = null;
                guest.TextFailed = null;
                guest.ConguratulationMsgId = null;
                guest.ConguratulationMsgFailed = null;
                guest.ConguratulationMsgDelivered = null;
                guest.ConguratulationMsgSent = null;
                guest.ConguratulationMsgRead = null;
                guest.ImgDelivered = null;
                guest.ImgFailed = null;
                guest.ImgRead = null;
                guest.ImgSent = null;
                guest.ImgSentMsgId = null;
                guest.WaresponseTime = null;
                guest.whatsappMessageEventLocationId = null;
                guest.EventLocationSent = null;
                guest.EventLocationRead = null;
                guest.EventLocationDelivered = null;
                guest.EventLocationFailed = null;
                guest.waMessageEventLocationForSendingToAll = null;
                guest.ReminderMessageId = null;
                guest.ReminderMessageSent = null;
                guest.ReminderMessageRead = null;
                guest.ReminderMessageDelivered = null;
                guest.ReminderMessageFailed = null;

                if (guests.Count > 1)
                {
                    _memoryCacheStoreService.save(messageSid, 0);
                }
            }
            else
            {
                guest.MessageId = null;
                guest.Response = "WA Error";
            }

        }

    }
}
