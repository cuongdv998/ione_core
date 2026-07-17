using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AdminConfig_AddSubCodeLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "sub_code",
                table: "admin_config",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                comment: "Mã con (nếu một cấu hình lớn cần chia nhiều cấu hình con)",
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25,
                oldComment: "Mã con (nếu một cấu hình lớn cần chia nhiều cấu hình con)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "sub_code",
                table: "admin_config",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                comment: "Mã con (nếu một cấu hình lớn cần chia nhiều cấu hình con)",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldComment: "Mã con (nếu một cấu hình lớn cần chia nhiều cấu hình con)");
        }
    }
}
