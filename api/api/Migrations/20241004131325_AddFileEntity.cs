using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddFileEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdevertisementAttributeValues_Advertisements_Advertisementid",
                table: "AdevertisementAttributeValues");

            migrationBuilder.DropForeignKey(
                name: "FK_AdevertisementAttributeValues_Attributes_AttributeId",
                table: "AdevertisementAttributeValues");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Advertisements_Advertisementid",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_AspNetUsers_OwnerUserId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permisssions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permisssions",
                table: "Permisssions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdevertisementAttributeValues",
                table: "AdevertisementAttributeValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Images",
                table: "Images");

            migrationBuilder.RenameTable(
                name: "Permisssions",
                newName: "Permissions");

            migrationBuilder.RenameTable(
                name: "AdevertisementAttributeValues",
                newName: "AdvertisementAttributeValues");

            migrationBuilder.RenameTable(
                name: "Images",
                newName: "Files");

            migrationBuilder.RenameColumn(
                name: "Advertisementid",
                table: "AdvertisementAttributeValues",
                newName: "AdvertisementId");

            migrationBuilder.RenameIndex(
                name: "IX_AdevertisementAttributeValues_AttributeId",
                table: "AdvertisementAttributeValues",
                newName: "IX_AdvertisementAttributeValues_AttributeId");

            migrationBuilder.RenameIndex(
                name: "IX_AdevertisementAttributeValues_Advertisementid",
                table: "AdvertisementAttributeValues",
                newName: "IX_AdvertisementAttributeValues_AdvertisementId");

            migrationBuilder.RenameColumn(
                name: "Advertisementid",
                table: "Files",
                newName: "AdvertisementId");

            migrationBuilder.RenameIndex(
                name: "IX_Images_OwnerUserId",
                table: "Files",
                newName: "IX_Files_OwnerUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Images_Advertisementid",
                table: "Files",
                newName: "IX_Files_AdvertisementId");

            migrationBuilder.AlterColumn<int>(
                name: "AdvertisementId",
                table: "Files",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdvertisementAttributeValues",
                table: "AdvertisementAttributeValues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                table: "Files",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementAttributeValues_Advertisements_AdvertisementId",
                table: "AdvertisementAttributeValues",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementAttributeValues_Attributes_AttributeId",
                table: "AdvertisementAttributeValues",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Advertisements_AdvertisementId",
                table: "Files",
                column: "AdvertisementId",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_AspNetUsers_OwnerUserId",
                table: "Files",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementAttributeValues_Advertisements_AdvertisementId",
                table: "AdvertisementAttributeValues");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementAttributeValues_Attributes_AttributeId",
                table: "AdvertisementAttributeValues");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Advertisements_AdvertisementId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_AspNetUsers_OwnerUserId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdvertisementAttributeValues",
                table: "AdvertisementAttributeValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Files");

            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "Permisssions");

            migrationBuilder.RenameTable(
                name: "AdvertisementAttributeValues",
                newName: "AdevertisementAttributeValues");

            migrationBuilder.RenameTable(
                name: "Files",
                newName: "Images");

            migrationBuilder.RenameColumn(
                name: "AdvertisementId",
                table: "AdevertisementAttributeValues",
                newName: "Advertisementid");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementAttributeValues_AttributeId",
                table: "AdevertisementAttributeValues",
                newName: "IX_AdevertisementAttributeValues_AttributeId");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementAttributeValues_AdvertisementId",
                table: "AdevertisementAttributeValues",
                newName: "IX_AdevertisementAttributeValues_Advertisementid");

            migrationBuilder.RenameColumn(
                name: "AdvertisementId",
                table: "Images",
                newName: "Advertisementid");

            migrationBuilder.RenameIndex(
                name: "IX_Files_OwnerUserId",
                table: "Images",
                newName: "IX_Images_OwnerUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_AdvertisementId",
                table: "Images",
                newName: "IX_Images_Advertisementid");

            migrationBuilder.AlterColumn<int>(
                name: "Advertisementid",
                table: "Images",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permisssions",
                table: "Permisssions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdevertisementAttributeValues",
                table: "AdevertisementAttributeValues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Images",
                table: "Images",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdevertisementAttributeValues_Advertisements_Advertisementid",
                table: "AdevertisementAttributeValues",
                column: "Advertisementid",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdevertisementAttributeValues_Attributes_AttributeId",
                table: "AdevertisementAttributeValues",
                column: "AttributeId",
                principalTable: "Attributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Advertisements_Advertisementid",
                table: "Images",
                column: "Advertisementid",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_AspNetUsers_OwnerUserId",
                table: "Images",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permisssions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "Permisssions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
