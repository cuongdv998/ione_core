using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddResProvince : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP (nếu table đã tồn tại)
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_province"";");

            migrationBuilder.CreateTable(
                name: "res_province",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động"),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
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
                    // ✅ ĐÚNG: Primary key name theo snake_case với prefix
                    table.PrimaryKey("pk_res_province", x => x.id);
                    // ✅ ĐÚNG: Foreign key name theo snake_case với prefix
                    table.ForeignKey(
                        name: "fk_res_province_country_id",
                        column: x => x.country_id,
                        principalTable: "res_country",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tỉnh/Thành");

            // ✅ ĐÚNG: Index name theo snake_case với prefix
            migrationBuilder.CreateIndex(
                name: "ix_res_province_code",
                table: "res_province",
                column: "code",
                unique: true);

            // ✅ ĐÚNG: Index trên foreign key để tối ưu query performance
            migrationBuilder.CreateIndex(
                name: "ix_res_province_country_id",
                table: "res_province",
                column: "country_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_province_country_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_province_code"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_province"";");
        }
    }
}
