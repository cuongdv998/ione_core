using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class FkKeyClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_claim_hr_employee_cancel_employee_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_hr_employee_close_employee_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_hr_employee_open_employee_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_pro_line_of_business_lob_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_res_claim_type_claim_type_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_res_partner_insurer_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_res_reason_cancel_reason_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_incident_claim_incident_id",
                table: "claim_incident");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_incident_res_incident_cause_incident_cause_id",
                table: "claim_incident");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_incident_res_incident_level_incident_level_id",
                table: "claim_incident");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_incident_res_object_type_object_type_id",
                table: "claim_incident");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_incident_res_partner_assessment_partner_id",
                table: "claim_incident");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_incident_res_province_incident_province_id",
                table: "claim_incident");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_incident_res_ward_incident_ward_id",
                table: "claim_incident");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_incident_risk_motor_claim_incident_incident_object_id",
                table: "claim_incident_risk_motor");

            migrationBuilder.DropIndex(
                name: "IX_claim_incident_assessment_partner_id",
                table: "claim_incident");

            migrationBuilder.DropIndex(
                name: "IX_claim_incident_incident_level_id",
                table: "claim_incident");

            migrationBuilder.DropIndex(
                name: "IX_claim_incident_incident_province_id",
                table: "claim_incident");

            migrationBuilder.DropIndex(
                name: "IX_claim_incident_incident_ward_id",
                table: "claim_incident");

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIM_REFERENCE_PROLINEO",
                table: "claim",
                column: "lob_id",
                principalTable: "pro_line_of_business",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIM_REFERENCE_RESCLAIM",
                table: "claim",
                column: "claim_type_id",
                principalTable: "res_claim_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIM_REFERENCE_RESPARTN",
                table: "claim",
                column: "insurer_id",
                principalTable: "res_partner",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_hr_employee_cancel_employee_id",
                table: "claim",
                column: "cancel_employee_id",
                principalTable: "hr_employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_hr_employee_close_employee_id",
                table: "claim",
                column: "close_employee_id",
                principalTable: "hr_employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_hr_employee_open_employee_id",
                table: "claim",
                column: "open_employee_id",
                principalTable: "hr_employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_res_reason_cancel_reason_id",
                table: "claim",
                column: "cancel_reason_id",
                principalTable: "res_reason",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIMINC_REFERENCE_CLAIMINC",
                table: "claim_incident_risk_motor",
                column: "incident_object_id",
                principalTable: "claim_incident",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CLAIM_REFERENCE_PROLINEO",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIM_REFERENCE_RESCLAIM",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIM_REFERENCE_RESPARTN",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_hr_employee_cancel_employee_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_hr_employee_close_employee_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_hr_employee_open_employee_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_res_reason_cancel_reason_id",
                table: "claim");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIMINC_REFERENCE_CLAIMINC",
                table: "claim_incident_risk_motor");

            migrationBuilder.CreateIndex(
                name: "IX_claim_incident_assessment_partner_id",
                table: "claim_incident",
                column: "assessment_partner_id");

            migrationBuilder.CreateIndex(
                name: "IX_claim_incident_incident_level_id",
                table: "claim_incident",
                column: "incident_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_claim_incident_incident_province_id",
                table: "claim_incident",
                column: "incident_province_id");

            migrationBuilder.CreateIndex(
                name: "IX_claim_incident_incident_ward_id",
                table: "claim_incident",
                column: "incident_ward_id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_hr_employee_cancel_employee_id",
                table: "claim",
                column: "cancel_employee_id",
                principalTable: "hr_employee",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_hr_employee_close_employee_id",
                table: "claim",
                column: "close_employee_id",
                principalTable: "hr_employee",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_hr_employee_open_employee_id",
                table: "claim",
                column: "open_employee_id",
                principalTable: "hr_employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_pro_line_of_business_lob_id",
                table: "claim",
                column: "lob_id",
                principalTable: "pro_line_of_business",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_res_claim_type_claim_type_id",
                table: "claim",
                column: "claim_type_id",
                principalTable: "res_claim_type",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_res_partner_insurer_id",
                table: "claim",
                column: "insurer_id",
                principalTable: "res_partner",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_res_reason_cancel_reason_id",
                table: "claim",
                column: "cancel_reason_id",
                principalTable: "res_reason",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_incident_claim_incident_id",
                table: "claim_incident",
                column: "incident_id",
                principalTable: "claim",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_incident_res_incident_cause_incident_cause_id",
                table: "claim_incident",
                column: "incident_cause_id",
                principalTable: "res_incident_cause",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_incident_res_incident_level_incident_level_id",
                table: "claim_incident",
                column: "incident_level_id",
                principalTable: "res_incident_level",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_incident_res_object_type_object_type_id",
                table: "claim_incident",
                column: "object_type_id",
                principalTable: "res_object_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_incident_res_partner_assessment_partner_id",
                table: "claim_incident",
                column: "assessment_partner_id",
                principalTable: "res_partner",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_incident_res_province_incident_province_id",
                table: "claim_incident",
                column: "incident_province_id",
                principalTable: "res_province",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_incident_res_ward_incident_ward_id",
                table: "claim_incident",
                column: "incident_ward_id",
                principalTable: "res_ward",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_incident_risk_motor_claim_incident_incident_object_id",
                table: "claim_incident_risk_motor",
                column: "incident_object_id",
                principalTable: "claim_incident",
                principalColumn: "id");
        }
    }
}
