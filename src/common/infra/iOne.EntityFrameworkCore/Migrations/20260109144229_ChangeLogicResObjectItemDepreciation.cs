using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLogicResObjectItemDepreciation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_res_object_item_depreciation_res_car_group_car_group_id",
                table: "res_object_item_depreciation",
                column: "car_group_id",
                principalTable: "res_car_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_res_object_item_depreciation_res_car_group_car_group_id",
                table: "res_object_item_depreciation");
        }
    }
}
