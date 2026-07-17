using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProCoverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_coverage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lob_id = table.Column<Guid>(type: "uuid", nullable: false),
                    object_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    coverage_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coverage_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    short_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Loại điều khoản: Main (Phạm vi chính), Addon (Phạm vi bổ sung), Exclusion (Phạm vi loại trừ), Benefit (Quyền lợi đi kèm)"),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: - active: hoạt động - deactive: không hoạt động"),
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
                    table.PrimaryKey("PK_pro_coverage", x => x.id);
                    table.ForeignKey(
                        name: "FK_pro_coverage_pro_coverage_group_coverage_group_id",
                        column: x => x.coverage_group_id,
                        principalTable: "pro_coverage_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_coverage_pro_coverage_type_coverage_type_id",
                        column: x => x.coverage_type_id,
                        principalTable: "pro_coverage_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_coverage_pro_line_of_business_lob_id",
                        column: x => x.lob_id,
                        principalTable: "pro_line_of_business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pro_coverage_res_object_type_object_type_id",
                        column: x => x.object_type_id,
                        principalTable: "res_object_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng định nghĩa phạm vi bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "ix_pro_coverage_code",
                table: "pro_coverage",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pro_coverage_coverage_group_id",
                table: "pro_coverage",
                column: "coverage_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_coverage_coverage_type_id",
                table: "pro_coverage",
                column: "coverage_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_coverage_lob_id",
                table: "pro_coverage",
                column: "lob_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_coverage_object_type_id",
                table: "pro_coverage",
                column: "object_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_coverage");
        }
    }
}
