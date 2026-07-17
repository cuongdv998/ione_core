using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class WorkTask_AddResBusinessAssignee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "res_business_assignee_id",
                table: "work_task",
                type: "uuid",
                nullable: true,
                comment: "Cấu hình phân cấp duyệt áp dụng khi tạo task");

            migrationBuilder.CreateIndex(
                name: "IX_work_task_res_business_assignee_id",
                table: "work_task",
                column: "res_business_assignee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_work_task_res_business_assignee_res_business_assignee_id",
                table: "work_task",
                column: "res_business_assignee_id",
                principalTable: "res_business_assignee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_work_task_res_business_assignee_res_business_assignee_id",
                table: "work_task");

            migrationBuilder.DropIndex(
                name: "IX_work_task_res_business_assignee_id",
                table: "work_task");

            migrationBuilder.DropColumn(
                name: "res_business_assignee_id",
                table: "work_task");
        }
    }
}
