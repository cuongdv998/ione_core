using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policy_product",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_version_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến phiên bản đơn bảo hiểm"),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến sản phẩm bảo hiểm"),
                    insurer_product_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã sản phẩm tương ứng của BH gốc"),
                    amount_liability = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Mức trách nhiệm"),
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
                    table.PrimaryKey("PK_policy_product", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_product_policy_version_id",
                        column: x => x.policy_version_id,
                        principalTable: "policy_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu sản phẩm liên quan đến policy");

            migrationBuilder.CreateIndex(
                name: "ix_policy_product_policy_version_id",
                table: "policy_product",
                column: "policy_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_product_product_id",
                table: "policy_product",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_product");
        }
    }
}
