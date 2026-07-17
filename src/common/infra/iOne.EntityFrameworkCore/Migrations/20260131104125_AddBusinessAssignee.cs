using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessAssignee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_business_assignee",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    business_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Mã nghiệp vụ liên quan, từ admin_config code = BUSINESS_CODE"),
                    authority_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Mã thẩm quyền thực hiện"),
                    assignee_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "emp - đích danh; role - theo vai trò; system - hệ thống"),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    assignee_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã vai trò, bắt buộc nếu assigneeType = role"),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID nhân viên, bắt buộc nếu assigneeType = emp"),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Đơn vị thực hiện"),
                    department_level = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "in - trong phân cấp; out - trên phân cấp"),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "active / deactive"),
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
                    table.PrimaryKey("PK_res_business_assignee", x => x.id);
                },
                comment: "Bảng định nghĩa đối tượng sẽ thực hiện theo thẩm quyền");

            migrationBuilder.CreateIndex(
                name: "ix_res_business_assignee_assignee_id",
                table: "res_business_assignee",
                column: "assignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_business_assignee_authority_code",
                table: "res_business_assignee",
                column: "authority_code");

            migrationBuilder.CreateIndex(
                name: "ix_res_business_assignee_business_code",
                table: "res_business_assignee",
                column: "business_code");

            migrationBuilder.CreateIndex(
                name: "ix_res_business_assignee_department_id",
                table: "res_business_assignee",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_business_assignee_effect_date",
                table: "res_business_assignee",
                column: "effect_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "res_business_assignee");
        }
    }
}
