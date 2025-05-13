using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfessionalsWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class EncryptAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Iv",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Iv",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "Users");
        }
    }
}
