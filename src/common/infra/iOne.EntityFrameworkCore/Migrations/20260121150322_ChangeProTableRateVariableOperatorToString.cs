using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProTableRateVariableOperatorToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
