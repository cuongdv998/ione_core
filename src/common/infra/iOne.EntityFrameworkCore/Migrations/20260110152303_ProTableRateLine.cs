using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProTableRateLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_table_rate_line",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_rate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coverage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    channel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    condition = table.Column<string>(type: "text", nullable: false),
                    net_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: true),
                    base_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: true),
                    flat_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: true),
                    max_discount = table.Column<decimal>(type: "numeric(15,3)", nullable: true),
                    loading_rate = table.Column<decimal>(type: "numeric(15,3)", nullable: true),
                    effect_date = table.Column<DateTime>(type: "date", nullable: false),
                    expire_date = table.Column<DateTime>(type: "date", nullable: true),
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
                    table.PrimaryKey("pk_pro_table_rate_line", x => x.id);
                    table.ForeignKey(
                        name: "fk_pro_table_rate_line_pro_coverage_coverage_id",
                        column: x => x.coverage_id,
                        principalTable: "pro_coverage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pro_table_rate_line_pro_table_rate_table_rate_id",
                        column: x => x.table_rate_id,
                        principalTable: "pro_table_rate",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pro_table_rate_line_res_channel_channel_id",
                        column: x => x.channel_id,
                        principalTable: "res_channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pro_table_rate_line_res_partner_partner_id",
                        column: x => x.partner_id,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng Định nghĩa phí bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "IX_pro_table_rate_line_channel_id",
                table: "pro_table_rate_line",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_table_rate_line_coverage_id",
                table: "pro_table_rate_line",
                column: "coverage_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_table_rate_line_partner_id",
                table: "pro_table_rate_line",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_table_rate_line_table_rate_id",
                table: "pro_table_rate_line",
                column: "table_rate_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_table_rate_line");
        }
    }
}
