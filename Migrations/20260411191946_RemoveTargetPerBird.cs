using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTargetPerBird : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetPerBird",
                table: "EggProductionSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TargetPerBird",
                table: "EggProductionSettings",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
