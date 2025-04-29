using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvertisementBookmarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdvertisementBookmarks",
                columns: table => new
                {
                    BookmarkedAdvertisementId = table.Column<int>(type: "integer", nullable: false),
                    BookmarkOwnerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementBookmarks", x => new { x.BookmarkOwnerId, x.BookmarkedAdvertisementId });
                    table.ForeignKey(
                        name: "FK_AdvertisementBookmarks_Advertisements_BookmarkedAdvertiseme~",
                        column: x => x.BookmarkedAdvertisementId,
                        principalTable: "Advertisements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvertisementBookmarks_AspNetUsers_BookmarkOwnerId",
                        column: x => x.BookmarkOwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementBookmarks_BookmarkedAdvertisementId",
                table: "AdvertisementBookmarks",
                column: "BookmarkedAdvertisementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvertisementBookmarks");
        }
    }
}
