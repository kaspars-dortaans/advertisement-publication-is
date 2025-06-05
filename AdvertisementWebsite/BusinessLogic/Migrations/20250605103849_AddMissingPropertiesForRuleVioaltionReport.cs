using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingPropertiesForRuleVioaltionReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTrue",
                table: "RuleViolationReports",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionDescription",
                table: "RuleViolationReports",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTrue",
                table: "RuleViolationReports");

            migrationBuilder.DropColumn(
                name: "ResolutionDescription",
                table: "RuleViolationReports");
        }
    }
}
