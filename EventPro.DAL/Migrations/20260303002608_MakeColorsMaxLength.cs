using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MakeColorsMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ForegroundColor",
                table: "CardInfo",
                type: "nvarchar(max)",
                unicode: false,
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldUnicode: false,
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BackgroundColor",
                table: "CardInfo",
                type: "nvarchar(max)",
                unicode: false,
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldUnicode: false,
                oldMaxLength: 40,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ForegroundColor",
                table: "CardInfo",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldUnicode: false,
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BackgroundColor",
                table: "CardInfo",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldUnicode: false,
                oldMaxLength: 40,
                oldNullable: true);
        }
    }
}
