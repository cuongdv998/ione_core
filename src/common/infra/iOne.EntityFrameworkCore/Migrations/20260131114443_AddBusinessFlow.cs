using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "business_flow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Đơn vị (từ hr_department, dept_level = unit)"),
                    insurer_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Công ty bảo hiểm áp dụng"),
                    business_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Mã nghiệp vụ liên quan, định nghĩa trong bảng admin_config với code = BUSINESS_CODE và sub_code là các nghiệp vụ tương ứng"),
                    workflow_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workflow_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái: active - Hoạt động; deactive - Không hoạt động"),
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
                    table.PrimaryKey("PK_business_flow", x => x.id);
                },
                comment: "Định nghĩa workflow cho business");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "business_flow");
        }
    }
}
