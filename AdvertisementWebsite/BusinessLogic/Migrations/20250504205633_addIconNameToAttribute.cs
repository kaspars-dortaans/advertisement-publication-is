using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class addIconNameToAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attributes_Files_IconId",
                table: "Attributes");

            migrationBuilder.DropIndex(
                name: "IX_Attributes_IconId",
                table: "Attributes");

            migrationBuilder.DropColumn(
                name: "IconId",
                table: "Attributes");

            migrationBuilder.AddColumn<string>(
                name: "IconName",
                table: "Attributes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowOnListItem",
                table: "Attributes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconName",
                table: "Attributes");

            migrationBuilder.DropColumn(
                name: "ShowOnListItem",
                table: "Attributes");

            migrationBuilder.AddColumn<int>(
                name: "IconId",
                table: "Attributes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_IconId",
                table: "Attributes",
                column: "IconId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attributes_Files_IconId",
                table: "Attributes",
                column: "IconId",
                principalTable: "Files",
                principalColumn: "Id");
        }
    }
}
