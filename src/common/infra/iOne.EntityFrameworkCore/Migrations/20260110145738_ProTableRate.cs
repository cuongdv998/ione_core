using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProTableRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pro_table_rate",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lob_id = table.Column<Guid>(type: "uuid", nullable: false),
                    insurer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
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
                    table.PrimaryKey("pk_pro_table_rate", x => x.id);
                    table.ForeignKey(
                        name: "fk_pro_table_rate_pro_line_of_business_lob_id",
                        column: x => x.lob_id,
                        principalTable: "pro_line_of_business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pro_table_rate_res_partner_insurer_id",
                        column: x => x.insurer_id,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng định nghĩa Bảng phí");

            migrationBuilder.CreateTable(
                name: "pro_table_rate_variable",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_rate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_id = table.Column<Guid>(type: "uuid", nullable: false),
                    @operator = table.Column<string>(name: "operator", type: "character varying(15)", maxLength: 15, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pro_table_rate_variable", x => x.id);
                    table.ForeignKey(
                        name: "fk_pro_table_rate_variable_pro_attribute_attribute_id",
                        column: x => x.attribute_id,
                        principalTable: "pro_attribute",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pro_table_rate_variable_pro_table_rate_table_rate_id",
                        column: x => x.table_rate_id,
                        principalTable: "pro_table_rate",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Định nghĩa biến đầu vào của một bảng Rate");

            migrationBuilder.CreateIndex(
                name: "ix_pro_table_rate_code",
                table: "pro_table_rate",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pro_table_rate_insurer_id",
                table: "pro_table_rate",
                column: "insurer_id");

            migrationBuilder.CreateIndex(
                name: "IX_pro_table_rate_lob_id",
                table: "pro_table_rate",
                column: "lob_id");

            migrationBuilder.CreateIndex(
                name: "ix_pro_table_rate_variable_attribute_id",
                table: "pro_table_rate_variable",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "ix_pro_table_rate_variable_table_rate_id",
                table: "pro_table_rate_variable",
                column: "table_rate_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pro_table_rate_variable");

            migrationBuilder.DropTable(
                name: "pro_table_rate");
        }
    }
}
