using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyRiskMotor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policy_risk_motor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_risk_object_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tham chiếu đến đối tượng bảo hiểm"),
                    risk_object_value = table.Column<decimal>(type: "numeric(15,3)", nullable: true, comment: "Giá trị đối tượng bảo hiểm"),
                    motor_class_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã nhóm xe cơ giới"),
                    car_line_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Dòng xe"),
                    car_group_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Nhóm xe"),
                    car_type_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Loại xe"),
                    car_brand_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Hãng xe"),
                    car_model_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Model xe"),
                    car_category_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Phân loại xe"),
                    car_usage = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Mục đích sử dụng xe"),
                    car_old = table.Column<decimal>(type: "numeric(6,3)", nullable: true, comment: "Số năm sử dụng xe"),
                    car_production_year = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Năm sản xuất xe"),
                    car_plate = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Biển số xe"),
                    car_plate_clear = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Biển số xe (chuẩn hóa, không dấu/ký tự đặc biệt)"),
                    car_seat_number = table.Column<decimal>(type: "NUMERIC(2)", nullable: true, comment: "Số chỗ ngồi"),
                    car_vin = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true, comment: "Số khung xe (VIN)"),
                    car_engine_number = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true, comment: "Số máy"),
                    car_payload_capacity = table.Column<decimal>(type: "numeric(6,3)", nullable: true, comment: "Tải trọng xe"),
                    car_color = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Màu xe"),
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
                    table.PrimaryKey("PK_policy_risk_motor", x => x.id);
                    table.ForeignKey(
                        name: "fk_policy_risk_motor_policy_risk_object_id",
                        column: x => x.policy_risk_object_id,
                        principalTable: "policy_risk_object",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu thông tin rủi ro xe cơ giới của đối tượng bảo hiểm");

            migrationBuilder.CreateIndex(
                name: "ix_policy_risk_motor_policy_risk_object_id",
                table: "policy_risk_motor",
                column: "policy_risk_object_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_risk_motor");
        }
    }
}
