using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ChangeClaimPlanItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_plan_claimfolderitemid",
                table: "claim_folder_item_plan");

            migrationBuilder.AddColumn<decimal>(
                name: "adjuster_amount",
                table: "claim_folder_item_plan",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "approved_amount",
                table: "claim_folder_item_plan",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "depreciation_amount",
                table: "claim_folder_item_plan",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "depreciation_percent",
                table: "claim_folder_item_plan",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                table: "claim_folder_item_plan",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_percent",
                table: "claim_folder_item_plan",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "partner_amount",
                table: "claim_folder_item_plan",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "plan_type",
                table: "claim_folder_item_plan",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_amount",
                table: "claim_folder_item_plan",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_claim_folder_item_plan_item_plan_type",
                table: "claim_folder_item_plan",
                columns: new[] { "claimfolderitemid", "claimplanid", "plan_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_claim_folder_item_plan_item_plan_type",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "adjuster_amount",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "approved_amount",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "depreciation_amount",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "depreciation_percent",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "discount_percent",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "partner_amount",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "plan_type",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "total_amount",
                table: "claim_folder_item_plan");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_claimfolderitemid",
                table: "claim_folder_item_plan",
                column: "claimfolderitemid");
        }
    }
}
