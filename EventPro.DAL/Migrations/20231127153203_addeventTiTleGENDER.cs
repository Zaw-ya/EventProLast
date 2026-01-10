using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace EventPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addeventTiTleGENDER : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "GKEventHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GK_Id = table.Column<int>(type: "int", nullable: false),
                    Event_Id = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogDT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GKEventHistory", x => x.Id);
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
                    LockedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    PreferedTimeZone = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: true),
                    BankAccountNo = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    IBNNumber = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    BankName = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4CDDBA614D", x => x.UserId);
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
                name: "EventCategory",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    Icon = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true)
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
                    EventLocationId = table.Column<int>(type: "int", nullable: true),
                    AttendanceTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShowOnCalender = table.Column<bool>(type: "bit", nullable: true),
                    EventCode = table.Column<int>(type: "int", nullable: true),
                    EventTitle = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    EventFrom = table.Column<DateTime>(type: "datetime", nullable: true),
                    EventTo = table.Column<DateTime>(type: "datetime", nullable: true),
                    EventVenue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GMapCode = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    EventDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedFor = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: true),
                    Status = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    ParentTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ParentTitleGender = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
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
                name: "Guest",
                columns: table => new
                {
                    GuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    Response = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
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
                name: "IX_CardInfo_EventId",
                table: "CardInfo",
                column: "EventId");

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
                name: "CardInfo");

            migrationBuilder.DropTable(
                name: "EventGatekeeperMapping");

            migrationBuilder.DropTable(
                name: "EventLocations");

            migrationBuilder.DropTable(
                name: "GKEventHistory");

            migrationBuilder.DropTable(
                name: "InvoiceDetails");

            migrationBuilder.DropTable(
                name: "LocallizationMaster");

            migrationBuilder.DropTable(
                name: "ScanHistory");

            migrationBuilder.DropTable(
                name: "SeriLog");

            migrationBuilder.DropTable(
                name: "ValidateQRCodeResult");

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
                name: "Roles");
        }
    }
}
