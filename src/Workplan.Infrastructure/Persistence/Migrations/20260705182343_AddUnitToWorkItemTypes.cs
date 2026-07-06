using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workplan.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitToWorkItemTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "WorkItemTypes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "WorkItemTypes");
        }
    }
}
