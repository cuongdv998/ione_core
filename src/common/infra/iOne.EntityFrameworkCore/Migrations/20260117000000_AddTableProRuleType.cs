using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddTableProRuleType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_rule_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: active (hoạt động), deactive (không hoạt động)"),
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
                    table.PrimaryKey("PK_pro_rule_type", x => x.id);
                },
                comment: "Bảng định nghĩa các quy tắc đối với sản phẩm");

            migrationBuilder.CreateIndex(
                name: "ix_pro_rule_type_code",
                table: "pro_rule_type",
                column: "code",
                unique: true);

            // Insert master data
            var typeOneId = Guid.NewGuid();
            var typeTwoId = Guid.NewGuid();
            var typeThreeId = Guid.NewGuid();
            var typeFourId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var concurrencyStamp1 = Guid.NewGuid().ToString("N").Substring(0, 40);
            var concurrencyStamp2 = Guid.NewGuid().ToString("N").Substring(0, 40);
            var concurrencyStamp3 = Guid.NewGuid().ToString("N").Substring(0, 40);
            var concurrencyStamp4 = Guid.NewGuid().ToString("N").Substring(0, 40);
            
            migrationBuilder.Sql($@"
                INSERT INTO pro_rule_type (id, code, name, description, status, extra_properties, concurrency_stamp, creation_time, creator_id, is_deleted)
                VALUES 
                ('{typeOneId}', 'TYPEONE', 'Eligibility', NULL, 'active', '{{}}', '{concurrencyStamp1}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{typeTwoId}', 'TYPETWO', 'Underwriting', NULL, 'active', '{{}}', '{concurrencyStamp2}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{typeThreeId}', 'TYPETHREE', 'PricingOverride', NULL, 'active', '{{}}', '{concurrencyStamp3}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{typeFourId}', 'TYPEFOUR', 'Discount', NULL, 'active', '{{}}', '{concurrencyStamp4}', CURRENT_TIMESTAMP, '{creatorId}', false);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_rule_type");
        }
    }
}
