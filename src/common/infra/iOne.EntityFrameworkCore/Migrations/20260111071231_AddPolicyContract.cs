using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AlterPolicyContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policy_contract",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    insurer_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Doanh nghiệp bảo hiểm"),
                    insurer_contract_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã hợp đồng phía công ty bảo hiểm"),
                    lob_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Nghiệp vụ bảo hiểm"),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Loại hợp đồng"),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Khách hàng ký hợp đồng"),
                    payer_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payer_email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payer_phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    payer_province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payer_ward_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payer_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payer_full_address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    quantity = table.Column<decimal>(type: "NUMERIC(6)", nullable: false),
                    current_quantity = table.Column<decimal>(type: "NUMERIC(6)", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: active (hoạt động), deactive (không hoạt động)"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cancellation_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    termination_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_reason_id = table.Column<Guid>(type: "uuid", nullable: true),
                    termination_reason_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cancellation_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    termination_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_recive_invoice = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "N", comment: "Có nhận hóa đơn hay không"),
                    quotation_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_policy_contract", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_contract_customer_id",
                        column: x => x.customer_id,
                        principalTable: "res_customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_contract_employee_id",
                        column: x => x.employee_id,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_contract_insurer_id",
                        column: x => x.insurer_id,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_contract_lob_id",
                        column: x => x.lob_id,
                        principalTable: "pro_line_of_business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Hợp đồng bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "ix_policy_contract_code",
                table: "policy_contract",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_policy_contract_customer_id",
                table: "policy_contract",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_contract_employee_id",
                table: "policy_contract",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_contract_insurer_id",
                table: "policy_contract",
                column: "insurer_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_contract_lob_id",
                table: "policy_contract",
                column: "lob_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_policy_contract_lob_id",
                table: "policy_contract");

            migrationBuilder.DropIndex(
                name: "ix_policy_contract_insurer_id",
                table: "policy_contract");

            migrationBuilder.DropIndex(
                name: "ix_policy_contract_employee_id",
                table: "policy_contract");

            migrationBuilder.DropIndex(
                name: "ix_policy_contract_customer_id",
                table: "policy_contract");

            migrationBuilder.DropIndex(
                name: "ix_policy_contract_code",
                table: "policy_contract");

            migrationBuilder.DropTable(
                name: "policy_contract");
        }
    }
}
