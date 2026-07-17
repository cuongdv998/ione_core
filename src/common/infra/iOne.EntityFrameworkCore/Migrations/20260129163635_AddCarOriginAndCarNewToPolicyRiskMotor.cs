using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddCarOriginAndCarNewToPolicyRiskMotor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "car_new",
                table: "policy_risk_motor",
                type: "character varying(1)",
                maxLength: 1,
                nullable: true,
                comment: "Xe mới (Y/N)");

            migrationBuilder.AddColumn<string>(
                name: "car_origin",
                table: "policy_risk_motor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Nguồn gốc xe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "car_new",
                table: "policy_risk_motor");

            migrationBuilder.DropColumn(
                name: "car_origin",
                table: "policy_risk_motor");
        }
    }
}
