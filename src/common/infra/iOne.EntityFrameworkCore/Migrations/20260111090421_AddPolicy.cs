using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Hợp đồng bảo hiểm"),
                    lob_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Loại hình bảo hiểm"),
                    policy_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Số đơn bảo hiểm"),
                    last_version_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phiên bản hiện tại của Policy"),
                    sell_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Loại khai thác: agency (đại lý), direct (trực tiếp), indirect (gián tiếp)"),
                    insurer_policy_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã policy tương ứng của bảo hiểm gốc"),
                    policy_type_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Hình thức cấp đơn"),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Đơn vị/Đối tác cấp đơn"),
                    seller_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Nhân viên khai thác"),
                    implementer_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Nhân viên cấp đơn"),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Loại tiền tệ"),
                    exchange_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: false, defaultValue: 1m, comment: "Tỷ giá tại thời điểm bán"),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: quotation (báo giá), draft (nháp), active (đang hiệu lực), expired (hết hạn), terminated (đã chấm dứt), cancelled (đã hủy)"),
                    issue_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày cấp đơn"),
                    approval_status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Trạng thái duyệt: pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối duyệt)"),
                    cancellation_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày hủy đơn, bắt buộc có nếu trạng thái đơn là cancelled"),
                    termination_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày chấm dứt đơn, bắt buộc có nếu trạng thái đơn là terminated"),
                    cancellation_reason_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Lý do hủy đơn (tham chiếu bảng ResReason)"),
                    termination_reason_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Lý do chấm dứt đơn (tham chiếu bảng ResReason)"),
                    org_effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày hiệu lực gốc của đơn bảo hiểm"),
                    org_expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày hết hạn gốc của đơn bảo hiểm"),
                    is_renewal = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "N", comment: "Đánh dấu đơn có phải tái tục hay không: Y (có), N (không). Mặc định là N."),
                    is_gift = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "N", comment: "Đánh dấu đơn có phải quà tặng không: Y (có), N (không). Mặc định là N."),
                    premium_total = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Tổng phí bảo hiểm hiện hành"),
                    premium = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Phí bảo hiểm hiện hành"),
                    vat = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Tiền thuế hiện hành"),
                    discount = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Số tiền giảm phí trên tổng đơn hiện hành"),
                    discount_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Tỷ lệ giảm phí trên tổng đơn hiện hành"),
                    is_bank_loan = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "N", comment: "Có vay từ công ty tài chính hay không: Y (có), N (không). Mặc định là không."),
                    channel_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Kênh phân phối"),
                    lot_import_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã lô import"),
                    insured_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Tên người được bảo hiểm"),
                    insured_id_no = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Số CCCD người được bảo hiểm"),
                    insured_tin = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Mã số thuế người được bảo hiểm"),
                    insured_passport = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true, comment: "Số hộ chiếu người được bảo hiểm"),
                    insured_phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Số điện thoại người được bảo hiểm"),
                    insured_email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Email người được bảo hiểm"),
                    insured_province_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tỉnh người được bảo hiểm"),
                    insured_ward_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phường/xã người được bảo hiểm"),
                    insured_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Địa chỉ người được bảo hiểm"),
                    insured_full_address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Địa chỉ đầy đủ người được bảo hiểm"),
                    insured_org_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Loại người được bảo hiểm: individual (cá nhân), group (tổ chức)"),
                    beneficiary_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Tên người thụ hưởng"),
                    beneficiary_id_no = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Số CCCD người thụ hưởng"),
                    beneficiary_tin = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Mã số thuế người thụ hưởng"),
                    beneficiary_passport = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true, comment: "Số hộ chiếu người thụ hưởng"),
                    beneficiary_phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Số điện thoại người thụ hưởng"),
                    beneficiary_email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Email người thụ hưởng"),
                    beneficiary_province_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tỉnh người thụ hưởng"),
                    beneficiary_ward_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phường/xã người thụ hưởng"),
                    beneficiary_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Địa chỉ người thụ hưởng"),
                    beneficiary_full_address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Địa chỉ đầy đủ người thụ hưởng"),
                    beneficiary_org_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Loại người thụ hưởng: individual (cá nhân), group (tổ chức)"),
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
                    table.PrimaryKey("PK_policy", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_contract_id",
                        column: x => x.contract_id,
                        principalTable: "policy_contract",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_currency_id",
                        column: x => x.currency_id,
                        principalTable: "res_currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_implementer_id",
                        column: x => x.implementer_id,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_lob_id",
                        column: x => x.lob_id,
                        principalTable: "pro_line_of_business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_partner_id",
                        column: x => x.partner_id,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_policy_type_id",
                        column: x => x.policy_type_id,
                        principalTable: "policy_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_seller_id",
                        column: x => x.seller_id,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu thông tin đơn bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "ix_policy_contract_id",
                table: "policy",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_currency_id",
                table: "policy",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_implementer_id",
                table: "policy",
                column: "implementer_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_lob_id",
                table: "policy",
                column: "lob_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_partner_id",
                table: "policy",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_policy_no",
                table: "policy",
                column: "policy_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_policy_policy_type_id",
                table: "policy",
                column: "policy_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_seller_id",
                table: "policy",
                column: "seller_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy");
        }
    }
}
