using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddResEventAndNotifyTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_event",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"),
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
                    table.PrimaryKey("PK_res_event", x => x.id);
                },
                comment: "Danh sách sự kiện hệ thống và thiết lập template cảnh báo");

            migrationBuilder.CreateTable(
                name: "res_event_notify_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_channel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    retry_number = table.Column<decimal>(type: "numeric(2,0)", nullable: false, comment: "Số lần retry, mặc định là 0"),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Tiêu đề bản tin, có thể có tham số truyền vào, nếu có thêm số thì để dạng ${param_name}"),
                    body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Nội dung của bản tin, có thể có tham số truyền vào, nếu có tham số truyền vào thì tham số có dạng ${param_name}"),
                    data = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Data có dạng json:\n{\n\"screen\": \"invoice_detail\",\n\"invoice_id\": \"INV20250911001\",\n....\n}"),
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
                    table.PrimaryKey("PK_res_event_notify_template", x => x.id);
                    table.ForeignKey(
                        name: "fk_res_event_notify_template_app_channel_id",
                        column: x => x.app_channel_id,
                        principalTable: "res_app_channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_res_event_notify_template_event_id",
                        column: x => x.event_id,
                        principalTable: "res_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Template cảnh báo tương ứng với sự kiện");

            migrationBuilder.CreateIndex(
                name: "ix_res_event_code",
                table: "res_event",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_res_event_notify_template_app_channel_id",
                table: "res_event_notify_template",
                column: "app_channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_event_notify_template_event_id",
                table: "res_event_notify_template",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "uq_res_event_notify_template_event_channel",
                table: "res_event_notify_template",
                columns: new[] { "event_id", "app_channel_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "res_event_notify_template");

            migrationBuilder.DropTable(
                name: "res_event");
        }
    }
}
