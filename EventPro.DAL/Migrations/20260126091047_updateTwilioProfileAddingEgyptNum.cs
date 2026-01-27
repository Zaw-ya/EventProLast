using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updateTwilioProfileAddingEgyptNum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: vwGuestInfo is a VIEW, not a table - removing the drop operation
            // migrationBuilder.DropTable(name: "vwGuestInfo");

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumberEgypt1",
                table: "TwilioProfileSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumberEgypt2",
                table: "TwilioProfileSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsAppNumberEgypt1",
                table: "TwilioProfileSettings");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumberEgypt2",
                table: "TwilioProfileSettings");

            // Note: vwGuestInfo is a VIEW, not a table - removing the create operation
            // The view already exists in the database
        }
    }
}
