using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace hw.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageAllowedUsers_Images_ImageId",
                table: "ImageAllowedUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageCollections_Images_CoverImageId",
                table: "ImageCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageComments_Images_ImageId",
                table: "ImageComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_AspNetUsers_UserId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_ImageCollections_ImageCollectionId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageStats_Images_ImageId",
                table: "ImageStats");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageTags_Images_ImageId",
                table: "ImageTags");

            migrationBuilder.DropIndex(
                name: "IX_ImageStats_ImageId",
                table: "ImageStats");

            migrationBuilder.DropIndex(
                name: "IX_Images_ImageCollectionId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "SearchQuery",
                table: "UserConsumerHistories");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "ImageStats");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "ImageStats");

            migrationBuilder.DropColumn(
                name: "Likes",
                table: "ImageStats");

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ContainerName",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ImageCollectionId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "Images");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                table: "ImageTags",
                newName: "ImageMetadataId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageTags_ImageId_Tag",
                table: "ImageTags",
                newName: "IX_ImageTags_ImageMetadataId_Tag");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Images",
                newName: "BlobName");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                table: "ImageComments",
                newName: "ImageStatsId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageComments_ImageId",
                table: "ImageComments",
                newName: "IX_ImageComments_ImageStatsId");

            migrationBuilder.RenameColumn(
                name: "CoverImageId",
                table: "ImageCollections",
                newName: "CoverImageMetadataId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageCollections_CoverImageId",
                table: "ImageCollections",
                newName: "IX_ImageCollections_CoverImageMetadataId");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                table: "ImageAllowedUsers",
                newName: "ImageMetadataId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageAllowedUsers_ImageId_UserId",
                table: "ImageAllowedUsers",
                newName: "IX_ImageAllowedUsers_ImageMetadataId_UserId");

            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "UserProducerHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "UserProducerHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousCollectionAccessLevel",
                table: "UserProducerHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousCoverImageId",
                table: "UserProducerHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousImageLocation",
                table: "UserProducerHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BlobUri",
                table: "Images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.CreateTable(
                name: "ImageMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImageId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    AccessLevel = table.Column<int>(type: "integer", nullable: false),
                    ImageCollectionId = table.Column<int>(type: "integer", nullable: true),
                    ImageStatsId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageMetadata_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageMetadata_ImageCollections_ImageCollectionId",
                        column: x => x.ImageCollectionId,
                        principalTable: "ImageCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ImageMetadata_ImageStats_ImageStatsId",
                        column: x => x.ImageStatsId,
                        principalTable: "ImageStats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageMetadata_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageStatsUserPreferences",
                columns: table => new
                {
                    LikedByUsersId = table.Column<int>(type: "integer", nullable: false),
                    LikedImagesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageStatsUserPreferences", x => new { x.LikedByUsersId, x.LikedImagesId });
                    table.ForeignKey(
                        name: "FK_ImageStatsUserPreferences_ImageStats_LikedImagesId",
                        column: x => x.LikedImagesId,
                        principalTable: "ImageStats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageStatsUserPreferences_UserPreferences_LikedByUsersId",
                        column: x => x.LikedByUsersId,
                        principalTable: "UserPreferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0ed4cb2b-6df6-4d79-9ffa-065e1576f102", "2c5d50a6-7508-4673-91e7-b2257852e5f8", "User", "USER" },
                    { "fcaed87d-ffba-4d8c-a9f0-0cf1d2f4b4de", "1d3b51b6-6a2e-42fc-bd8b-f6d4d1e83a6f", "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageMetadata_ImageCollectionId",
                table: "ImageMetadata",
                column: "ImageCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageMetadata_ImageId",
                table: "ImageMetadata",
                column: "ImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageMetadata_ImageStatsId",
                table: "ImageMetadata",
                column: "ImageStatsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageMetadata_UserId",
                table: "ImageMetadata",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageStatsUserPreferences_LikedImagesId",
                table: "ImageStatsUserPreferences",
                column: "LikedImagesId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageAllowedUsers_ImageMetadata_ImageMetadataId",
                table: "ImageAllowedUsers",
                column: "ImageMetadataId",
                principalTable: "ImageMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageCollections_ImageMetadata_CoverImageMetadataId",
                table: "ImageCollections",
                column: "CoverImageMetadataId",
                principalTable: "ImageMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageComments_ImageStats_ImageStatsId",
                table: "ImageComments",
                column: "ImageStatsId",
                principalTable: "ImageStats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_AspNetUsers_UserId",
                table: "Images",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageTags_ImageMetadata_ImageMetadataId",
                table: "ImageTags",
                column: "ImageMetadataId",
                principalTable: "ImageMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageAllowedUsers_ImageMetadata_ImageMetadataId",
                table: "ImageAllowedUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageCollections_ImageMetadata_CoverImageMetadataId",
                table: "ImageCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageComments_ImageStats_ImageStatsId",
                table: "ImageComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_AspNetUsers_UserId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageTags_ImageMetadata_ImageMetadataId",
                table: "ImageTags");

            migrationBuilder.DropTable(
                name: "ImageMetadata");

            migrationBuilder.DropTable(
                name: "ImageStatsUserPreferences");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0ed4cb2b-6df6-4d79-9ffa-065e1576f102");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fcaed87d-ffba-4d8c-a9f0-0cf1d2f4b4de");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "UserProducerHistories");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "UserProducerHistories");

            migrationBuilder.DropColumn(
                name: "PreviousCollectionAccessLevel",
                table: "UserProducerHistories");

            migrationBuilder.DropColumn(
                name: "PreviousCoverImageId",
                table: "UserProducerHistories");

            migrationBuilder.DropColumn(
                name: "PreviousImageLocation",
                table: "UserProducerHistories");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ImageMetadataId",
                table: "ImageTags",
                newName: "ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageTags_ImageMetadataId_Tag",
                table: "ImageTags",
                newName: "IX_ImageTags_ImageId_Tag");

            migrationBuilder.RenameColumn(
                name: "BlobName",
                table: "Images",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "ImageStatsId",
                table: "ImageComments",
                newName: "ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageComments_ImageStatsId",
                table: "ImageComments",
                newName: "IX_ImageComments_ImageId");

            migrationBuilder.RenameColumn(
                name: "CoverImageMetadataId",
                table: "ImageCollections",
                newName: "CoverImageId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageCollections_CoverImageMetadataId",
                table: "ImageCollections",
                newName: "IX_ImageCollections_CoverImageId");

            migrationBuilder.RenameColumn(
                name: "ImageMetadataId",
                table: "ImageAllowedUsers",
                newName: "ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageAllowedUsers_ImageMetadataId_UserId",
                table: "ImageAllowedUsers",
                newName: "IX_ImageAllowedUsers_ImageId_UserId");

            migrationBuilder.AddColumn<string>(
                name: "SearchQuery",
                table: "UserConsumerHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Comments",
                table: "ImageStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "ImageStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "ImageStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Images",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BlobUri",
                table: "Images",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "Images",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContainerName",
                table: "Images",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Images",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAt",
                table: "Images",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ImageCollectionId",
                table: "Images",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Images",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "Images",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ImageStats_ImageId",
                table: "ImageStats",
                column: "ImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_ImageCollectionId",
                table: "Images",
                column: "ImageCollectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageAllowedUsers_Images_ImageId",
                table: "ImageAllowedUsers",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageCollections_Images_CoverImageId",
                table: "ImageCollections",
                column: "CoverImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageComments_Images_ImageId",
                table: "ImageComments",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_AspNetUsers_UserId",
                table: "Images",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_ImageCollections_ImageCollectionId",
                table: "Images",
                column: "ImageCollectionId",
                principalTable: "ImageCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageStats_Images_ImageId",
                table: "ImageStats",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageTags_Images_ImageId",
                table: "ImageTags",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
