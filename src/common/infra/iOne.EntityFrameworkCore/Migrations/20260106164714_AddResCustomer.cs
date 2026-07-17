using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddResCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_customer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động"),
                    industry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    province_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ward_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invoice_province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invoice_ward_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sale_id = table.Column<Guid>(type: "uuid", nullable: true),
                    address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    full_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    invoice_address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    invoice_full_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    id_no = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    passport_no = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    dob = table.Column<DateTime>(type: "date", nullable: true),
                    sex = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    rep_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    rep_email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    rep_phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    rep_id_no = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    rep_title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    authorizer = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    authorizer_phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    authorizer_email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    authorizer_no = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    authorizer_date = table.Column<DateTime>(type: "date", nullable: true),
                    authorizer_title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    business_no = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_res_customer", x => x.id);
                    table.ForeignKey(
                        name: "fk_res_customer_industry_id",
                        column: x => x.industry_id,
                        principalTable: "res_industry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_customer_invoice_province_id",
                        column: x => x.invoice_province_id,
                        principalTable: "res_province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_customer_invoice_ward_id",
                        column: x => x.invoice_ward_id,
                        principalTable: "res_ward",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_customer_organization_type_id",
                        column: x => x.organization_type_id,
                        principalTable: "res_organization_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_customer_province_id",
                        column: x => x.province_id,
                        principalTable: "res_province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_customer_sale_id",
                        column: x => x.sale_id,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_customer_ward_id",
                        column: x => x.ward_id,
                        principalTable: "res_ward",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu thông tin khách hàng");

            migrationBuilder.CreateIndex(
                name: "ix_res_customer_code",
                table: "res_customer",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_customer_industry_id",
                table: "res_customer",
                column: "industry_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_customer_invoice_province_id",
                table: "res_customer",
                column: "invoice_province_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_customer_invoice_ward_id",
                table: "res_customer",
                column: "invoice_ward_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_customer_organization_type_id",
                table: "res_customer",
                column: "organization_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_customer_province_id",
                table: "res_customer",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_customer_sale_id",
                table: "res_customer",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_customer_ward_id",
                table: "res_customer",
                column: "ward_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_customer_ward_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_customer_sale_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_customer_province_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_customer_organization_type_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_customer_invoice_ward_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_customer_invoice_province_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_customer_industry_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_customer_code"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_customer"";");
        }
    }
}
