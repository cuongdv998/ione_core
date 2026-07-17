using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddResSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_sequence",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    prefix = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    suffix = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Phân loại:\n- normal: thông thường (có thể nhảy cóc mà ko cần quan tâm thứ tự cấp phát số tăng dần)\n- no_gap: cần lock transaction để cấp phát số theo thứ tự"),
                    padding = table.Column<int>(type: "integer", nullable: true, comment: "Số ký tự tối đa của số sinh tự động, nếu số nhỏ hơn số này thì sẽ thêm số 0 ở đầu để đủ số ký tự"),
                    number_next = table.Column<long>(type: "bigint", nullable: false, comment: "Số tiếp theo sẽ được sinh ra"),
                    number_increment = table.Column<int>(type: "integer", nullable: false, comment: "Số bước nhảy, thường là 1"),
                    use_date_range = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Số sẽ được reset sau một khoảng thời gian:\n- Y: Có\n- N: Không (mặc định)"),
                    date_range_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, comment: "Loại thời gian mà số tự sinh sẽ được reset:\n- week: sang tuần mới sẽ reset\n- month: sang tháng mới sẽ reset\n- quater: sang quý mới sẽ reset\n- half: sang nửa năm tiếp theo sẽ reset\n- year: sang năm mới sẽ reset"),
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
                    table.PrimaryKey("pk_res_sequence", x => x.id);
                },
                comment: "Bảng cấu hình các mã tự sinh của hệ thống");

            migrationBuilder.CreateIndex(
                name: "ix_res_sequence_code",
                table: "res_sequence",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ Sử dụng IF EXISTS để đảm bảo migration có thể chạy trên database mới hoặc đã có dữ liệu
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_sequence_code"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_sequence"";");
        }
    }
}
