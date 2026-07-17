using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProductPlanDefinition_RemoveCodeUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_pro_product_plan_definition_plan_code",
                table: "pro_product_plan_definition");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_pro_product_plan_definition_plan_code",
                table: "pro_product_plan_definition",
                column: "plan_code",
                unique: true);
        }
    }
}
