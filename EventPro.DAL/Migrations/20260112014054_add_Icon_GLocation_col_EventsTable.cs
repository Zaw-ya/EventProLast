using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class add_Icon_GLocation_col_EventsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Glocation",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Glocation",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Events");
        }
    }
}
