using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class updaterecipientcol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "recipient",
                table: "system_event_notify",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Địa chỉ nhận tin",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Địa chỉ nhận tin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "recipient",
                table: "system_event_notify",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Địa chỉ nhận tin",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Địa chỉ nhận tin");
        }
    }
}
