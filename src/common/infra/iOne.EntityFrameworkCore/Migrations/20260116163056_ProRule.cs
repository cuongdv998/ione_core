using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_rule",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    apply_to = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Phạm vi áp dụng: product (mức sản phẩm), coverage (mức phạm vi), level (mức hạn mức của phạm vi)"),
                    apply_to_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rule_script = table.Column<string>(type: "text", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "Thứ tự ưu tiên thực thi, số nhỏ ưu tiên trước"),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: active (hoạt động), deactive (không hoạt động)"),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
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
                    table.PrimaryKey("PK_pro_rule", x => x.id);
                    table.ForeignKey(
                        name: "FK_pro_rule_pro_rule_type_rule_type_id",
                        column: x => x.rule_type_id,
                        principalTable: "pro_rule_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng định nghĩa các quy tắc đối với sản phẩm");

            migrationBuilder.CreateIndex(
                name: "IX_pro_rule_rule_type_id",
                table: "pro_rule",
                column: "rule_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_rule");
        }
    }
}
