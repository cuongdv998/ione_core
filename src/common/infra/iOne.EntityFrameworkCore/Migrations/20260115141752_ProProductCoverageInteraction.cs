using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProductCoverageInteraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "availability_type",
                table: "pro_product_coverage",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                comment: "Loại khả dụng: required (bắt buộc), standard (chuẩn), optional (tùy chọn), selectable (có thể chọn)",
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15);

            migrationBuilder.CreateTable(
                name: "pro_product_coverage_interaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_coverage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    interaction_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Loại ràng buộc: dependency (phụ thuộc), incompatible (không tương thích), exclusive (loại trừ nhau)"),
                    interaction_coverage_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phạm vi có ràng buộc"),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
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
                    table.PrimaryKey("PK_pro_product_coverage_interaction", x => x.id);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_interaction_pro_product_coverage_inter~",
                        column: x => x.interaction_coverage_id,
                        principalTable: "pro_product_coverage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_interaction_pro_product_coverage_produ~",
                        column: x => x.product_coverage_id,
                        principalTable: "pro_product_coverage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng định nghĩa ràng buộc giữa các phạm vi bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_coverage_interaction_interaction_coverage_id",
                table: "pro_product_coverage_interaction",
                column: "interaction_coverage_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_coverage_interaction_product_coverage_id",
                table: "pro_product_coverage_interaction",
                column: "product_coverage_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_product_coverage_interaction");

            migrationBuilder.AlterColumn<string>(
                name: "availability_type",
                table: "pro_product_coverage",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldComment: "Loại khả dụng: required (bắt buộc), standard (chuẩn), optional (tùy chọn), selectable (có thể chọn)");
        }
    }
}
