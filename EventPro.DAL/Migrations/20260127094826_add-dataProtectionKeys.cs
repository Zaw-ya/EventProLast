using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class adddataProtectionKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "vwGuestInfo");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            //migrationBuilder.CreateTable(
            //    name: "vwGuestInfo",
            //    columns: table => new
            //    {
            //        AdditionalText = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ConguratulationMsgDelivered = table.Column<bool>(type: "bit", nullable: true),
            //        ConguratulationMsgFailed = table.Column<bool>(type: "bit", nullable: true),
            //        ConguratulationMsgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ConguratulationMsgRead = table.Column<bool>(type: "bit", nullable: true),
            //        ConguratulationMsgSent = table.Column<bool>(type: "bit", nullable: true),
            //        CreatedBy = table.Column<int>(type: "int", nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Cypertext = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        EventId = table.Column<int>(type: "int", nullable: true),
            //        EventLocationDelivered = table.Column<bool>(type: "bit", nullable: true),
            //        EventLocationFailed = table.Column<bool>(type: "bit", nullable: true),
            //        EventLocationRead = table.Column<bool>(type: "bit", nullable: true),
            //        EventLocationSent = table.Column<bool>(type: "bit", nullable: true),
            //        FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        GateKeeper = table.Column<int>(type: "int", nullable: true),
            //        GuestArchieved = table.Column<bool>(type: "bit", nullable: false),
            //        GuestId = table.Column<int>(type: "int", nullable: false),
            //        ImgDelivered = table.Column<bool>(type: "bit", nullable: true),
            //        ImgFailed = table.Column<bool>(type: "bit", nullable: true),
            //        ImgRead = table.Column<bool>(type: "bit", nullable: true),
            //        ImgSent = table.Column<bool>(type: "bit", nullable: true),
            //        ImgSentMsgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IsPhoneNumberValid = table.Column<bool>(type: "bit", nullable: true),
            //        LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        MessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ModeOfCommunication = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        NoOfMembers = table.Column<int>(type: "int", nullable: true),
            //        PrimaryContactNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ReminderMessageDelivered = table.Column<bool>(type: "bit", nullable: true),
            //        ReminderMessageFailed = table.Column<bool>(type: "bit", nullable: true),
            //        ReminderMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ReminderMessageRead = table.Column<bool>(type: "bit", nullable: true),
            //        ReminderMessageSent = table.Column<bool>(type: "bit", nullable: true),
            //        ReminderMessageWatiId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Scanned = table.Column<int>(type: "int", nullable: true),
            //        SecondaryContactNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        TextDelivered = table.Column<bool>(type: "bit", nullable: true),
            //        TextFailed = table.Column<bool>(type: "bit", nullable: true),
            //        TextRead = table.Column<bool>(type: "bit", nullable: true),
            //        TextSent = table.Column<bool>(type: "bit", nullable: true),
            //        WaresponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        WatiConguratulationMsgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        WhatsappStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        waMessageEventLocationForSendingToAll = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        whatsappMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        whatsappMessageImgId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        whatsappWatiEventLocationId = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //    });
        }
    }
}
