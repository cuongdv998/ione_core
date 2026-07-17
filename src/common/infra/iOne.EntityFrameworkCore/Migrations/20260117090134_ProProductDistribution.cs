using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProductDistribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PROPRODUCTDISTRIBUTION",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUCTID = table.Column<Guid>(type: "uuid", nullable: false),
                    CHANNELID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Kênh phân phối"),
                    APPCHANNELID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Kênh ứng dụng (app mobile, web ....)"),
                    EMPLOYEEROLEID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Nhân viên bán hàng"),
                    STATUS = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động"),
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
                    table.PrimaryKey("PK_PROPRODUCTDISTRIBUTION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PROPRODUCTDISTRIBUTION_hr_employee_role_EMPLOYEEROLEID",
                        column: x => x.EMPLOYEEROLEID,
                        principalTable: "hr_employee_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PROPRODUCTDISTRIBUTION_pro_product_PRODUCTID",
                        column: x => x.PRODUCTID,
                        principalTable: "pro_product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PROPRODUCTDISTRIBUTION_res_app_channel_APPCHANNELID",
                        column: x => x.APPCHANNELID,
                        principalTable: "res_app_channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PROPRODUCTDISTRIBUTION_res_channel_CHANNELID",
                        column: x => x.CHANNELID,
                        principalTable: "res_channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Thiết lập quyền phân phối sản phẩm");

            migrationBuilder.CreateIndex(
                name: "IX_PROPRODUCTDISTRIBUTION_APPCHANNELID",
                table: "PROPRODUCTDISTRIBUTION",
                column: "APPCHANNELID");

            migrationBuilder.CreateIndex(
                name: "IX_PROPRODUCTDISTRIBUTION_CHANNELID",
                table: "PROPRODUCTDISTRIBUTION",
                column: "CHANNELID");

            migrationBuilder.CreateIndex(
                name: "IX_PROPRODUCTDISTRIBUTION_EMPLOYEEROLEID",
                table: "PROPRODUCTDISTRIBUTION",
                column: "EMPLOYEEROLEID");

            migrationBuilder.CreateIndex(
                name: "IX_PROPRODUCTDISTRIBUTION_PRODUCTID",
                table: "PROPRODUCTDISTRIBUTION",
                column: "PRODUCTID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROPRODUCTDISTRIBUTION");
        }
    }
}
