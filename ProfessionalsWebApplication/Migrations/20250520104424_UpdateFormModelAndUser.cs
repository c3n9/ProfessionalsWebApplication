using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfessionalsWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFormModelAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FormId",
                table: "Users",
                newName: "FormModelId");

            migrationBuilder.AddColumn<int>(
                name: "CompetenceId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_FormModelId",
                table: "Users",
                column: "FormModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Forms_FormModelId",
                table: "Users",
                column: "FormModelId",
                principalTable: "Forms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Forms_FormModelId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_FormModelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompetenceId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "FormModelId",
                table: "Users",
                newName: "FormId");
        }
    }
}
