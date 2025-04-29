using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemFilesAndDeleteImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Advertisements_AdvertisementId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_AdvertisementId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AdvertisementId",
                table: "Files");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerUserId",
                table: "Files",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "Files",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5);

            migrationBuilder.AddColumn<int>(
                name: "IconId",
                table: "Attributes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_IconId",
                table: "Attributes",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_ThumbnailImageId",
                table: "Advertisements",
                column: "ThumbnailImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_Files_ThumbnailImageId",
                table: "Advertisements",
                column: "ThumbnailImageId",
                principalTable: "Files",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attributes_Files_IconId",
                table: "Attributes",
                column: "IconId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_Files_ThumbnailImageId",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_Attributes_Files_IconId",
                table: "Attributes");

            migrationBuilder.DropIndex(
                name: "IX_Attributes_IconId",
                table: "Attributes");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_ThumbnailImageId",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "IconId",
                table: "Attributes");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerUserId",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "Files",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);

            migrationBuilder.AddColumn<int>(
                name: "AdvertisementId",
                table: "Files",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_AdvertisementId",
                table: "Files",
                column: "AdvertisementId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Advertisements_AdvertisementId",
                table: "Files",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
