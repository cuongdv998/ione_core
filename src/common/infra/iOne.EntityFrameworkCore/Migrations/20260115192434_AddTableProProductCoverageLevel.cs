using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddTableProProductCoverageLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_product_coverage_level",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_coverage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày hiệu lực"),
                    expire_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày hết hạn"),
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
                    table.PrimaryKey("PK_pro_product_coverage_level", x => x.id);
                    table.ForeignKey(
                        name: "FK_pro_product_coverage_level_pro_product_coverage_product_cov~",
                        column: x => x.product_coverage_id,
                        principalTable: "pro_product_coverage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Định nghĩa các nhóm hạn mức của phạm vi");

            migrationBuilder.CreateIndex(
                name: "IX_pro_product_coverage_level_product_coverage_id",
                table: "pro_product_coverage_level",
                column: "product_coverage_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_product_coverage_level");
        }
    }
}
