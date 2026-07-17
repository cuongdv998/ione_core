using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProductPlanDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PROPRODUCTPLANDEFINITION",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    APPLYTO = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Phạm vi áp dụng: product (mức sản phẩm), coverage (mức phạm vi), level (mức hạn mức của phạm vi)"),
                    APPLYTOID = table.Column<Guid>(type: "uuid", nullable: false),
                    RULETYPEID = table.Column<Guid>(type: "uuid", nullable: false),
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NAME = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RULESCRIPT = table.Column<string>(type: "text", nullable: false),
                    PRIORITY = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "Thứ tự ưu tiên thực thi, số nhỏ ưu tiên trước"),
                    STATUS = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: active (hoạt động), deactive (không hoạt động)"),
                    EFFECTDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EXPIREDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PRODUCTID = table.Column<Guid>(type: "uuid", nullable: true),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: true),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROPRODUCTPLANDEFINITION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PROPRODUCTPLANDEFINITION_pro_product_PRODUCTID",
                        column: x => x.PRODUCTID,
                        principalTable: "pro_product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PROPRODUCTPLANDEFINITION_pro_rule_type_RULETYPEID",
                        column: x => x.RULETYPEID,
                        principalTable: "pro_rule_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng định nghĩa các gói của sản phẩm gốc");

            migrationBuilder.CreateIndex(
                name: "ix_proproductplandefinition_code",
                table: "PROPRODUCTPLANDEFINITION",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROPRODUCTPLANDEFINITION_PRODUCTID",
                table: "PROPRODUCTPLANDEFINITION",
                column: "PRODUCTID");

            migrationBuilder.CreateIndex(
                name: "IX_PROPRODUCTPLANDEFINITION_RULETYPEID",
                table: "PROPRODUCTPLANDEFINITION",
                column: "RULETYPEID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROPRODUCTPLANDEFINITION");
        }
    }
}
