using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ResPartnerEventMessage_Add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_partner_message_logging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    partner_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    api_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    http_method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    request_body = table.Column<string>(type: "text", nullable: true),
                    response_body = table.Column<string>(type: "text", nullable: true),
                    http_status_code = table.Column<int>(type: "integer", nullable: true),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    duration_ms = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_res_partner_message_logging", x => x.id);
                },
                comment: "Lưu log request/response khi gọi API đối tác bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_message_logging_partner_code",
                table: "res_partner_message_logging",
                column: "partner_code");

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_message_logging_policy_id",
                table: "res_partner_message_logging",
                column: "policy_id");

            migrationBuilder.AddForeignKey(
                name: "FK_policy_product_pro_product_product_id",
                table: "policy_product",
                column: "product_id",
                principalTable: "pro_product",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_policy_product_pro_product_product_id",
                table: "policy_product");

            migrationBuilder.DropTable(
                name: "res_partner_message_logging");
        }
    }
}
