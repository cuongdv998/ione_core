using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddTableProCoverageLevelBasis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_coverage_level_basis",
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
                    table.PrimaryKey("PK_pro_coverage_level_basis", x => x.id);
                },
                comment: "Bảng định nghĩa các loại mức độ của phạm vi bảo hiểm. Ví dụ: mức miễn thường, mức đồng chi trả ...");

            migrationBuilder.CreateIndex(
                name: "ix_pro_coverage_level_basis_code",
                table: "pro_coverage_level_basis",
                column: "code",
                unique: true);

            // Insert master data
            var typeOneId = Guid.NewGuid();
            var typeTwoId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var concurrencyStamp1 = Guid.NewGuid().ToString("N").Substring(0, 40);
            var concurrencyStamp2 = Guid.NewGuid().ToString("N").Substring(0, 40);
            
            migrationBuilder.Sql($@"
                INSERT INTO pro_coverage_level_basis (id, code, name, description, status, extra_properties, concurrency_stamp, creation_time, creator_id, is_deleted)
                VALUES 
                ('{typeOneId}', 'TYPEONE', 'theo người', NULL, 'active', '{{}}', '{concurrencyStamp1}', CURRENT_TIMESTAMP, '{creatorId}', false),
                ('{typeTwoId}', 'TYPETWO', 'theo năm', NULL, 'active', '{{}}', '{concurrencyStamp2}', CURRENT_TIMESTAMP, '{creatorId}', false);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_coverage_level_basis");
        }
    }
}
