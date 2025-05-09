using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingDbSetsInContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Advertisements_AdvertisementId",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatUser_AspNetUsers_UserId",
                table: "ChatUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatUser_Chat_ChatId",
                table: "ChatUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chat_ChatId",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatUser",
                table: "ChatUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat",
                table: "Chat");

            migrationBuilder.RenameTable(
                name: "ChatUser",
                newName: "ChatUsers");

            migrationBuilder.RenameTable(
                name: "Chat",
                newName: "Chats");

            migrationBuilder.RenameIndex(
                name: "IX_ChatUser_UserId",
                table: "ChatUsers",
                newName: "IX_ChatUsers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_AdvertisementId",
                table: "Chats",
                newName: "IX_Chats_AdvertisementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatUsers",
                table: "ChatUsers",
                columns: new[] { "ChatId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chats",
                table: "Chats",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Advertisements_AdvertisementId",
                table: "Chats",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatUsers_AspNetUsers_UserId",
                table: "ChatUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatUsers_Chats_ChatId",
                table: "ChatUsers",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Advertisements_AdvertisementId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatUsers_AspNetUsers_UserId",
                table: "ChatUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatUsers_Chats_ChatId",
                table: "ChatUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatUsers",
                table: "ChatUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chats",
                table: "Chats");

            migrationBuilder.RenameTable(
                name: "ChatUsers",
                newName: "ChatUser");

            migrationBuilder.RenameTable(
                name: "Chats",
                newName: "Chat");

            migrationBuilder.RenameIndex(
                name: "IX_ChatUsers_UserId",
                table: "ChatUser",
                newName: "IX_ChatUser_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Chats_AdvertisementId",
                table: "Chat",
                newName: "IX_Chat_AdvertisementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatUser",
                table: "ChatUser",
                columns: new[] { "ChatId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat",
                table: "Chat",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Advertisements_AdvertisementId",
                table: "Chat",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatUser_AspNetUsers_UserId",
                table: "ChatUser",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatUser_Chat_ChatId",
                table: "ChatUser",
                column: "ChatId",
                principalTable: "Chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chat_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "Chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
