using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddCarPlateType2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "car_plate_type",
                table: "policy_risk_motor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Loại biển số xe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "car_plate_type",
                table: "policy_risk_motor");
        }
    }
}
