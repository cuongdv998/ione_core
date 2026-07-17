using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ReportTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "policy_contract",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                comment: "Loại hợp đồng:\n- individual: hợp đồng lẻ\n- group: hợp đồng nhóm",
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldComment: "Loại hợp đồng");

            migrationBuilder.CreateTable(
                name: "report_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "draft"),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
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
                    table.PrimaryKey("PK_report_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_template_parameter",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "active"),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    data_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_report_template_parameter", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_template_parameter_report_template_report_template_id",
                        column: x => x.report_template_id,
                        principalTable: "report_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_template_sql",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sql_text = table.Column<string>(type: "text", nullable: false),
                    var_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "active"),
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
                    table.PrimaryKey("PK_report_template_sql", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_template_sql_report_template_report_template_id",
                        column: x => x.report_template_id,
                        principalTable: "report_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_template_sql_parameter",
                columns: table => new
                {
                    sql_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parameter_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_template_sql_parameter", x => new { x.sql_id, x.parameter_id });
                    table.ForeignKey(
                        name: "FK_report_template_sql_parameter_report_template_parameter_par~",
                        column: x => x.parameter_id,
                        principalTable: "report_template_parameter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_template_sql_parameter_report_template_sql_sql_id",
                        column: x => x.sql_id,
                        principalTable: "report_template_sql",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_template_code",
                table: "report_template",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_template_parameter_report_template_id_code",
                table: "report_template_parameter",
                columns: new[] { "report_template_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_template_sql_report_template_id",
                table: "report_template_sql",
                column: "report_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_template_sql_parameter_parameter_id",
                table: "report_template_sql_parameter",
                column: "parameter_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "report_template_sql_parameter");

            migrationBuilder.DropTable(
                name: "report_template_parameter");

            migrationBuilder.DropTable(
                name: "report_template_sql");

            migrationBuilder.DropTable(
                name: "report_template");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "policy_contract",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                comment: "Loại hợp đồng",
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldComment: "Loại hợp đồng:\n- individual: hợp đồng lẻ\n- group: hợp đồng nhóm");
        }
    }
}
