using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryNameLocaleTextEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Categories");

            migrationBuilder.CreateTable(
                name: "CategoryNameLocaleTexts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Locale = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryNameLocaleTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryNameLocaleTexts_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryNameLocaleTexts_CategoryId",
                table: "CategoryNameLocaleTexts",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryNameLocaleTexts");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
