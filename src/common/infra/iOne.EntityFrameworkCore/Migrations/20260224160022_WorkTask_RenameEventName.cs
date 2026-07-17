using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class WorkTask_RenameEventName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventName",
                table: "work_task",
                newName: "event_name");

            migrationBuilder.AlterColumn<string>(
                name: "event_name",
                table: "work_task",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Event name",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "driver_id_no",
                table: "claim_incident_risk_motor",
                type: "character varying(25)",
                maxLength: 25,
                nullable: true,
                comment: "Số CCCD của lái xe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "driver_id_no",
                table: "claim_incident_risk_motor");

            migrationBuilder.RenameColumn(
                name: "event_name",
                table: "work_task",
                newName: "EventName");

            migrationBuilder.AlterColumn<string>(
                name: "EventName",
                table: "work_task",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Event name");
        }
    }
}
