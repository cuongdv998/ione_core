using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProductCoverageLevel_AddConditionalScript : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "conditional_script",
                table: "pro_product_coverage_level",
                type: "TEXT",
                nullable: true,
                comment: "Script điều kiện");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "conditional_script",
                table: "pro_product_coverage_level");
        }
    }
}
