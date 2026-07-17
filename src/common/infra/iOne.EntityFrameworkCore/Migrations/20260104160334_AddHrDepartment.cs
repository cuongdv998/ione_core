using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddHrDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hr_department",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"),
                    dept_level = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Đơn vị hay phòng/ban:\n- unit: đơn vị\n- dept: phòng/ban"),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    province_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tỉnh đăng ký kinh doanh"),
                    ward_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phường/Xã đăng ký kinh doanh"),
                    bank_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Ngân hàng"),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Địa chỉ văn phòng đăng ký kinh doanh"),
                    full_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    bank_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Số tài khoản ngân hàng"),
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
                    table.PrimaryKey("PK_hr_department", x => x.id);
                    table.ForeignKey(
                        name: "fk_hr_department_bank_id",
                        column: x => x.bank_id,
                        principalTable: "res_bank",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_department_org_id",
                        column: x => x.org_id,
                        principalTable: "hr_department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_department_parent_id",
                        column: x => x.parent_id,
                        principalTable: "hr_department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_department_partner_id",
                        column: x => x.partner_id,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_department_province_id",
                        column: x => x.province_id,
                        principalTable: "res_province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_department_type_id",
                        column: x => x.type_id,
                        principalTable: "hr_department_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_department_ward_id",
                        column: x => x.ward_id,
                        principalTable: "res_ward",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Phòng ban/Đơn vị");

            migrationBuilder.CreateIndex(
                name: "ix_hr_department_bank_id",
                table: "hr_department",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_department_code",
                table: "hr_department",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_department_org_id",
                table: "hr_department",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_department_parent_id",
                table: "hr_department",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_department_partner_id",
                table: "hr_department",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_department_province_id",
                table: "hr_department",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_department_type_id",
                table: "hr_department",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_department_ward_id",
                table: "hr_department",
                column: "ward_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hr_department");
        }
    }
}
