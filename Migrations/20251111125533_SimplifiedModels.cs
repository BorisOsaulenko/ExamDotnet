using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace hw.Migrations
{
    /// <inheritdoc />
    public partial class SimplifiedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0ed4cb2b-6df6-4d79-9ffa-065e1576f102");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fcaed87d-ffba-4d8c-a9f0-0cf1d2f4b4de");

            migrationBuilder.DropColumn(
                name: "FavoriteAuthors",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "UserUserPreferences",
                columns: table => new
                {
                    FavoriteAuthorsId = table.Column<string>(type: "text", nullable: false),
                    FavoriteAuthorsOfId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUserPreferences", x => new { x.FavoriteAuthorsId, x.FavoriteAuthorsOfId });
                    table.ForeignKey(
                        name: "FK_UserUserPreferences_AspNetUsers_FavoriteAuthorsId",
                        column: x => x.FavoriteAuthorsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserUserPreferences_UserPreferences_FavoriteAuthorsOfId",
                        column: x => x.FavoriteAuthorsOfId,
                        principalTable: "UserPreferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserUserPreferences_FavoriteAuthorsOfId",
                table: "UserUserPreferences",
                column: "FavoriteAuthorsOfId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserUserPreferences");

            migrationBuilder.AddColumn<string>(
                name: "FavoriteAuthors",
                table: "UserPreferences",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0ed4cb2b-6df6-4d79-9ffa-065e1576f102", "2c5d50a6-7508-4673-91e7-b2257852e5f8", "User", "USER" },
                    { "fcaed87d-ffba-4d8c-a9f0-0cf1d2f4b4de", "1d3b51b6-6a2e-42fc-bd8b-f6d4d1e83a6f", "Admin", "ADMIN" }
                });
        }
    }
}
