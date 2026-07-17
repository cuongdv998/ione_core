using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddTableResCarCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_car_category",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    car_brand_id = table.Column<Guid>(type: "uuid", nullable: false),
                    car_model_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motor_class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    car_line_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    seat_number = table.Column<decimal>(type: "numeric(2,0)", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: active (hoạt động), deactive (không hoạt động)"),
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
                    table.PrimaryKey("PK_res_car_category", x => x.id);
                    table.ForeignKey(
                        name: "FK_res_car_category_res_car_brand_car_brand_id",
                        column: x => x.car_brand_id,
                        principalTable: "res_car_brand",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_res_car_category_res_car_model_car_model_id",
                        column: x => x.car_model_id,
                        principalTable: "res_car_model",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Định nghĩa Phiên bản xe");

            migrationBuilder.CreateIndex(
                name: "IX_res_car_category_car_brand_id",
                table: "res_car_category",
                column: "car_brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_res_car_category_car_model_id",
                table: "res_car_category",
                column: "car_model_id");

            migrationBuilder.CreateIndex(
                name: "ix_res_car_category_code",
                table: "res_car_category",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "res_car_category");
        }
    }
}
