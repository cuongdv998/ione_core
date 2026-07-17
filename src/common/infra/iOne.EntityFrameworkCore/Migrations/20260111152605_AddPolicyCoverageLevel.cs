using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyCoverageLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policy_coverage_level",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_coverage_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến phạm vi bảo hiểm của đơn"),
                    coverage_level_type_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Loại hạn mức"),
                    coverage_level_basis_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Cơ sở tính hạn mức"),
                    condition_script = table.Column<string>(type: "TEXT", nullable: true, comment: "Script điều kiện áp dụng hạn mức"),
                    compute_script = table.Column<string>(type: "TEXT", nullable: true, comment: "Script tính toán giá trị hạn mức"),
                    amount_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Loại giá trị: percent (Tỷ lệ), fix (Giá trị cố định), quantity (Số lượng)"),
                    from_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Mức tối thiểu"),
                    to_amount = table.Column<decimal>(type: "numeric(15,3)", nullable: false, comment: "Mức tối đa"),
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
                    table.PrimaryKey("PK_policy_coverage_level", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_coverage_level_policy_coverage_id",
                        column: x => x.policy_coverage_id,
                        principalTable: "policy_coverage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu các hạn mức của phạm vi bảo hiểm theo đơn");

            migrationBuilder.CreateIndex(
                name: "ix_policy_coverage_level_policy_coverage_id",
                table: "policy_coverage_level",
                column: "policy_coverage_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_coverage_level");
        }
    }
}
