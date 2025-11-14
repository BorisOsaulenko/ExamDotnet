using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hw.Migrations
{
    /// <inheritdoc />
    public partial class TypoFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserUserPreferences_UserPreferences_FavoriteAuthorsOfId",
                table: "UserUserPreferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserUserPreferences",
                table: "UserUserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_UserUserPreferences_FavoriteAuthorsOfId",
                table: "UserUserPreferences");

            migrationBuilder.RenameColumn(
                name: "FavoriteAuthorsOfId",
                table: "UserUserPreferences",
                newName: "FavoriteAuthorOfId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ImageMetadata",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserUserPreferences",
                table: "UserUserPreferences",
                columns: new[] { "FavoriteAuthorOfId", "FavoriteAuthorsId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserUserPreferences_FavoriteAuthorsId",
                table: "UserUserPreferences",
                column: "FavoriteAuthorsId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserUserPreferences_UserPreferences_FavoriteAuthorOfId",
                table: "UserUserPreferences",
                column: "FavoriteAuthorOfId",
                principalTable: "UserPreferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserUserPreferences_UserPreferences_FavoriteAuthorOfId",
                table: "UserUserPreferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserUserPreferences",
                table: "UserUserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_UserUserPreferences_FavoriteAuthorsId",
                table: "UserUserPreferences");

            migrationBuilder.RenameColumn(
                name: "FavoriteAuthorOfId",
                table: "UserUserPreferences",
                newName: "FavoriteAuthorsOfId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ImageMetadata",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserUserPreferences",
                table: "UserUserPreferences",
                columns: new[] { "FavoriteAuthorsId", "FavoriteAuthorsOfId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserUserPreferences_FavoriteAuthorsOfId",
                table: "UserUserPreferences",
                column: "FavoriteAuthorsOfId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserUserPreferences_UserPreferences_FavoriteAuthorsOfId",
                table: "UserUserPreferences",
                column: "FavoriteAuthorsOfId",
                principalTable: "UserPreferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
