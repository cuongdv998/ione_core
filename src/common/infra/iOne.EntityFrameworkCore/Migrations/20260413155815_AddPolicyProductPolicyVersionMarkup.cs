using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyProductPolicyVersionMarkup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "markup",
                table: "policy_version",
                type: "numeric(15,3)",
                nullable: true,
                comment: "Markup");

            migrationBuilder.AddColumn<decimal>(
                name: "markup",
                table: "policy_product",
                type: "numeric(15,3)",
                nullable: true,
                comment: "Markup");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "markup",
                table: "policy_version");

            migrationBuilder.DropColumn(
                name: "markup",
                table: "policy_product");
        }
    }
}
