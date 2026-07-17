using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class clearUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_res_uom_class_code",
                table: "res_uom_class");

            migrationBuilder.DropIndex(
                name: "ix_res_uom_code",
                table: "res_uom");

            migrationBuilder.DropIndex(
                name: "ix_res_object_type_code",
                table: "res_object_type");

            migrationBuilder.DropIndex(
                name: "ix_res_object_item_type_code",
                table: "res_object_item_type");

            migrationBuilder.DropIndex(
                name: "ix_res_motor_class_code",
                table: "res_motor_class");

            migrationBuilder.DropIndex(
                name: "ix_res_damage_level_code",
                table: "res_damage_level");

            migrationBuilder.DropIndex(
                name: "ix_res_car_type_code",
                table: "res_car_type");

            migrationBuilder.DropIndex(
                name: "ix_res_car_model_code",
                table: "res_car_model");

            migrationBuilder.DropIndex(
                name: "ix_res_car_line_code",
                table: "res_car_line");

            migrationBuilder.DropIndex(
                name: "ix_res_car_group_code",
                table: "res_car_group");

            migrationBuilder.DropIndex(
                name: "ix_res_car_category_code",
                table: "res_car_category");

            migrationBuilder.DropIndex(
                name: "ix_res_car_brand_code",
                table: "res_car_brand");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_res_uom_class_code",
                table: "res_uom_class",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_uom_code",
                table: "res_uom",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_object_type_code",
                table: "res_object_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_object_item_type_code",
                table: "res_object_item_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_motor_class_code",
                table: "res_motor_class",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_damage_level_code",
                table: "res_damage_level",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_car_type_code",
                table: "res_car_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_car_model_code",
                table: "res_car_model",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_car_line_code",
                table: "res_car_line",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_car_group_code",
                table: "res_car_group",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_car_category_code",
                table: "res_car_category",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_car_brand_code",
                table: "res_car_brand",
                column: "code",
                unique: true);
        }
    }
}
