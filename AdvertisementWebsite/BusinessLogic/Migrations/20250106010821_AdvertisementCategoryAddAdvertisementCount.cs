using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AdvertisementCategoryAddAdvertisementCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContainsAdvertisements",
                table: "Categories",
                newName: "CanContainAdvertisements");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "Files",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "AdvertisementCount",
                table: "Categories",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvertisementCount",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "CanContainAdvertisements",
                table: "Categories",
                newName: "ContainsAdvertisements");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "Files",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5);
        }
    }
}
