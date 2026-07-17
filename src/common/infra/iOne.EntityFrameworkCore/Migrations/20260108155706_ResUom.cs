using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ResUom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_uom",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái: active (hiệu lực), deactive (hết hiệu lực)"),
                    rounding = table.Column<decimal>(type: "numeric(2,4)", nullable: false, defaultValue: 0.001m, comment: "Độ chính xác làm tròn"),
                    factor = table.Column<decimal>(type: "numeric(6,6)", nullable: true, defaultValue: 1m, comment: "Hệ số chuyển đổi"),
                    type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "none", comment: "Loại: ref (đơn vị cơ sở), none (đơn vị chuẩn)"),
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
                    table.PrimaryKey("PK_res_uom", x => x.id);
                    table.ForeignKey(
                        name: "FK_res_uom_res_uom_class_class_id",
                        column: x => x.class_id,
                        principalTable: "res_uom_class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng đơn vị tính");

            migrationBuilder.CreateIndex(
                name: "ix_res_uom_class_id",
                table: "res_uom",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_uom_code",
                table: "res_uom",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "res_uom");
        }
    }
}
