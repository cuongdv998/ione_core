using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class IncreasePayerAddressLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "risk_object_address",
                table: "policy_risk_object",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                comment: "Địa chỉ đối tượng bảo hiểm",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Địa chỉ đối tượng bảo hiểm");

            migrationBuilder.AlterColumn<string>(
                name: "rep_address",
                table: "policy_risk_object",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                comment: "Địa chỉ người đại diện",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Địa chỉ người đại diện");

            migrationBuilder.AlterColumn<string>(
                name: "payer_name",
                table: "policy_contract",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "payer_full_address",
                table: "policy_contract",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "insured_address",
                table: "policy",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                comment: "Địa chỉ người được bảo hiểm",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Địa chỉ người được bảo hiểm");

            migrationBuilder.AlterColumn<string>(
                name: "beneficiary_address",
                table: "policy",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                comment: "Địa chỉ người thụ hưởng",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Địa chỉ người thụ hưởng");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "risk_object_address",
                table: "policy_risk_object",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Địa chỉ đối tượng bảo hiểm",
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true,
                oldComment: "Địa chỉ đối tượng bảo hiểm");

            migrationBuilder.AlterColumn<string>(
                name: "rep_address",
                table: "policy_risk_object",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Địa chỉ người đại diện",
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true,
                oldComment: "Địa chỉ người đại diện");

            migrationBuilder.AlterColumn<string>(
                name: "payer_name",
                table: "policy_contract",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "payer_full_address",
                table: "policy_contract",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "insured_address",
                table: "policy",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Địa chỉ người được bảo hiểm",
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true,
                oldComment: "Địa chỉ người được bảo hiểm");

            migrationBuilder.AlterColumn<string>(
                name: "beneficiary_address",
                table: "policy",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Địa chỉ người thụ hưởng",
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true,
                oldComment: "Địa chỉ người thụ hưởng");
        }
    }
}
