using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class accountPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_payment_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_res_payment_type", x => x.id);
                },
                comment: "Bảng định loại thanh toán");

            migrationBuilder.CreateTable(
                name: "account_payment_request",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Khách hàng nhận thanh toán"),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Đối tác nhận thanh toán"),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    issue_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày thanh toán"),
                    payment_method_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phương thức thanh toán"),
                    payment_type_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Loại thanh toán: Tạm ứng, Thanh toán công nợ"),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tiền tệ"),
                    claim_folder_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID của claim folder liên quan"),
                    due_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Hạn thanh toán"),
                    amount = table.Column<decimal>(type: "numeric(15,3)", precision: 15, scale: 3, nullable: false, comment: "Tổng giá trị thanh toán"),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: draft, pending_approval, approved, rejected, cancelled"),
                    submitted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày trình"),
                    submitter_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người trình"),
                    approve_emp_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người duyệt"),
                    approved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày duyệt"),
                    reason_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Lý do, liên kết với bảng ResReason (sử dụng khi từ chối duyệt)"),
                    reason_description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Mô tả khi chọn lý do"),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_payment_request", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_payment_request_claim_claim_folder_id",
                        column: x => x.claim_folder_id,
                        principalTable: "claim",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_payment_request_hr_employee_approve_emp_id",
                        column: x => x.approve_emp_id,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_payment_request_hr_employee_submitter_id",
                        column: x => x.submitter_id,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_payment_request_res_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "res_currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_payment_request_res_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "res_customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_payment_request_res_partner_partner_id",
                        column: x => x.partner_id,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_payment_request_res_payment_method_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "res_payment_method",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_payment_request_res_payment_type_payment_type_id",
                        column: x => x.payment_type_id,
                        principalTable: "res_payment_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_payment_request_res_reason_reason_id",
                        column: x => x.reason_id,
                        principalTable: "res_reason",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng đề nghị thanh toán");

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_request_approve_emp_id",
                table: "account_payment_request",
                column: "approve_emp_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_request_claim_folder_id",
                table: "account_payment_request",
                column: "claim_folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_request_currency_id",
                table: "account_payment_request",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_request_customer_id",
                table: "account_payment_request",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_request_partner_id",
                table: "account_payment_request",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_request_payment_method_id",
                table: "account_payment_request",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_request_payment_type_id",
                table: "account_payment_request",
                column: "payment_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_request_reason_id",
                table: "account_payment_request",
                column: "reason_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_request_submitter_id",
                table: "account_payment_request",
                column: "submitter_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_payment_request");

            migrationBuilder.DropTable(
                name: "res_payment_type");
        }
    }
}
