using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProductCoverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "pro_product",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                comment: "Trạng thái: active (hoạt động), deactive (không hoạt động), draft (nháp)",
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldComment: "Trạng thái: active (hoạt động), deactive (không hoạt động)");

            migrationBuilder.CreateTable(
                name: "pro_product_coverage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coverage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    insurer_coverage_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    uom_id = table.Column<Guid>(type: "uuid", nullable: true),
                    availability_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    seq_number = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    tax_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enable_quantity = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
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
                    table.PrimaryKey("PK_pro_product_coverage", x => x.id);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_pro_coverage_coverage_id",
                        column: x => x.coverage_id,
                        principalTable: "pro_coverage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_pro_product_coverage_parent_id",
                        column: x => x.parent_id,
                        principalTable: "pro_product_coverage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_pro_product_product_id",
                        column: x => x.product_id,
                        principalTable: "pro_product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_res_tax_tax_id",
                        column: x => x.tax_id,
                        principalTable: "res_tax",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_res_uom_uom_id",
                        column: x => x.uom_id,
                        principalTable: "res_uom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng định nghĩa các phạm vi bảo hiểm của sản phẩm");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_coverage_coverage_id",
                table: "pro_product_coverage",
                column: "coverage_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_coverage_parent_id",
                table: "pro_product_coverage",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_coverage_product_id",
                table: "pro_product_coverage",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_coverage_tax_id",
                table: "pro_product_coverage",
                column: "tax_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_coverage_uom_id",
                table: "pro_product_coverage",
                column: "uom_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_product_coverage");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "pro_product",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                comment: "Trạng thái: active (hoạt động), deactive (không hoạt động)",
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldComment: "Trạng thái: active (hoạt động), deactive (không hoạt động), draft (nháp)");
        }
    }
}
