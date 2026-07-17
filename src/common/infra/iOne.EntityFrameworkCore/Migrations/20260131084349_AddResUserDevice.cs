using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddResUserDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_user_device",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    device_uid = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    device_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    app_channel_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    effect_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expir_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"),
                    os = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    device_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_res_user_device", x => x.id);
                },
                comment: "Bảng lưu trữ định danh thiết bị mobile của user để gửi notify đến app");

            migrationBuilder.CreateIndex(
                name: "uq_res_user_device_username_device_uid_app_channel_code",
                table: "res_user_device",
                columns: new[] { "username", "device_uid", "app_channel_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "res_user_device");
        }
    }
}
