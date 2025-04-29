using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "Files",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);

            migrationBuilder.AddColumn<int>(
                name: "AdvertisementId",
                table: "Files",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Files",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Files_AdvertisementId",
                table: "Files",
                column: "AdvertisementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Advertisements_AdvertisementId",
                table: "Files",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Files");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "Files",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);
        }
    }
}
