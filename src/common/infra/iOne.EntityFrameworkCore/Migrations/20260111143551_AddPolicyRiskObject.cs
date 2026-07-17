using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyRiskObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policy_risk_object",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến đơn bảo hiểm gốc"),
                    policy_version_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tham chiếu đến phiên bản đơn bảo hiểm"),
                    object_type_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Loại đối tượng bảo hiểm"),
                    rep_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Tên người đại diện"),
                    rep_id_no = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Số CMND/CCCD người đại diện"),
                    rep_passport = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Hộ chiếu người đại diện"),
                    rep_phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Số điện thoại người đại diện"),
                    rep_email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Email người đại diện"),
                    rep_province_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tỉnh/Thành phố người đại diện"),
                    rep_ward_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phường/Xã người đại diện"),
                    rep_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Địa chỉ người đại diện"),
                    rep_full_address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Địa chỉ đầy đủ người đại diện"),
                    risk_object_province_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tỉnh/Thành phố đối tượng bảo hiểm"),
                    risk_object_ward_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phường/Xã đối tượng bảo hiểm"),
                    risk_object_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Địa chỉ đối tượng bảo hiểm"),
                    risk_object_full_address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Địa chỉ đầy đủ đối tượng bảo hiểm"),
                    risk_object_lat = table.Column<double>(type: "FLOAT8", nullable: true, comment: "Vĩ độ đối tượng bảo hiểm"),
                    risk_object_long = table.Column<double>(type: "FLOAT8", nullable: true, comment: "Kinh độ đối tượng bảo hiểm"),
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
                    table.PrimaryKey("PK_policy_risk_object", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_risk_object_object_type_id",
                        column: x => x.object_type_id,
                        principalTable: "res_object_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_risk_object_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_risk_object_policy_version_id",
                        column: x => x.policy_version_id,
                        principalTable: "policy_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu đối tượng bảo hiểm của đơn bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "ix_policy_risk_object_object_type_id",
                table: "policy_risk_object",
                column: "object_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_risk_object_policy_id",
                table: "policy_risk_object",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_risk_object_policy_version_id",
                table: "policy_risk_object",
                column: "policy_version_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_risk_object");
        }
    }
}
