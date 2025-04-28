using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfessionalsWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class updateQuestionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Forms_FormModelId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_FormModelId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "FormModelId",
                table: "Questions");

            migrationBuilder.AddColumn<int>(
                name: "ThemeId",
                table: "Questions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ThemeId",
                table: "Questions",
                column: "ThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Forms_ThemeId",
                table: "Questions",
                column: "ThemeId",
                principalTable: "Forms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Forms_ThemeId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ThemeId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "Questions");

            migrationBuilder.AddColumn<int>(
                name: "FormModelId",
                table: "Questions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_FormModelId",
                table: "Questions",
                column: "FormModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Forms_FormModelId",
                table: "Questions",
                column: "FormModelId",
                principalTable: "Forms",
                principalColumn: "Id");
        }
    }
}
