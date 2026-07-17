using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProductPlanDefinitionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROPRODUCTPLANDEFINITION_pro_rule_type_RULETYPEID",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropIndex(
                name: "ix_proproductplandefinition_code",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropIndex(
                name: "IX_PROPRODUCTPLANDEFINITION_RULETYPEID",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropColumn(
                name: "APPLYTO",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropColumn(
                name: "APPLYTOID",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropColumn(
                name: "CODE",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropColumn(
                name: "EFFECTDATE",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropColumn(
                name: "EXPIREDATE",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropColumn(
                name: "PRIORITY",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropColumn(
                name: "RULESCRIPT",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropColumn(
                name: "RULETYPEID",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.RenameColumn(
                name: "NAME",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "PLANNAME");

            migrationBuilder.AlterColumn<Guid>(
                name: "PRODUCTID",
                table: "PROPRODUCTPLANDEFINITION",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PLANCODE",
                table: "PROPRODUCTPLANDEFINITION",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                comment: "Nếu có khai mã PLAN_OPTIONAL thì sản phẩm này cho phép ngoài gói cố định, có thể tùy chỉnh khi cấp đơn");

            migrationBuilder.CreateIndex(
                name: "ix_proproductplandefinition_plancode",
                table: "PROPRODUCTPLANDEFINITION",
                column: "PLANCODE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_proproductplandefinition_plancode",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropColumn(
                name: "PLANCODE",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.RenameColumn(
                name: "PLANNAME",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "NAME");

            migrationBuilder.AlterColumn<Guid>(
                name: "PRODUCTID",
                table: "PROPRODUCTPLANDEFINITION",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "APPLYTO",
                table: "PROPRODUCTPLANDEFINITION",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                comment: "Phạm vi áp dụng: product (mức sản phẩm), coverage (mức phạm vi), level (mức hạn mức của phạm vi)");

            migrationBuilder.AddColumn<Guid>(
                name: "APPLYTOID",
                table: "PROPRODUCTPLANDEFINITION",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CODE",
                table: "PROPRODUCTPLANDEFINITION",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "PROPRODUCTPLANDEFINITION",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EFFECTDATE",
                table: "PROPRODUCTPLANDEFINITION",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EXPIREDATE",
                table: "PROPRODUCTPLANDEFINITION",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PRIORITY",
                table: "PROPRODUCTPLANDEFINITION",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                comment: "Thứ tự ưu tiên thực thi, số nhỏ ưu tiên trước");

            migrationBuilder.AddColumn<string>(
                name: "RULESCRIPT",
                table: "PROPRODUCTPLANDEFINITION",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RULETYPEID",
                table: "PROPRODUCTPLANDEFINITION",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_proproductplandefinition_code",
                table: "PROPRODUCTPLANDEFINITION",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROPRODUCTPLANDEFINITION_RULETYPEID",
                table: "PROPRODUCTPLANDEFINITION",
                column: "RULETYPEID");

            migrationBuilder.AddForeignKey(
                name: "FK_PROPRODUCTPLANDEFINITION_pro_rule_type_RULETYPEID",
                table: "PROPRODUCTPLANDEFINITION",
                column: "RULETYPEID",
                principalTable: "pro_rule_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
