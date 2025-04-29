using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributeName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryNameLocaleTexts_Categories_CategoryId",
                table: "CategoryNameLocaleTexts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryNameLocaleTexts",
                table: "CategoryNameLocaleTexts");

            migrationBuilder.RenameTable(
                name: "CategoryNameLocaleTexts",
                newName: "LocaleTexts");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryNameLocaleTexts_CategoryId",
                table: "LocaleTexts",
                newName: "IX_LocaleTexts_CategoryId");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "LocaleTexts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AttributeId",
                table: "LocaleTexts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "LocaleTexts",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocaleTexts",
                table: "LocaleTexts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_LocaleTexts_AttributeId",
                table: "LocaleTexts",
                column: "AttributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocaleTexts_Attributes_AttributeId",
                table: "LocaleTexts",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocaleTexts_Categories_CategoryId",
                table: "LocaleTexts",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocaleTexts_Attributes_AttributeId",
                table: "LocaleTexts");

            migrationBuilder.DropForeignKey(
                name: "FK_LocaleTexts_Categories_CategoryId",
                table: "LocaleTexts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocaleTexts",
                table: "LocaleTexts");

            migrationBuilder.DropIndex(
                name: "IX_LocaleTexts_AttributeId",
                table: "LocaleTexts");

            migrationBuilder.DropColumn(
                name: "AttributeId",
                table: "LocaleTexts");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "LocaleTexts");

            migrationBuilder.RenameTable(
                name: "LocaleTexts",
                newName: "CategoryNameLocaleTexts");

            migrationBuilder.RenameIndex(
                name: "IX_LocaleTexts_CategoryId",
                table: "CategoryNameLocaleTexts",
                newName: "IX_CategoryNameLocaleTexts_CategoryId");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "CategoryNameLocaleTexts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryNameLocaleTexts",
                table: "CategoryNameLocaleTexts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryNameLocaleTexts_Categories_CategoryId",
                table: "CategoryNameLocaleTexts",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
