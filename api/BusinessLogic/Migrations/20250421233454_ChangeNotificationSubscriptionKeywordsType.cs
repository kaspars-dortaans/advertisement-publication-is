using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNotificationSubscriptionKeywordsType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<string[]>(
            //    name: "Keywords",
            //    table: "NotificationSubscriptions",
            //    type: "text[]",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "text",
            //    oldNullable: true);

            migrationBuilder.Sql(@"ALTER TABLE ""NotificationSubscriptions"" ALTER COLUMN ""Keywords"" TYPE text[] USING ""Keywords""::text[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<string>(
            //    name: "Keywords",
            //    table: "NotificationSubscriptions",
            //    type: "text",
            //    nullable: true,
            //    oldClrType: typeof(string[]),
            //    oldType: "text[]",
            //    oldNullable: true);

            migrationBuilder.Sql(@"ALTER TABLE ""NotificationSubscriptions"" ALTER COLUMN ""Keywords"" TYPE text USING ""Keywords""::text");
        }
    }
}
