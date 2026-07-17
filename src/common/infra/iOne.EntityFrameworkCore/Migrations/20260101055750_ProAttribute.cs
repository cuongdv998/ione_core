using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_attribute",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: - active: hoạt động - deactive: không hoạt động"),
                    spec = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Phạm vi tham số: RiskObject, Customer, Policy, Coverage"),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    data_path = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Loại dữ liệu: String, Int, Float, Date, Boolean"),
                    compute_script = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    clear_data_script = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_pro_attribute", x => x.id);
                },
                comment: "Bảng cấu hình các thuộc tính hệ thống");

            migrationBuilder.CreateIndex(
                name: "ix_pro_attribute_code",
                table: "pro_attribute",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_attribute");
        }
    }
}
