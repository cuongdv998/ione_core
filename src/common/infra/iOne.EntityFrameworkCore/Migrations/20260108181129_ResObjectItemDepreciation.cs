using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ResObjectItemDepreciation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_object_item_depreciation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    object_type_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    car_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    used_time_from = table.Column<double>(type: "double precision", nullable: false, comment: "Thời gian sử dụng (năm) từ"),
                    used_time_to = table.Column<double>(type: "double precision", nullable: false, comment: "Thời gian sử dụng (năm) đến"),
                    depreciation_percent = table.Column<double>(type: "double precision", nullable: false, comment: "Tỷ lệ khấu hao (%)"),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày hiệu lực"),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày hết hạn"),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: active (hoạt động), deactive (không hoạt động)"),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_res_object_item_depreciation", x => x.id);
                    table.ForeignKey(
                        name: "FK_res_object_item_depreciation_res_object_type_item_object_ty~",
                        column: x => x.object_type_item_id,
                        principalTable: "res_object_type_item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Định nghĩa khấu hao tài sản");

            migrationBuilder.CreateIndex(
                name: "ix_res_object_item_depreciation_car_group_id",
                table: "res_object_item_depreciation",
                column: "car_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_object_item_depreciation_dates",
                table: "res_object_item_depreciation",
                columns: new[] { "effect_date", "expire_date" });

            migrationBuilder.CreateIndex(
                name: "ix_res_object_item_depreciation_object_type_item_id",
                table: "res_object_item_depreciation",
                column: "object_type_item_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "res_object_item_depreciation");
        }
    }
}
