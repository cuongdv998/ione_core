using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "certificate_no",
                table: "claim",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Số GCN (Giấy chứng nhận bảo hiểm)");

            migrationBuilder.AddColumn<Guid>(
                name: "process_dept_id",
                table: "claim",
                type: "uuid",
                nullable: true,
                comment: "Đơn vị giám định (khi assign)");

            migrationBuilder.AddColumn<Guid>(
                name: "process_emp_id",
                table: "claim",
                type: "uuid",
                nullable: true,
                comment: "Người giám định (khi assign)");

            migrationBuilder.CreateIndex(
                name: "IX_claim_process_dept_id",
                table: "claim",
                column: "process_dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_claim_process_emp_id",
                table: "claim",
                column: "process_emp_id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_hr_department_process_dept_id",
                table: "claim",
                column: "process_dept_id",
                principalTable: "hr_department",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_hr_employee_process_emp_id",
                table: "claim",
                column: "process_emp_id",
                principalTable: "hr_employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_claim_hr_department_process_dept_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_hr_employee_process_emp_id",
                table: "claim");

            migrationBuilder.DropIndex(
                name: "IX_claim_process_dept_id",
                table: "claim");

            migrationBuilder.DropIndex(
                name: "IX_claim_process_emp_id",
                table: "claim");

            migrationBuilder.DropColumn(
                name: "certificate_no",
                table: "claim");

            migrationBuilder.DropColumn(
                name: "process_dept_id",
                table: "claim");

            migrationBuilder.DropColumn(
                name: "process_emp_id",
                table: "claim");
        }
    }
}
