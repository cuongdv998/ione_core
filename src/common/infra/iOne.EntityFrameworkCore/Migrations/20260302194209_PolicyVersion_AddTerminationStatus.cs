using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class PolicyVersion_AddTerminationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "termination_status",
                table: "policy_version",
                type: "integer",
                nullable: true,
                comment: "Trạng thái chấm dứt: 0-Pending (chờ duyệt chấm dứt), 1-Approved (đã duyệt chấm dứt), 2-Rejected (từ chối chấm dứt)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "termination_status",
                table: "policy_version");
        }
    }
}
