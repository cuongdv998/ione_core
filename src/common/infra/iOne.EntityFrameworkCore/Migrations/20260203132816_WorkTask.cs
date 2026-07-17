using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class WorkTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "work_instance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_instance_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    business_flow_id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_code = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    business_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    business_key = table.Column<Guid>(type: "uuid", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: new, inprogress, completed, accepted, rejected, cancelled, wait_approve, approved, pending, return"),
                    BusinessFlowId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_instance", x => x.id);
                    table.ForeignKey(
                        name: "FK_work_instance_business_flow_BusinessFlowId1",
                        column: x => x.BusinessFlowId1,
                        principalTable: "business_flow",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_work_instance_business_flow_business_flow_id",
                        column: x => x.business_flow_id,
                        principalTable: "business_flow",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu thông tin WorkFlow xử lý");

            migrationBuilder.CreateTable(
                name: "work_task",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    business_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    business_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Tên bảng liên quan đến task"),
                    business_key = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID của bản ghi liên quan đến task"),
                    form_key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Link đến form nghiệp vụ tương ứng"),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Mã task"),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Tên Task"),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    reporter_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Người gán việc"),
                    business_authority_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã phân cấp duyệt"),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người nhận việc"),
                    assignee_department_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Đơn vị của người nhận việc"),
                    assignee_organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: new, inprogress, completed, accepted, rejected, cancelled, wait_approve, approved, pending, return"),
                    priority = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Mức ưu tiên: high, medium, low"),
                    task_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày dự kiến bắt đầu task"),
                    end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày dự kiến kết thúc task"),
                    actual_start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày bắt đầu thực tế task"),
                    actual_end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày kết thúc task"),
                    reason_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Lý do (nếu từ chối)"),
                    reason_description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Mô tả lý do"),
                    WorkInstanceId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    TaskCategoryId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_work_task", x => x.id);
                    table.ForeignKey(
                        name: "FK_work_task_res_task_category_TaskCategoryId1",
                        column: x => x.TaskCategoryId1,
                        principalTable: "res_task_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_work_task_res_task_category_task_category_id",
                        column: x => x.task_category_id,
                        principalTable: "res_task_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_task_work_instance_WorkInstanceId1",
                        column: x => x.WorkInstanceId1,
                        principalTable: "work_instance",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_work_task_work_instance_work_instance_id",
                        column: x => x.work_instance_id,
                        principalTable: "work_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu các task thực hiện");

            migrationBuilder.CreateIndex(
                name: "IX_work_instance_business_flow_id",
                table: "work_instance",
                column: "business_flow_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_instance_BusinessFlowId1",
                table: "work_instance",
                column: "BusinessFlowId1");

            migrationBuilder.CreateIndex(
                name: "IX_work_task_task_category_id",
                table: "work_task",
                column: "task_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_task_TaskCategoryId1",
                table: "work_task",
                column: "TaskCategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_work_task_work_instance_id",
                table: "work_task",
                column: "work_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_task_WorkInstanceId1",
                table: "work_task",
                column: "WorkInstanceId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "work_task");

            migrationBuilder.DropTable(
                name: "work_instance");
        }
    }
}
