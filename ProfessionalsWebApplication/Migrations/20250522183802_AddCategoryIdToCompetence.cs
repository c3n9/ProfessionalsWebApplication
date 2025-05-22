using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfessionalsWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToCompetence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Competences",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Competences");
        }
    }
}
