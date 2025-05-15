using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfessionalsWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddedYearForChampionship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Year",
                table: "Competences");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Championships",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Year",
                table: "Championships");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Competences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
