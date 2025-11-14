using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hw.Migrations
{
    /// <inheritdoc />
    public partial class TypoFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_AspNetUsers_UserId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_UserId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "ImageMetadata",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageMetadata_UserId1",
                table: "ImageMetadata",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageMetadata_AspNetUsers_UserId1",
                table: "ImageMetadata",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageMetadata_AspNetUsers_UserId1",
                table: "ImageMetadata");

            migrationBuilder.DropIndex(
                name: "IX_ImageMetadata_UserId1",
                table: "ImageMetadata");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "ImageMetadata");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Images",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_UserId",
                table: "Images",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_AspNetUsers_UserId",
                table: "Images",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
