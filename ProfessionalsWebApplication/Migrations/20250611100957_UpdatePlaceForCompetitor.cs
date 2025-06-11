using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfessionalsWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlaceForCompetitor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Place",
                table: "Competitors",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Place",
                table: "Competitors",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
