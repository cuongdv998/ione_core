using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimFolderQuotationApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "claim_folder_quotation_approval",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Yêu cầu bồi thường"),
                    claim_folder_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Hồ sơ (tùy chọn)"),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Đối tác chịu trách nhiệm"),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: new (mới), inprogress (đang xử lý), pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối), done (hoàn thành), cancelled (hủy)"),
                    submitted_date = table.Column<DateTime>(type: "date", nullable: false, comment: "Ngày trình duyệt"),
                    submitter_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Người trình duyệt"),
                    approved_date = table.Column<DateTime>(type: "date", nullable: true, comment: "Ngày phê duyệt"),
                    approver_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người phê duyệt"),
                    claim_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Số tiền yêu cầu bồi thường"),
                    discount_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Tổng tiền giảm giá"),
                    depreciation_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Tổng khấu hao phụ tùng"),
                    expense_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Số tiền sẽ chi trả"),
                    assessment_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Chi phí giám định"),
                    loss_prevention_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Chi phí đề phòng hạn chế tổn thất"),
                    rescue_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Chi phí cứu hộ"),
                    other_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Chi phí khác"),
                    data = table.Column<string>(type: "text", nullable: true, comment: "Dữ liệu snapshot của lần trình duyệt (JSON)"),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    reason_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Lý do (bắt buộc khi từ chối)"),
                    reason_description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Mô tả lý do"),
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
                    table.PrimaryKey("PK_claim_folder_quotation_approval", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_folder_quotation_approval_res_partner_partner_id",
                        column: x => x.partner_id,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_quotation_approval_res_reason_reason_id",
                        column: x => x.reason_id,
                        principalTable: "res_reason",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_claim_folder_quotation_approval_claim",
                        column: x => x.claim_id,
                        principalTable: "claim",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_claim_folder_quotation_approval_claim_folder",
                        column: x => x.claim_folder_id,
                        principalTable: "claim_folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Phê duyệt PASC/Báo giá");

            migrationBuilder.CreateIndex(
                name: "ix_claim_folder_quotation_approval_claim_folder_id",
                table: "claim_folder_quotation_approval",
                column: "claim_folder_id");

            migrationBuilder.CreateIndex(
                name: "ix_claim_folder_quotation_approval_claim_id",
                table: "claim_folder_quotation_approval",
                column: "claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_quotation_approval_partner_id",
                table: "claim_folder_quotation_approval",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_quotation_approval_reason_id",
                table: "claim_folder_quotation_approval",
                column: "reason_id");

            migrationBuilder.CreateIndex(
                name: "ix_claim_folder_quotation_approval_status",
                table: "claim_folder_quotation_approval",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "claim_folder_quotation_approval");
        }
    }
}
