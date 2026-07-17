using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMineType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "mime_type",
                table: "res_document",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "Loại file",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldComment: "Loại file");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "policy_contract",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                comment: "Loại hợp đồng:\n- individual: hợp đồng lẻ\n- group: hợp đồng nhóm",
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldComment: "Loại hợp đồng");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "mime_type",
                table: "res_document",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                comment: "Loại file",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "Loại file");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "policy_contract",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                comment: "Loại hợp đồng",
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldComment: "Loại hợp đồng:\n- individual: hợp đồng lẻ\n- group: hợp đồng nhóm");
        }
    }
}
