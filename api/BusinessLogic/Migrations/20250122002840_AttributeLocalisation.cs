using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AttributeLocalisation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "AttributeValueLists");

            migrationBuilder.AddColumn<int>(
                name: "AttributeValueListEntryId",
                table: "LocaleTexts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttributeValueListId",
                table: "LocaleTexts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocaleTexts_AttributeValueListEntryId",
                table: "LocaleTexts",
                column: "AttributeValueListEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_LocaleTexts_AttributeValueListId",
                table: "LocaleTexts",
                column: "AttributeValueListId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocaleTexts_AttributeValueListEntries_AttributeValueListEnt~",
                table: "LocaleTexts",
                column: "AttributeValueListEntryId",
                principalTable: "AttributeValueListEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocaleTexts_AttributeValueLists_AttributeValueListId",
                table: "LocaleTexts",
                column: "AttributeValueListId",
                principalTable: "AttributeValueLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocaleTexts_AttributeValueListEntries_AttributeValueListEnt~",
                table: "LocaleTexts");

            migrationBuilder.DropForeignKey(
                name: "FK_LocaleTexts_AttributeValueLists_AttributeValueListId",
                table: "LocaleTexts");

            migrationBuilder.DropIndex(
                name: "IX_LocaleTexts_AttributeValueListEntryId",
                table: "LocaleTexts");

            migrationBuilder.DropIndex(
                name: "IX_LocaleTexts_AttributeValueListId",
                table: "LocaleTexts");

            migrationBuilder.DropColumn(
                name: "AttributeValueListEntryId",
                table: "LocaleTexts");

            migrationBuilder.DropColumn(
                name: "AttributeValueListId",
                table: "LocaleTexts");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AttributeValueLists",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
