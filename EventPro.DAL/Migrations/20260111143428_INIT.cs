using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class INIT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateKeeperCheckNotificationsNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GateKeeperReminderPeriodForEvent = table.Column<int>(type: "int", nullable: false),
                    WhatsappServiceProvider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WhatsappDefaultTwilioProfile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BulkSendingLimit = table.Column<int>(type: "int", nullable: false),
                    NumberOfOpertorToSendBulkOnSameTime = table.Column<int>(type: "int", nullable: false),
                    NumberOfWebHookRequestsDbCanHandleOnSameTime = table.Column<int>(type: "int", nullable: false),
                    TwilioBalanceEmailAlertThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DefaultWhatsappSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageTextBox = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendMessageButton = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaOptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageOption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendImageButton = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageTextBox = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoTextBox = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddNewChatButton = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SearchNewChatButton = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewContactButton = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultWhatsappSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Governorate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventsStatsByGK",
                columns: table => new
                {
                    EventTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventVenue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Scanned = table.Column<int>(type: "int", nullable: true),
                    TotalAllocated = table.Column<int>(type: "int", nullable: true),
                    GmapCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Eventlocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeaveTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttendanceTime = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "GuestsDeliveredourServiceMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestsDeliveredourServiceMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocallizationMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabelName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    RegionCode = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Translation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocallizationMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MobileLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GKId = table.Column<long>(type: "bigint", nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    QrCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiResponse = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportDeletedEventsByGk",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    GatekeeperId = table.Column<int>(type: "int", nullable: false),
                    UnassignedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnassignedById = table.Column<int>(type: "int", nullable: false),
                    UnassignedByName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDeletedEventsByGk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScanSummary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    LinkedEvent = table.Column<long>(type: "bigint", nullable: true),
                    EventTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemEventTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalGuests = table.Column<int>(type: "int", nullable: true),
                    Allowed = table.Column<int>(type: "int", nullable: false),
                    Declined = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SeriLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TwilioProfileSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountSid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessagingServiceSid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WhatsAppNumberSaudi1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsAppNumberSaudi2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsAppNumberKuwait1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsAppNumberKuwait2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsAppNumberBahrain1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsAppNumberBahrain2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderTextAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderImageAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderVideoAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderTextAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderImageAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderVideoAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderTextAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderImageAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderVideoAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderTextAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderImageAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderVideoAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicFemaleWithHeaderImageAndHeaderTextAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderTextAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderImageAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderVideoAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderTextAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderImageAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderVideoAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderTextAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderImageAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderVideoAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderTextAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderImageAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderVideoAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmArabicMaleWithHeaderImageAndHeaderTextAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderTextAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderImageAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderVideoAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderImageAndHeaderTextAndWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderTextAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderImageAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderVideoAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderImageAndHeaderTextAndWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderTextAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderImageAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderVideoAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderImageAndHeaderTextAndWithoutGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderTextAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderImageAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderVideoAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmEnglishWithHeaderImageAndHeaderTextAndWithGuestNameWithLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArabicCardWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArabicCardWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglihsCardWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishCardWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomThanksWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomThanksWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp8 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp9 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp10 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomThanksWithoutGuestNameWithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomThanksWithGuestNameWithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp1WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp2WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp3WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp4WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp5WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp6WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp7WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp8WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp9WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTemp10WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArabicCongratulationMessageToEventOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishCongratulationMessageToEventOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArabicDuplicateAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishDuplicateAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArabicEventLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishEventLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArabicDecline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomReminderWithoutGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomReminderWithGuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp1WithCalenderIcs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp2WithCalenderIcs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp3WithCalenderIcs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomReminderWithoutGuestNameWithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomReminderWithGuestNameWithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp1WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp1WithHeaderImageWithCalenderIcs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp2WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp2WithHeaderImageWithCalenderIcs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp3WithHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTemp3WithHeaderImageWithCalenderIcs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarketingInterestMsg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarketingInterestMsgWithImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarketingNotInterestMsg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarketingNotInterestMsgWithImage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwilioProfileSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValidateQRCodeResult",
                columns: table => new
                {
                    succeed = table.Column<bool>(type: "bit", nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    No = table.Column<int>(type: "int", nullable: false),
                    Scanned = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VMDashboardCount",
                columns: table => new
                {
                    CategoryCount = table.Column<int>(type: "int", nullable: false),
                    EventsCount = table.Column<int>(type: "int", nullable: false),
                    UsersCount = table.Column<int>(type: "int", nullable: false),
                    GatekeeperCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "vw_ConfirmationReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    EventTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemEventTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedEvent = table.Column<long>(type: "bigint", nullable: true),
                    YesResponse = table.Column<int>(type: "int", nullable: true),
                    NoResponse = table.Column<int>(type: "int", nullable: true),
                    WaitingResponse = table.Column<int>(type: "int", nullable: true),
                    TotalConfirmedGuests = table.Column<int>(type: "int", nullable: true),
                    TextSent = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "vwGuestInfo",
                columns: table => new
                {
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    GuestArchieved = table.Column<bool>(type: "bit", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryContactNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryContactNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModeOfCommunication = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoOfMembers = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WaresponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GateKeeper = table.Column<int>(type: "int", nullable: true),
                    MessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImgSentMsgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextSent = table.Column<bool>(type: "bit", nullable: true),
                    TextDelivered = table.Column<bool>(type: "bit", nullable: true),
                    TextRead = table.Column<bool>(type: "bit", nullable: true),
                    TextFailed = table.Column<bool>(type: "bit", nullable: true),
                    ImgFailed = table.Column<bool>(type: "bit", nullable: true),
                    ImgSent = table.Column<bool>(type: "bit", nullable: true),
                    ImgDelivered = table.Column<bool>(type: "bit", nullable: true),
                    ImgRead = table.Column<bool>(type: "bit", nullable: true),
                    Cypertext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsappStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    whatsappMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    whatsappMessageImgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    waMessageEventLocationForSendingToAll = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    whatsappWatiEventLocationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventLocationSent = table.Column<bool>(type: "bit", nullable: true),
                    EventLocationDelivered = table.Column<bool>(type: "bit", nullable: true),
                    EventLocationRead = table.Column<bool>(type: "bit", nullable: true),
                    EventLocationFailed = table.Column<bool>(type: "bit", nullable: true),
                    ConguratulationMsgSent = table.Column<bool>(type: "bit", nullable: true),
                    ConguratulationMsgDelivered = table.Column<bool>(type: "bit", nullable: true),
                    ConguratulationMsgRead = table.Column<bool>(type: "bit", nullable: true),
                    ConguratulationMsgFailed = table.Column<bool>(type: "bit", nullable: true),
                    ConguratulationMsgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WatiConguratulationMsgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderMessageWatiId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderMessageSent = table.Column<bool>(type: "bit", nullable: true),
                    ReminderMessageDelivered = table.Column<bool>(type: "bit", nullable: true),
                    ReminderMessageRead = table.Column<bool>(type: "bit", nullable: true),
                    ReminderMessageFailed = table.Column<bool>(type: "bit", nullable: true),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scanned = table.Column<int>(type: "int", nullable: true),
                    IsPhoneNumberValid = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VwScanLogs",
                columns: table => new
                {
                    ScanId = table.Column<int>(type: "int", nullable: false),
                    ScanBy = table.Column<int>(type: "int", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    ScannedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScannedCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestId = table.Column<int>(type: "int", nullable: true),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScannedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nos = table.Column<int>(type: "int", nullable: true),
                    EventTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemEventTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedFor = table.Column<int>(type: "int", nullable: true),
                    LinkedEvent = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "WhatsappResponseLogs",
                columns: table => new
                {
                    WaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WAKey = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Response = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Ticks = table.Column<long>(type: "bigint", nullable: true),
                    RecepientNo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Whatsapp__81F5B101D6599776", x => x.WaId);
                });

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.Id);
                    table.ForeignKey(
                        name: "FK_City_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Password = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    Gender = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryContactNo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    SecondaryContantNo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    ModeOfCommunication = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    LoginAttempt = table.Column<int>(type: "int", nullable: true),
                    TemporaryPass = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    Approved = table.Column<bool>(type: "bit", nullable: true),
                    LockedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    PreferedTimeZone = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: true),
                    BankAccountNo = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    IBNNumber = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    BankName = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    SendNotificationsOrEmails = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4CDDBA614D", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Users__CreatedBy__3A81B327",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Users__ModifiedB__3B75D760",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Users__Role__4F7CD00D",
                        column: x => x.Role,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BulkOperatorEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorAssignedFromId = table.Column<int>(type: "int", nullable: false),
                    OperatorAssignedToId = table.Column<int>(type: "int", nullable: false),
                    AssignedById = table.Column<int>(type: "int", nullable: false),
                    AssignedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkOperatorEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulkOperatorEvents_Users_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_BulkOperatorEvents_Users_OperatorAssignedFromId",
                        column: x => x.OperatorAssignedFromId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_BulkOperatorEvents_Users_OperatorAssignedToId",
                        column: x => x.OperatorAssignedToId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ConfirmationMessageResponsesKeyword",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeywordKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    KeywordValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmationMessageResponsesKeyword", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfirmationMessageResponsesKeyword_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfirmationMessageResponsesKeyword_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "EventCategory",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EventCat__7944C810DB34F5B1", x => x.EventId);
                    table.ForeignKey(
                        name: "FK__EventCate__Creat__412EB0B6",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomConfirmationTemplateWithVariables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomCardTemplateWithVariables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomReminderTemplateWithVariables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomCongratulationTemplateWithVariables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmationButtonsType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventLocationId = table.Column<int>(type: "int", nullable: true),
                    AttendanceTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShowOnCalender = table.Column<bool>(type: "bit", nullable: true),
                    WhatsappConfirmation = table.Column<bool>(type: "bit", nullable: true),
                    WhatsappPush = table.Column<bool>(type: "bit", nullable: true),
                    ShowFailedSendingEventLocationLink = table.Column<bool>(type: "bit", nullable: true),
                    ShowFailedSendingCongratulationLink = table.Column<bool>(type: "bit", nullable: true),
                    FailedSendingConfiramtionMessagesLinksLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendingConfiramtionMessagesLinksLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventCode = table.Column<int>(type: "int", nullable: true),
                    EventTitle = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    SystemEventTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    EventFrom = table.Column<DateTime>(type: "datetime", nullable: true),
                    EventTo = table.Column<DateTime>(type: "datetime", nullable: true),
                    EventVenue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MessageHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageHeaderText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GMapCode = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    EventDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedFor = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: true),
                    Status = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    ParentTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ParentTitleGender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedEvent = table.Column<long>(type: "bigint", nullable: true),
                    SendInvitation = table.Column<bool>(type: "bit", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    LeaveTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsappProviderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomInvitationMessageTemplateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomCardInvitationTemplateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardInvitationTemplateType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConguratulationsMsgSentOnNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConguratulationsMsgType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConguratulationsMsgTemplateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendingType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderMessageTempName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderTempId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanksTempId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclineTempId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailedGuestsMessag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailedGuestsCardText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkGuestsCardText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailedGuestsLocationEmbedSrc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkGuestsLocationEmbedSrc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailedGuestsReminderMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailedGuestsCongratulationMsg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderMsgHeaderImg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CongratulationMsgHeaderImg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChoosenNumberWithinCountry = table.Column<int>(type: "int", nullable: false),
                    choosenSendingWhatsappProfile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    choosenSendingCountryNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseInterestedOfMarketingMsg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseInterestedOfMarketingMsgHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseNotInterestedOfMarketingMsg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseNotInterestedOfMarketingMsgHeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Events__CreatedB__44FF419A",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Events__CreatedF__45F365D3",
                        column: x => x.CreatedFor,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Events__Modified__46E78A0C",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Events__Type__440B1D61",
                        column: x => x.Type,
                        principalTable: "EventCategory",
                        principalColumn: "EventId");
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<int>(type: "int", nullable: true),
                    RelatedId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLog_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CardInfo",
                columns: table => new
                {
                    CardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    BackgroundImage = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    BarcodeXAxis = table.Column<double>(type: "float", nullable: true),
                    BarcodeYAxis = table.Column<double>(type: "float", nullable: true),
                    BarcodeWidth = table.Column<int>(type: "int", nullable: true),
                    BarcodeHeight = table.Column<int>(type: "int", nullable: true),
                    BarcodeColorCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    ContactNameXAxis = table.Column<double>(type: "float", nullable: true),
                    ContactNameYAxis = table.Column<double>(type: "float", nullable: true),
                    FontName = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    FontSize = table.Column<double>(type: "float", nullable: true),
                    FontColor = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    ContactNoXAxis = table.Column<double>(type: "float", nullable: true),
                    ContactNoYAxis = table.Column<double>(type: "float", nullable: true),
                    ContactNoFontName = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    ContactNoFontSize = table.Column<double>(type: "float", nullable: true),
                    ContactNoFontColor = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    AltTextXAxis = table.Column<double>(type: "float", nullable: true),
                    AltTextYAxis = table.Column<double>(type: "float", nullable: true),
                    AltTextFontName = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    AltTextFontSize = table.Column<double>(type: "float", nullable: true),
                    AltTextFontColor = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NOSXAxis = table.Column<double>(type: "float", nullable: true),
                    NOSYAxis = table.Column<double>(type: "float", nullable: true),
                    NOSFontName = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    NOSFontSize = table.Column<double>(type: "float", nullable: true),
                    NOSFontColor = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    CardWidth = table.Column<int>(type: "int", nullable: true),
                    CardHeight = table.Column<int>(type: "int", nullable: true),
                    BackgroundColor = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    ForegroundColor = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    TransparentBackground = table.Column<bool>(type: "bit", nullable: true),
                    DefaultFont = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    SelectedPlaceHolder = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    FontAlignment = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    AddTextFontAlignment = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    ContactNoAlignment = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    NosAlignment = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    NameRightAxis = table.Column<double>(type: "float", nullable: true),
                    ContactRightAxis = table.Column<double>(type: "float", nullable: true),
                    NosRightAxis = table.Column<double>(type: "float", nullable: true),
                    AddTextRightAxis = table.Column<double>(type: "float", nullable: true),
                    BarcodeBorder = table.Column<bool>(type: "bit", nullable: true),
                    RightAlignment = table.Column<bool>(type: "bit", nullable: true),
                    FontStyleName = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    FontStyleMobNo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    FontStyleAddText = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    FontStyleNos = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CardInfo__55FECDAE4B958A2D", x => x.CardId);
                    table.ForeignKey(
                        name: "FK__CardInfo__EventI__160F4887",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventGatekeeperMapping",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    GatekeeperId = table.Column<int>(type: "int", nullable: true),
                    AsssignedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    AssignedBy = table.Column<int>(type: "int", nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EventGat__7C6949B1EFFB8CEC", x => x.TaskId);
                    table.ForeignKey(
                        name: "FK__EventGate__Assig__51300E55",
                        column: x => x.AssignedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__EventGate__Event__4F47C5E3",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__EventGate__Gatek__503BEA1C",
                        column: x => x.GatekeeperId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "EventOperator",
                columns: table => new
                {
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    EventStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BulkOperatroEventsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventOperator", x => new { x.OperatorId, x.EventId });
                    table.ForeignKey(
                        name: "FK_EventOperator_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventOperator_Users_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GKEventHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GK_Id = table.Column<int>(type: "int", nullable: false),
                    Event_Id = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogDT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    latitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    longitude = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GKEventHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GKEventHistory_Events_Event_Id",
                        column: x => x.Event_Id,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GKEventHistory_Users_GK_Id",
                        column: x => x.GK_Id,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Guest",
                columns: table => new
                {
                    GuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestArchieved = table.Column<bool>(type: "bit", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PrimaryContactNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SecondaryContactNo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    EmailAddress = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    ModeOfCommunication = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NoOfMembers = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Source = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    GateKeeper = table.Column<int>(type: "int", nullable: true),
                    AdditionalText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cypertext = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    WhatsappStatus = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    MessageId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    WASentOn = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    CountryCode = table.Column<int>(type: "int", nullable: true),
                    ImgSentMsgId = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    ImgSenOn = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: true),
                    WAResponseTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    MsgResponse = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    QRResponse = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    TextSent = table.Column<bool>(type: "bit", nullable: true),
                    TextDelivered = table.Column<bool>(type: "bit", nullable: true),
                    TextRead = table.Column<bool>(type: "bit", nullable: true),
                    ImgSent = table.Column<bool>(type: "bit", nullable: true),
                    ImgDelivered = table.Column<bool>(type: "bit", nullable: true),
                    ImgRead = table.Column<bool>(type: "bit", nullable: true),
                    TextFailed = table.Column<bool>(type: "bit", nullable: true),
                    ImgFailed = table.Column<bool>(type: "bit", nullable: true),
                    Response = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    whatsappMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    whatsappMessageImgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    whatsappMessageEventLocationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    waMessageEventLocationForSendingToAll = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventLocationSent = table.Column<bool>(type: "bit", nullable: true),
                    EventLocationDelivered = table.Column<bool>(type: "bit", nullable: true),
                    EventLocationRead = table.Column<bool>(type: "bit", nullable: true),
                    EventLocationFailed = table.Column<bool>(type: "bit", nullable: true),
                    whatsappWatiEventLocationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConguratulationMsgCount = table.Column<int>(type: "int", nullable: false),
                    ConguratulationMsgSent = table.Column<bool>(type: "bit", nullable: true),
                    ConguratulationMsgDelivered = table.Column<bool>(type: "bit", nullable: true),
                    ConguratulationMsgRead = table.Column<bool>(type: "bit", nullable: true),
                    ConguratulationMsgFailed = table.Column<bool>(type: "bit", nullable: true),
                    ConguratulationMsgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WatiConguratulationMsgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConguratulationMsgLinkId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderMessageWatiId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReminderMessageSent = table.Column<bool>(type: "bit", nullable: true),
                    ReminderMessageDelivered = table.Column<bool>(type: "bit", nullable: true),
                    ReminderMessageRead = table.Column<bool>(type: "bit", nullable: true),
                    ReminderMessageFailed = table.Column<bool>(type: "bit", nullable: true),
                    IsPhoneNumberValid = table.Column<bool>(type: "bit", nullable: true),
                    YesButtonId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoButtonId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventLocationButtonId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guest", x => x.GuestId);
                    table.ForeignKey(
                        name: "FK__Guest__CreatedBy__2DE6D218",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Guest__EventId__2EDAF651",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Guest__GateKeepe__2FCF1A8A",
                        column: x => x.GateKeeper,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "date", nullable: true),
                    DueDate = table.Column<DateTime>(type: "date", nullable: true),
                    BillTo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    BillingAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BillingContactNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EventCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    EventLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EventPlace = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxPer = table.Column<decimal>(type: "money", nullable: true),
                    TotalDue = table.Column<decimal>(type: "money", nullable: true),
                    NetDue = table.Column<decimal>(type: "money", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Invoices__EventI__1C873BEC",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScanHistory",
                columns: table => new
                {
                    ScanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanBy = table.Column<int>(type: "int", nullable: true),
                    ScannedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    ScannedCode = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    GuestId = table.Column<int>(type: "int", nullable: true),
                    ResponseCode = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Response = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ScanHist__63B326812054FE6F", x => x.ScanId);
                    table.ForeignKey(
                        name: "FK__ScanHisto__Guest__3D2915A8",
                        column: x => x.GuestId,
                        principalTable: "Guest",
                        principalColumn: "GuestId");
                    table.ForeignKey(
                        name: "FK__ScanHisto__ScanB__3C34F16F",
                        column: x => x.ScanBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDetails",
                columns: table => new
                {
                    IDId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    Product = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NoFGuest = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Rate = table.Column<decimal>(type: "money", nullable: true),
                    Qty = table.Column<decimal>(type: "money", nullable: true),
                    Total = table.Column<decimal>(type: "money", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__InvoiceD__B87DF1C449EDCD95", x => x.IDId);
                    table.ForeignKey(
                        name: "FK__InvoiceDe__Invoi__1F63A897",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EventId",
                table: "AuditLog",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId",
                table: "AuditLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BulkOperatorEvents_AssignedById",
                table: "BulkOperatorEvents",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_BulkOperatorEvents_OperatorAssignedFromId",
                table: "BulkOperatorEvents",
                column: "OperatorAssignedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_BulkOperatorEvents_OperatorAssignedToId",
                table: "BulkOperatorEvents",
                column: "OperatorAssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_CardInfo_EventId",
                table: "CardInfo",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_City_CountryId",
                table: "City",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationMessageResponsesKeyword_CreatedBy",
                table: "ConfirmationMessageResponsesKeyword",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationMessageResponsesKeyword_UpdatedBy",
                table: "ConfirmationMessageResponsesKeyword",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EventCategory_CreatedBy",
                table: "EventCategory",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EventGatekeeperMapping_AssignedBy",
                table: "EventGatekeeperMapping",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EventGatekeeperMapping_EventId",
                table: "EventGatekeeperMapping",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventGatekeeperMapping_GatekeeperId",
                table: "EventGatekeeperMapping",
                column: "GatekeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_EventOperator_EventId",
                table: "EventOperator",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CityId",
                table: "Events",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatedBy",
                table: "Events",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatedFor",
                table: "Events",
                column: "CreatedFor");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ModifiedBy",
                table: "Events",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Type",
                table: "Events",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_GKEventHistory_Event_Id",
                table: "GKEventHistory",
                column: "Event_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GKEventHistory_GK_Id",
                table: "GKEventHistory",
                column: "GK_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Guest_CreatedBy",
                table: "Guest",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Guest_EventId",
                table: "Guest",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Guest_GateKeeper",
                table: "Guest",
                column: "GateKeeper");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_InvoiceId",
                table: "InvoiceDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_EventId",
                table: "Invoices",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "UQ__Invoices__640F671637A0326E",
                table: "Invoices",
                column: "EventCode",
                unique: true,
                filter: "[EventCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__8A2B6160339910C6",
                table: "Roles",
                column: "RoleName",
                unique: true,
                filter: "[RoleName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScanHistory_GuestId",
                table: "ScanHistory",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanHistory_ScanBy",
                table: "ScanHistory",
                column: "ScanBy");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CityId",
                table: "Users",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedBy",
                table: "Users",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ModifiedBy",
                table: "Users",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__C9F2845664DD6561",
                table: "Users",
                column: "UserName",
                unique: true,
                filter: "[UserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "BulkOperatorEvents");

            migrationBuilder.DropTable(
                name: "CardInfo");

            migrationBuilder.DropTable(
                name: "ConfirmationMessageResponsesKeyword");

            migrationBuilder.DropTable(
                name: "DefaultWhatsappSettings");

            migrationBuilder.DropTable(
                name: "EventGatekeeperMapping");

            migrationBuilder.DropTable(
                name: "EventLocations");

            migrationBuilder.DropTable(
                name: "EventOperator");

            migrationBuilder.DropTable(
                name: "EventsStatsByGK");

            migrationBuilder.DropTable(
                name: "GKEventHistory");

            migrationBuilder.DropTable(
                name: "GuestsDeliveredourServiceMessage");

            migrationBuilder.DropTable(
                name: "InvoiceDetails");

            migrationBuilder.DropTable(
                name: "LocallizationMaster");

            migrationBuilder.DropTable(
                name: "MobileLog");

            migrationBuilder.DropTable(
                name: "ReportDeletedEventsByGk");

            migrationBuilder.DropTable(
                name: "ScanHistory");

            migrationBuilder.DropTable(
                name: "ScanSummary");

            migrationBuilder.DropTable(
                name: "SeriLog");

            migrationBuilder.DropTable(
                name: "TwilioProfileSettings");

            migrationBuilder.DropTable(
                name: "ValidateQRCodeResult");

            migrationBuilder.DropTable(
                name: "VMDashboardCount");

            migrationBuilder.DropTable(
                name: "vw_ConfirmationReport");

            migrationBuilder.DropTable(
                name: "vwGuestInfo");

            migrationBuilder.DropTable(
                name: "VwScanLogs");

            migrationBuilder.DropTable(
                name: "WhatsappResponseLogs");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Guest");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "EventCategory");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Country");
        }
    }
}
