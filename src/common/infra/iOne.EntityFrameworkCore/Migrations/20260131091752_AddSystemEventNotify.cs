using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemEventNotify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "system_event_notify",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    app_channel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Tiêu đề bản tin, đã được thay thế tham số"),
                    body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Nội dung bản tin, đã được thay thế tham số"),
                    payload = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Thông tin mô tả hành xử cho các hệ thống nhận tin, đã được thay thế các tham số"),
                    recipient_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Loại đối tượng nhận tin: cus (khách hàng), emp (nhân viên)"),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Id tương ứng với nhân viên hoặc khách hàng"),
                    recipient = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Địa chỉ nhận tin"),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái: pending (chờ gửi), sent (đã gửi), fail (lỗi), read (đã đọc), deactive (đã xóa)"),
                    schedule_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Thời gian dự kiến gửi"),
                    sent_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Thời điểm gửi"),
                    read_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Thời điểm đọc"),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Bản tin lỗi nếu có lỗi xảy ra"),
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
                    table.PrimaryKey("PK_system_event_notify", x => x.id);
                },
                comment: "Lưu các bản tin cần gửi của hệ thống (chưa gửi, hoặc gửi fail)");

            migrationBuilder.CreateIndex(
                name: "ix_system_event_notify_event_code",
                table: "system_event_notify",
                column: "event_code");

            migrationBuilder.CreateIndex(
                name: "ix_system_event_notify_schedule_at",
                table: "system_event_notify",
                column: "schedule_at");

            migrationBuilder.CreateIndex(
                name: "ix_system_event_notify_status",
                table: "system_event_notify",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_event_notify");
        }
    }
}
