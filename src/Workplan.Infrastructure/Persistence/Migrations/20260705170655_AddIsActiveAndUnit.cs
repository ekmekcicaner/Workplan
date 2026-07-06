using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workplan.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveAndUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WorkItemTypes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Locations",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "DailyPlans",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CrewRegions",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WorkItemTypes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "DailyPlans");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CrewRegions");
        }
    }
}
