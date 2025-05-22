using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfessionalsWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeIdToCompetence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Competences",
                newName: "TypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "Competences",
                newName: "CategoryId");
        }
    }
}
