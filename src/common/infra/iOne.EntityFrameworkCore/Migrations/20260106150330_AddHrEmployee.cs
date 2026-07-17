using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddHrEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "org_id",
                table: "hr_department",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "hr_employee",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"),
                    position_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Liên kết chức danh"),
                    level_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Liên kết cấp bậc"),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    org_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID đơn vị"),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_manager = table.Column<bool>(type: "boolean", nullable: true, comment: "Đánh dấu có phải lãnh đạo đơn vị không:\n- true: có\n- false: không"),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người quản lý trực tiếp"),
                    province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ward_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID liên kết với 1 user login"),
                    address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    full_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("pk_hr_employee", x => x.id);
                    table.ForeignKey(
                        name: "fk_hr_employee_abp_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employee_hr_department_department_id",
                        column: x => x.department_id,
                        principalTable: "hr_department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employee_hr_department_org_id",
                        column: x => x.org_id,
                        principalTable: "hr_department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employee_hr_employee_level_level_id",
                        column: x => x.level_id,
                        principalTable: "hr_employee_level",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employee_hr_employee_manager_id",
                        column: x => x.manager_id,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employee_hr_employee_position_position_id",
                        column: x => x.position_id,
                        principalTable: "hr_employee_position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employee_res_partner_partner_id",
                        column: x => x.partner_id,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employee_res_province_province_id",
                        column: x => x.province_id,
                        principalTable: "res_province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employee_res_ward_ward_id",
                        column: x => x.ward_id,
                        principalTable: "res_ward",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng định nghĩa nhân viên");

            migrationBuilder.CreateTable(
                name: "hr_employee_role_rel",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hr_employee_role_rel", x => x.id);
                    table.ForeignKey(
                        name: "fk_hr_employee_role_rel_hr_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hr_employee_role_rel_hr_employee_role_role_id",
                        column: x => x.role_id,
                        principalTable: "hr_employee_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu các vai trò của nhân viên, một nhân viên có thể có nhiều hơn 1 vai trò");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_code",
                table: "hr_employee",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_department_id",
                table: "hr_employee",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_level_id",
                table: "hr_employee",
                column: "level_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_manager_id",
                table: "hr_employee",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_org_id",
                table: "hr_employee",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_partner_id",
                table: "hr_employee",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_position_id",
                table: "hr_employee",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_province_id",
                table: "hr_employee",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_user_id",
                table: "hr_employee",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_ward_id",
                table: "hr_employee",
                column: "ward_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_role_rel_employee_id",
                table: "hr_employee_role_rel",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_role_rel_employee_role",
                table: "hr_employee_role_rel",
                columns: new[] { "employee_id", "role_id" });

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_role_rel_role_id",
                table: "hr_employee_role_rel",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hr_employee_role_rel");

            migrationBuilder.DropTable(
                name: "hr_employee");

            migrationBuilder.AlterColumn<Guid>(
                name: "org_id",
                table: "hr_department",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
