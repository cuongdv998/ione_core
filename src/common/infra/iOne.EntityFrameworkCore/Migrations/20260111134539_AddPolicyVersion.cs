using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policy_version",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<decimal>(type: "NUMERIC(2)", nullable: false, comment: "Phiên bản"),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến đơn bảo hiểm gốc"),
                    type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Loại Policy: O (policy gốc), A (policy sửa đổi bổ sung)"),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái đơn: quotation (báo giá), draft (nháp), active (đang hiệu lực), expired (hết hạn), terminated (chấm dứt), cancelled (hủy)"),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày hiệu lực"),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày hết hạn"),
                    org_effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày hiệu lực gốc"),
                    org_expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày hết hạn gốc"),
                    internal_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Ghi chú nội bộ"),
                    customer_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Ghi chú khách hàng"),
                    premium_total = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Tổng phí bảo hiểm"),
                    premium = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Phí bảo hiểm"),
                    vat = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Tiền thuế"),
                    discount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Số tiền giảm phí"),
                    discount_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Tỷ lệ giảm phí"),
                    approval_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày duyệt"),
                    approver_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người duyệt"),
                    approval_status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Trạng thái duyệt: pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối duyệt)"),
                    insurer_integration_status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Trạng thái tích hợp với BH gốc: fail (lỗi tích hợp), succ (thành công)"),
                    insurer_integration_description = table.Column<string>(type: "TEXT", nullable: true, comment: "Mô tả tích hợp với BH gốc"),
                    endorsement_type = table.Column<Guid>(type: "uuid", nullable: true, comment: "Loại sửa đổi bổ sung"),
                    endorsement_reason_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Lý do sửa đổi bổ sung"),
                    endorsement_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true, comment: "Mô tả sửa đổi bổ sung"),
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
                    table.PrimaryKey("PK_policy_version", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_version_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Phiên bản đơn bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "ix_policy_version_policy_id",
                table: "policy_version",
                column: "policy_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_version");
        }
    }
}
