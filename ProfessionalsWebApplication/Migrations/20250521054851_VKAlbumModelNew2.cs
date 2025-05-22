using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfessionalsWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class VKAlbumModelNew2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "VKAlbums");

            migrationBuilder.AddColumn<string>(
                name: "AlbumId",
                table: "VKAlbums",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "VKAlbums",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlbumId",
                table: "VKAlbums");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "VKAlbums");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "VKAlbums",
                type: "TEXT",
                nullable: true);
        }
    }
}
