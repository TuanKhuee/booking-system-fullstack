using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomService.Migrations
{
    /// <inheritdoc />
    public partial class AddImagesAndLocationToRoomType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "RoomTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "RoomTypes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "RoomTypes");
        }
    }
}
