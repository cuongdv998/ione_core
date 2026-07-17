using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyCoverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policy_coverage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_product_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến sản phẩm bảo hiểm của đơn"),
                    coverage_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến phạm vi bảo hiểm"),
                    coverage_parent_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tham chiếu đến phạm vi bảo hiểm cha (nếu có)"),
                    insurer_coverage_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã phạm vi bảo hiểm tương ứng của BH gốc"),
                    uom_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Đơn vị đo lường"),
                    table_rate_line_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tham chiếu đến bảng tỷ lệ"),
                    amount_liability = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Mức trách nhiệm"),
                    quantity = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Số lượng tham gia"),
                    tax_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Thuế suất"),
                    net_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Phí thuần (theo đăng ký với bộ tài chính)"),
                    base_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Phí cơ bản (>= phí thuần)"),
                    flat_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Phí theo số tiền cố định"),
                    loading = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Phí bổ sung thêm"),
                    premium_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Tỷ lệ phí bảo hiểm (= phí cơ sở + phí bổ sung)"),
                    premium_total = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Tổng phí bảo hiểm"),
                    premium = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Phí bảo hiểm trước thuế"),
                    vat = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Tiền thuế"),
                    discount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Giảm phí theo số tiền"),
                    discount_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Giảm phí theo tỷ lệ"),
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
                    table.PrimaryKey("PK_policy_coverage", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_coverage_policy_product_id",
                        column: x => x.policy_product_id,
                        principalTable: "policy_product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu các phạm vi bảo hiểm của đơn bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "ix_policy_coverage_coverage_id",
                table: "policy_coverage",
                column: "coverage_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_coverage_policy_product_id",
                table: "policy_coverage",
                column: "policy_product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_coverage");
        }
    }
}
