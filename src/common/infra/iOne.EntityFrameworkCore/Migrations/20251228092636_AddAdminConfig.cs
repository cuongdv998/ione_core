using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_config",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    sub_code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, comment: "Mã con (nếu một cấu hình lớn cần chia nhiều cấu hình con)"),
                    value = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Giá trị cấu hình"),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"),
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
                    table.PrimaryKey("pk_admin_config", x => x.id);
                },
                comment: "Bảng lưu trữ các cấu hình chung của hệ thống");

            migrationBuilder.CreateIndex(
                name: "ix_admin_config_code_sub_code",
                table: "admin_config",
                columns: new[] { "code", "sub_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_admin_config_code_sub_code"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""admin_config"";");
        }
    }
}
