using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProductCoverageLevelTerm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_product_coverage_level_term",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_coverage_level_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tham chiếu đến nhóm hạn mức của phạm vi"),
                    coverage_level_type_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Loại hạn mức"),
                    coverage_level_basis_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Cơ sở tính hạn mức"),
                    amount_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Loại giá trị: percent (Tỷ lệ), fix (Giá trị cố định), quantity (Số lượng)"),
                    from_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Mức tối thiểu"),
                    to_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Mức tối đa"),
                    condition_script = table.Column<string>(type: "TEXT", nullable: true, comment: "Script dạng python cho phép kiểm tra các điều kiện nhất định"),
                    compute_script = table.Column<string>(type: "TEXT", nullable: true, comment: "Script python tính toán ra giá trị"),
                    is_default = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Hiển thị mặc định: Y (có), N (không)"),
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
                    table.PrimaryKey("PK_pro_product_coverage_level_term", x => x.id);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_level_term_pro_coverage_level_basis_co~",
                        column: x => x.coverage_level_basis_id,
                        principalTable: "pro_coverage_level_basis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_level_term_pro_coverage_level_type_cov~",
                        column: x => x.coverage_level_type_id,
                        principalTable: "pro_coverage_level_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_level_term_pro_product_coverage_level_~",
                        column: x => x.product_coverage_level_id,
                        principalTable: "pro_product_coverage_level",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng định nghĩa các hạn mức chi tiết của phạm vi bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "ix_pro_product_coverage_level_term_coverage_level_basis_id",
                table: "pro_product_coverage_level_term",
                column: "coverage_level_basis_id");

            migrationBuilder.CreateIndex(
                name: "ix_pro_product_coverage_level_term_coverage_level_type_id",
                table: "pro_product_coverage_level_term",
                column: "coverage_level_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_pro_product_coverage_level_term_product_coverage_level_id",
                table: "pro_product_coverage_level_term",
                column: "product_coverage_level_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_product_coverage_level_term");
        }
    }
}
