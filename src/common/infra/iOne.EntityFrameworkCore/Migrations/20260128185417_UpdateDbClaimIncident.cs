using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbClaimIncident : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CLAIM_REFERENCE_PROLINEO",
                table: "CLAIM");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIM_REFERENCE_RESCLAIM",
                table: "CLAIM");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIM_REFERENCE_RESPARTN",
                table: "CLAIM");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIM_hr_employee_CANCELEMPLOYEEID",
                table: "CLAIM");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIM_hr_employee_CLOSEEMPLOYEEID",
                table: "CLAIM");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIM_hr_employee_OPENEMPLOYEEID",
                table: "CLAIM");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIM_res_reason_CANCELREASONID",
                table: "CLAIM");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIMINCIDENT_res_object_type_OBJECTTYPEID",
                table: "CLAIMINCIDENT");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIMINCIDENT_res_partner_ASSESSMENTPARTNERID",
                table: "CLAIMINCIDENT");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIMINCIDENT_res_province_INCIDENTPROVINCEID",
                table: "CLAIMINCIDENT");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIMINCIDENT_res_ward_INCIDENTWARDID",
                table: "CLAIMINCIDENT");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIMINC_REFERENCE_CLAIM",
                table: "CLAIMINCIDENT");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIMINC_REFERENCE_RESINCID_CAUSE",
                table: "CLAIMINCIDENT");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIMINC_REFERENCE_RESINCID_LEVEL",
                table: "CLAIMINCIDENT");

            migrationBuilder.DropForeignKey(
                name: "FK_CLAIMINC_REFERENCE_CLAIMINC",
                table: "CLAIMINCIDENTRISKMOTOR");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CLAIM",
                table: "CLAIM");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RESINCIDENTLEVEL",
                table: "RESINCIDENTLEVEL");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RESINCIDENTCAUSE",
                table: "RESINCIDENTCAUSE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CLAIMINCIDENTRISKMOTOR",
                table: "CLAIMINCIDENTRISKMOTOR");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CLAIMINCIDENT",
                table: "CLAIMINCIDENT");

            migrationBuilder.RenameTable(
                name: "CLAIM",
                newName: "claim");

            migrationBuilder.RenameTable(
                name: "RESINCIDENTLEVEL",
                newName: "res_incident_level");

            migrationBuilder.RenameTable(
                name: "RESINCIDENTCAUSE",
                newName: "res_incident_cause");

            migrationBuilder.RenameTable(
                name: "CLAIMINCIDENTRISKMOTOR",
                newName: "claim_incident_risk_motor");

            migrationBuilder.RenameTable(
                name: "CLAIMINCIDENT",
                newName: "claim_incident");

            migrationBuilder.RenameColumn(
                name: "STATUS",
                table: "claim",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "PRIORITY",
                table: "claim",
                newName: "priority");

            migrationBuilder.RenameColumn(
                name: "IS_DELETED",
                table: "claim",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DELETION_TIME",
                table: "claim",
                newName: "deletion_time");

            migrationBuilder.RenameColumn(
                name: "DELETER_ID",
                table: "claim",
                newName: "deleter_id");

            migrationBuilder.RenameColumn(
                name: "CONCURRENCY_STAMP",
                table: "claim",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "claim",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "claim",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SNAPSHOTLINK",
                table: "claim",
                newName: "snapshot_link");

            migrationBuilder.RenameColumn(
                name: "PROCESSCLAIMTYPE",
                table: "claim",
                newName: "process_claim_type");

            migrationBuilder.RenameColumn(
                name: "OPENEMPLOYEEID",
                table: "claim",
                newName: "open_employee_id");

            migrationBuilder.RenameColumn(
                name: "OPENDATE",
                table: "claim",
                newName: "open_date");

            migrationBuilder.RenameColumn(
                name: "NOTIFYDATE",
                table: "claim",
                newName: "notify_date");

            migrationBuilder.RenameColumn(
                name: "NOTIFIERPHONE",
                table: "claim",
                newName: "notifier_phone");

            migrationBuilder.RenameColumn(
                name: "NOTIFIERNAME",
                table: "claim",
                newName: "notifier_name");

            migrationBuilder.RenameColumn(
                name: "NOTIFIERINRELATIONSHIP",
                table: "claim",
                newName: "notifier_in_relationship");

            migrationBuilder.RenameColumn(
                name: "NOTIFIEREMAIL",
                table: "claim",
                newName: "notifier_email");

            migrationBuilder.RenameColumn(
                name: "LOBID",
                table: "claim",
                newName: "lob_id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFIERID",
                table: "claim",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFICATIONTIME",
                table: "claim",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "INSURERINCIDENTCODE",
                table: "claim",
                newName: "insurer_incident_code");

            migrationBuilder.RenameColumn(
                name: "INSURERID",
                table: "claim",
                newName: "insurer_id");

            migrationBuilder.RenameColumn(
                name: "EXTRA_PROPERTIES",
                table: "claim",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "CREATORID",
                table: "claim",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CREATIONTIME",
                table: "claim",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "CONTACTPHONE",
                table: "claim",
                newName: "contact_phone");

            migrationBuilder.RenameColumn(
                name: "CONTACTNAME",
                table: "claim",
                newName: "contact_name");

            migrationBuilder.RenameColumn(
                name: "CONTACTINRELATIONSHIP",
                table: "claim",
                newName: "contact_in_relationship");

            migrationBuilder.RenameColumn(
                name: "CONTACTEMAIL",
                table: "claim",
                newName: "contact_email");

            migrationBuilder.RenameColumn(
                name: "CLOSEEMPLOYEEID",
                table: "claim",
                newName: "close_employee_id");

            migrationBuilder.RenameColumn(
                name: "CLOSEDATE",
                table: "claim",
                newName: "close_date");

            migrationBuilder.RenameColumn(
                name: "CLAIMTYPEID",
                table: "claim",
                newName: "claim_type_id");

            migrationBuilder.RenameColumn(
                name: "CANCELREASONID",
                table: "claim",
                newName: "cancel_reason_id");

            migrationBuilder.RenameColumn(
                name: "CANCELNOTE",
                table: "claim",
                newName: "cancel_note");

            migrationBuilder.RenameColumn(
                name: "CANCELEMPLOYEEID",
                table: "claim",
                newName: "cancel_employee_id");

            migrationBuilder.RenameColumn(
                name: "CANCELDATE",
                table: "claim",
                newName: "cancel_date");

            migrationBuilder.RenameIndex(
                name: "IX_CLAIM_CLOSEEMPLOYEEID",
                table: "claim",
                newName: "IX_claim_close_employee_id");

            migrationBuilder.RenameIndex(
                name: "IX_CLAIM_CANCELREASONID",
                table: "claim",
                newName: "IX_claim_cancel_reason_id");

            migrationBuilder.RenameIndex(
                name: "IX_CLAIM_CANCELEMPLOYEEID",
                table: "claim",
                newName: "IX_claim_cancel_employee_id");

            migrationBuilder.RenameColumn(
                name: "STATUS",
                table: "res_incident_level",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "NAME",
                table: "res_incident_level",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "IS_DELETED",
                table: "res_incident_level",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DELETION_TIME",
                table: "res_incident_level",
                newName: "deletion_time");

            migrationBuilder.RenameColumn(
                name: "DELETER_ID",
                table: "res_incident_level",
                newName: "deleter_id");

            migrationBuilder.RenameColumn(
                name: "CONCURRENCY_STAMP",
                table: "res_incident_level",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "res_incident_level",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "res_incident_level",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFIERID",
                table: "res_incident_level",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFICATIONTIME",
                table: "res_incident_level",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "EXTRA_PROPERTIES",
                table: "res_incident_level",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "CREATORID",
                table: "res_incident_level",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CREATIONTIME",
                table: "res_incident_level",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "STATUS",
                table: "res_incident_cause",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "NAME",
                table: "res_incident_cause",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "IS_DELETED",
                table: "res_incident_cause",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DELETION_TIME",
                table: "res_incident_cause",
                newName: "deletion_time");

            migrationBuilder.RenameColumn(
                name: "DELETER_ID",
                table: "res_incident_cause",
                newName: "deleter_id");

            migrationBuilder.RenameColumn(
                name: "CONCURRENCY_STAMP",
                table: "res_incident_cause",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "CODE",
                table: "res_incident_cause",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "res_incident_cause",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFIERID",
                table: "res_incident_cause",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFICATIONTIME",
                table: "res_incident_cause",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "EXTRA_PROPERTIES",
                table: "res_incident_cause",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "CREATORID",
                table: "res_incident_cause",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CREATIONTIME",
                table: "res_incident_cause",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "VIN",
                table: "claim_incident_risk_motor",
                newName: "vin");

            migrationBuilder.RenameColumn(
                name: "IS_DELETED",
                table: "claim_incident_risk_motor",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DELETION_TIME",
                table: "claim_incident_risk_motor",
                newName: "deletion_time");

            migrationBuilder.RenameColumn(
                name: "DELETER_ID",
                table: "claim_incident_risk_motor",
                newName: "deleter_id");

            migrationBuilder.RenameColumn(
                name: "CONCURRENCY_STAMP",
                table: "claim_incident_risk_motor",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "claim_incident_risk_motor",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PERSONSONCAR",
                table: "claim_incident_risk_motor",
                newName: "persons_on_car");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFIERID",
                table: "claim_incident_risk_motor",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFICATIONTIME",
                table: "claim_incident_risk_motor",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "INCIDENTOBJECTID",
                table: "claim_incident_risk_motor",
                newName: "incident_object_id");

            migrationBuilder.RenameColumn(
                name: "EXTRA_PROPERTIES",
                table: "claim_incident_risk_motor",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "ENGINENUMBER",
                table: "claim_incident_risk_motor",
                newName: "engine_number");

            migrationBuilder.RenameColumn(
                name: "DRIVERSEX",
                table: "claim_incident_risk_motor",
                newName: "driver_sex");

            migrationBuilder.RenameColumn(
                name: "DRIVERPHONE",
                table: "claim_incident_risk_motor",
                newName: "driver_phone");

            migrationBuilder.RenameColumn(
                name: "DRIVERNAME",
                table: "claim_incident_risk_motor",
                newName: "driver_name");

            migrationBuilder.RenameColumn(
                name: "DRIVERLICENSENO",
                table: "claim_incident_risk_motor",
                newName: "driver_license_no");

            migrationBuilder.RenameColumn(
                name: "DRIVERLICENSELEVEL",
                table: "claim_incident_risk_motor",
                newName: "driver_license_level");

            migrationBuilder.RenameColumn(
                name: "DRIVERLICENSEEXPIREDATE",
                table: "claim_incident_risk_motor",
                newName: "driver_license_expire_date");

            migrationBuilder.RenameColumn(
                name: "DRIVERLICENSEEFFECTDATE",
                table: "claim_incident_risk_motor",
                newName: "driver_license_effect_date");

            migrationBuilder.RenameColumn(
                name: "CREATORID",
                table: "claim_incident_risk_motor",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CREATIONTIME",
                table: "claim_incident_risk_motor",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "CARREGISTRYNO",
                table: "claim_incident_risk_motor",
                newName: "car_registry_no");

            migrationBuilder.RenameColumn(
                name: "CARREGISTRYEXPIREDATE",
                table: "claim_incident_risk_motor",
                newName: "car_registry_expire_date");

            migrationBuilder.RenameColumn(
                name: "CARREGISTRYEFFECTDATE",
                table: "claim_incident_risk_motor",
                newName: "car_registry_effect_date");

            migrationBuilder.RenameColumn(
                name: "CARPLATE",
                table: "claim_incident_risk_motor",
                newName: "car_plate");

            migrationBuilder.RenameColumn(
                name: "NOTE",
                table: "claim_incident",
                newName: "note");

            migrationBuilder.RenameColumn(
                name: "IS_DELETED",
                table: "claim_incident",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DELETION_TIME",
                table: "claim_incident",
                newName: "deletion_time");

            migrationBuilder.RenameColumn(
                name: "DELETER_ID",
                table: "claim_incident",
                newName: "deleter_id");

            migrationBuilder.RenameColumn(
                name: "CONCURRENCY_STAMP",
                table: "claim_incident",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "claim_incident",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ONLOCATION",
                table: "claim_incident",
                newName: "on_location");

            migrationBuilder.RenameColumn(
                name: "OBJECTTYPEID",
                table: "claim_incident",
                newName: "object_type_id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFIERID",
                table: "claim_incident",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFICATIONTIME",
                table: "claim_incident",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "INCIDENTWARDID",
                table: "claim_incident",
                newName: "incident_ward_id");

            migrationBuilder.RenameColumn(
                name: "INCIDENTRESULT",
                table: "claim_incident",
                newName: "incident_result");

            migrationBuilder.RenameColumn(
                name: "INCIDENTPROVINCEID",
                table: "claim_incident",
                newName: "incident_province_id");

            migrationBuilder.RenameColumn(
                name: "INCIDENTLONG",
                table: "claim_incident",
                newName: "incident_long");

            migrationBuilder.RenameColumn(
                name: "INCIDENTLEVELID",
                table: "claim_incident",
                newName: "incident_level_id");

            migrationBuilder.RenameColumn(
                name: "INCIDENTLAT",
                table: "claim_incident",
                newName: "incident_lat");

            migrationBuilder.RenameColumn(
                name: "INCIDENTID",
                table: "claim_incident",
                newName: "incident_id");

            migrationBuilder.RenameColumn(
                name: "INCIDENTFULLADDRESS",
                table: "claim_incident",
                newName: "incident_full_address");

            migrationBuilder.RenameColumn(
                name: "INCIDENTDESCRIPTION",
                table: "claim_incident",
                newName: "incident_description");

            migrationBuilder.RenameColumn(
                name: "INCIDENTDATE",
                table: "claim_incident",
                newName: "incident_date");

            migrationBuilder.RenameColumn(
                name: "INCIDENTCAUSEID",
                table: "claim_incident",
                newName: "incident_cause_id");

            migrationBuilder.RenameColumn(
                name: "INCIDENTADDRESS",
                table: "claim_incident",
                newName: "incident_address");

            migrationBuilder.RenameColumn(
                name: "EXTRA_PROPERTIES",
                table: "claim_incident",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "CREATORID",
                table: "claim_incident",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CREATIONTIME",
                table: "claim_incident",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "ASSESSMENTPARTNERID",
                table: "claim_incident",
                newName: "assessment_partner_id");

            migrationBuilder.RenameColumn(
                name: "ASSESSMENTDATE",
                table: "claim_incident",
                newName: "assessment_date");

            migrationBuilder.RenameIndex(
                name: "IX_CLAIMINCIDENT_INCIDENTWARDID",
                table: "claim_incident",
                newName: "IX_claim_incident_incident_ward_id");

            migrationBuilder.RenameIndex(
                name: "IX_CLAIMINCIDENT_INCIDENTPROVINCEID",
                table: "claim_incident",
                newName: "IX_claim_incident_incident_province_id");

            migrationBuilder.RenameIndex(
                name: "IX_CLAIMINCIDENT_INCIDENTLEVELID",
                table: "claim_incident",
                newName: "IX_claim_incident_incident_level_id");

            migrationBuilder.RenameIndex(
                name: "IX_CLAIMINCIDENT_ASSESSMENTPARTNERID",
                table: "claim_incident",
                newName: "IX_claim_incident_assessment_partner_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "creator_id",
                table: "res_incident_level",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "creator_id",
                table: "res_incident_cause",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "creator_id",
                table: "claim_incident_risk_motor",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_claim",
                table: "claim",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_res_incident_level",
                table: "res_incident_level",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_res_incident_cause",
                table: "res_incident_cause",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_claim_incident_risk_motor",
                table: "claim_incident_risk_motor",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_claim_incident",
                table: "claim_incident",
                column: "id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_claim",
                table: "claim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_res_incident_level",
                table: "res_incident_level");

            migrationBuilder.DropPrimaryKey(
                name: "PK_res_incident_cause",
                table: "res_incident_cause");

            migrationBuilder.DropPrimaryKey(
                name: "PK_claim_incident_risk_motor",
                table: "claim_incident_risk_motor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_claim_incident",
                table: "claim_incident");

            migrationBuilder.RenameTable(
                name: "claim",
                newName: "CLAIM");

            migrationBuilder.RenameTable(
                name: "res_incident_level",
                newName: "RESINCIDENTLEVEL");

            migrationBuilder.RenameTable(
                name: "res_incident_cause",
                newName: "RESINCIDENTCAUSE");

            migrationBuilder.RenameTable(
                name: "claim_incident_risk_motor",
                newName: "CLAIMINCIDENTRISKMOTOR");

            migrationBuilder.RenameTable(
                name: "claim_incident",
                newName: "CLAIMINCIDENT");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "CLAIM",
                newName: "STATUS");

            migrationBuilder.RenameColumn(
                name: "priority",
                table: "CLAIM",
                newName: "PRIORITY");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "CLAIM",
                newName: "IS_DELETED");

            migrationBuilder.RenameColumn(
                name: "deletion_time",
                table: "CLAIM",
                newName: "DELETION_TIME");

            migrationBuilder.RenameColumn(
                name: "deleter_id",
                table: "CLAIM",
                newName: "DELETER_ID");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "CLAIM",
                newName: "CONCURRENCY_STAMP");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "CLAIM",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CLAIM",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "snapshot_link",
                table: "CLAIM",
                newName: "SNAPSHOTLINK");

            migrationBuilder.RenameColumn(
                name: "process_claim_type",
                table: "CLAIM",
                newName: "PROCESSCLAIMTYPE");

            migrationBuilder.RenameColumn(
                name: "open_employee_id",
                table: "CLAIM",
                newName: "OPENEMPLOYEEID");

            migrationBuilder.RenameColumn(
                name: "open_date",
                table: "CLAIM",
                newName: "OPENDATE");

            migrationBuilder.RenameColumn(
                name: "notify_date",
                table: "CLAIM",
                newName: "NOTIFYDATE");

            migrationBuilder.RenameColumn(
                name: "notifier_phone",
                table: "CLAIM",
                newName: "NOTIFIERPHONE");

            migrationBuilder.RenameColumn(
                name: "notifier_name",
                table: "CLAIM",
                newName: "NOTIFIERNAME");

            migrationBuilder.RenameColumn(
                name: "notifier_in_relationship",
                table: "CLAIM",
                newName: "NOTIFIERINRELATIONSHIP");

            migrationBuilder.RenameColumn(
                name: "notifier_email",
                table: "CLAIM",
                newName: "NOTIFIEREMAIL");

            migrationBuilder.RenameColumn(
                name: "lob_id",
                table: "CLAIM",
                newName: "LOBID");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "CLAIM",
                newName: "LASTMODIFIERID");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "CLAIM",
                newName: "LASTMODIFICATIONTIME");

            migrationBuilder.RenameColumn(
                name: "insurer_incident_code",
                table: "CLAIM",
                newName: "INSURERINCIDENTCODE");

            migrationBuilder.RenameColumn(
                name: "insurer_id",
                table: "CLAIM",
                newName: "INSURERID");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "CLAIM",
                newName: "CREATORID");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "CLAIM",
                newName: "CREATIONTIME");

            migrationBuilder.RenameColumn(
                name: "contact_phone",
                table: "CLAIM",
                newName: "CONTACTPHONE");

            migrationBuilder.RenameColumn(
                name: "contact_name",
                table: "CLAIM",
                newName: "CONTACTNAME");

            migrationBuilder.RenameColumn(
                name: "contact_in_relationship",
                table: "CLAIM",
                newName: "CONTACTINRELATIONSHIP");

            migrationBuilder.RenameColumn(
                name: "contact_email",
                table: "CLAIM",
                newName: "CONTACTEMAIL");

            migrationBuilder.RenameColumn(
                name: "close_employee_id",
                table: "CLAIM",
                newName: "CLOSEEMPLOYEEID");

            migrationBuilder.RenameColumn(
                name: "close_date",
                table: "CLAIM",
                newName: "CLOSEDATE");

            migrationBuilder.RenameColumn(
                name: "claim_type_id",
                table: "CLAIM",
                newName: "CLAIMTYPEID");

            migrationBuilder.RenameColumn(
                name: "cancel_reason_id",
                table: "CLAIM",
                newName: "CANCELREASONID");

            migrationBuilder.RenameColumn(
                name: "cancel_note",
                table: "CLAIM",
                newName: "CANCELNOTE");

            migrationBuilder.RenameColumn(
                name: "cancel_employee_id",
                table: "CLAIM",
                newName: "CANCELEMPLOYEEID");

            migrationBuilder.RenameColumn(
                name: "cancel_date",
                table: "CLAIM",
                newName: "CANCELDATE");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                table: "CLAIM",
                newName: "EXTRA_PROPERTIES");

            migrationBuilder.RenameIndex(
                name: "IX_claim_close_employee_id",
                table: "CLAIM",
                newName: "IX_CLAIM_CLOSEEMPLOYEEID");

            migrationBuilder.RenameIndex(
                name: "IX_claim_cancel_reason_id",
                table: "CLAIM",
                newName: "IX_CLAIM_CANCELREASONID");

            migrationBuilder.RenameIndex(
                name: "IX_claim_cancel_employee_id",
                table: "CLAIM",
                newName: "IX_CLAIM_CANCELEMPLOYEEID");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "RESINCIDENTLEVEL",
                newName: "STATUS");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "RESINCIDENTLEVEL",
                newName: "NAME");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "RESINCIDENTLEVEL",
                newName: "IS_DELETED");

            migrationBuilder.RenameColumn(
                name: "deletion_time",
                table: "RESINCIDENTLEVEL",
                newName: "DELETION_TIME");

            migrationBuilder.RenameColumn(
                name: "deleter_id",
                table: "RESINCIDENTLEVEL",
                newName: "DELETER_ID");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "RESINCIDENTLEVEL",
                newName: "CONCURRENCY_STAMP");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "RESINCIDENTLEVEL",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "RESINCIDENTLEVEL",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "RESINCIDENTLEVEL",
                newName: "LASTMODIFIERID");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "RESINCIDENTLEVEL",
                newName: "LASTMODIFICATIONTIME");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "RESINCIDENTLEVEL",
                newName: "CREATORID");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "RESINCIDENTLEVEL",
                newName: "CREATIONTIME");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                table: "RESINCIDENTLEVEL",
                newName: "EXTRA_PROPERTIES");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "RESINCIDENTCAUSE",
                newName: "STATUS");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "RESINCIDENTCAUSE",
                newName: "NAME");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "RESINCIDENTCAUSE",
                newName: "IS_DELETED");

            migrationBuilder.RenameColumn(
                name: "deletion_time",
                table: "RESINCIDENTCAUSE",
                newName: "DELETION_TIME");

            migrationBuilder.RenameColumn(
                name: "deleter_id",
                table: "RESINCIDENTCAUSE",
                newName: "DELETER_ID");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "RESINCIDENTCAUSE",
                newName: "CONCURRENCY_STAMP");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "RESINCIDENTCAUSE",
                newName: "CODE");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "RESINCIDENTCAUSE",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "RESINCIDENTCAUSE",
                newName: "LASTMODIFIERID");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "RESINCIDENTCAUSE",
                newName: "LASTMODIFICATIONTIME");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "RESINCIDENTCAUSE",
                newName: "CREATORID");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "RESINCIDENTCAUSE",
                newName: "CREATIONTIME");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                table: "RESINCIDENTCAUSE",
                newName: "EXTRA_PROPERTIES");

            migrationBuilder.RenameColumn(
                name: "vin",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "VIN");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "IS_DELETED");

            migrationBuilder.RenameColumn(
                name: "deletion_time",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "DELETION_TIME");

            migrationBuilder.RenameColumn(
                name: "deleter_id",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "DELETER_ID");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "CONCURRENCY_STAMP");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "persons_on_car",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "PERSONSONCAR");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "LASTMODIFIERID");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "LASTMODIFICATIONTIME");

            migrationBuilder.RenameColumn(
                name: "incident_object_id",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "INCIDENTOBJECTID");

            migrationBuilder.RenameColumn(
                name: "engine_number",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "ENGINENUMBER");

            migrationBuilder.RenameColumn(
                name: "driver_sex",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "DRIVERSEX");

            migrationBuilder.RenameColumn(
                name: "driver_phone",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "DRIVERPHONE");

            migrationBuilder.RenameColumn(
                name: "driver_name",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "DRIVERNAME");

            migrationBuilder.RenameColumn(
                name: "driver_license_no",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "DRIVERLICENSENO");

            migrationBuilder.RenameColumn(
                name: "driver_license_level",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "DRIVERLICENSELEVEL");

            migrationBuilder.RenameColumn(
                name: "driver_license_expire_date",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "DRIVERLICENSEEXPIREDATE");

            migrationBuilder.RenameColumn(
                name: "driver_license_effect_date",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "DRIVERLICENSEEFFECTDATE");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "CREATORID");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "CREATIONTIME");

            migrationBuilder.RenameColumn(
                name: "car_registry_no",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "CARREGISTRYNO");

            migrationBuilder.RenameColumn(
                name: "car_registry_expire_date",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "CARREGISTRYEXPIREDATE");

            migrationBuilder.RenameColumn(
                name: "car_registry_effect_date",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "CARREGISTRYEFFECTDATE");

            migrationBuilder.RenameColumn(
                name: "car_plate",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "CARPLATE");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                table: "CLAIMINCIDENTRISKMOTOR",
                newName: "EXTRA_PROPERTIES");

            migrationBuilder.RenameColumn(
                name: "note",
                table: "CLAIMINCIDENT",
                newName: "NOTE");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "CLAIMINCIDENT",
                newName: "IS_DELETED");

            migrationBuilder.RenameColumn(
                name: "deletion_time",
                table: "CLAIMINCIDENT",
                newName: "DELETION_TIME");

            migrationBuilder.RenameColumn(
                name: "deleter_id",
                table: "CLAIMINCIDENT",
                newName: "DELETER_ID");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "CLAIMINCIDENT",
                newName: "CONCURRENCY_STAMP");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CLAIMINCIDENT",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "on_location",
                table: "CLAIMINCIDENT",
                newName: "ONLOCATION");

            migrationBuilder.RenameColumn(
                name: "object_type_id",
                table: "CLAIMINCIDENT",
                newName: "OBJECTTYPEID");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "CLAIMINCIDENT",
                newName: "LASTMODIFIERID");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "CLAIMINCIDENT",
                newName: "LASTMODIFICATIONTIME");

            migrationBuilder.RenameColumn(
                name: "incident_ward_id",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTWARDID");

            migrationBuilder.RenameColumn(
                name: "incident_result",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTRESULT");

            migrationBuilder.RenameColumn(
                name: "incident_province_id",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTPROVINCEID");

            migrationBuilder.RenameColumn(
                name: "incident_long",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTLONG");

            migrationBuilder.RenameColumn(
                name: "incident_level_id",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTLEVELID");

            migrationBuilder.RenameColumn(
                name: "incident_lat",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTLAT");

            migrationBuilder.RenameColumn(
                name: "incident_id",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTID");

            migrationBuilder.RenameColumn(
                name: "incident_full_address",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTFULLADDRESS");

            migrationBuilder.RenameColumn(
                name: "incident_description",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTDESCRIPTION");

            migrationBuilder.RenameColumn(
                name: "incident_date",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTDATE");

            migrationBuilder.RenameColumn(
                name: "incident_cause_id",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTCAUSEID");

            migrationBuilder.RenameColumn(
                name: "incident_address",
                table: "CLAIMINCIDENT",
                newName: "INCIDENTADDRESS");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "CLAIMINCIDENT",
                newName: "CREATORID");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "CLAIMINCIDENT",
                newName: "CREATIONTIME");

            migrationBuilder.RenameColumn(
                name: "assessment_partner_id",
                table: "CLAIMINCIDENT",
                newName: "ASSESSMENTPARTNERID");

            migrationBuilder.RenameColumn(
                name: "assessment_date",
                table: "CLAIMINCIDENT",
                newName: "ASSESSMENTDATE");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                table: "CLAIMINCIDENT",
                newName: "EXTRA_PROPERTIES");

            migrationBuilder.RenameIndex(
                name: "IX_claim_incident_incident_ward_id",
                table: "CLAIMINCIDENT",
                newName: "IX_CLAIMINCIDENT_INCIDENTWARDID");

            migrationBuilder.RenameIndex(
                name: "IX_claim_incident_incident_province_id",
                table: "CLAIMINCIDENT",
                newName: "IX_CLAIMINCIDENT_INCIDENTPROVINCEID");

            migrationBuilder.RenameIndex(
                name: "IX_claim_incident_incident_level_id",
                table: "CLAIMINCIDENT",
                newName: "IX_CLAIMINCIDENT_INCIDENTLEVELID");

            migrationBuilder.RenameIndex(
                name: "IX_claim_incident_assessment_partner_id",
                table: "CLAIMINCIDENT",
                newName: "IX_CLAIMINCIDENT_ASSESSMENTPARTNERID");

            migrationBuilder.AlterColumn<Guid>(
                name: "CREATORID",
                table: "RESINCIDENTLEVEL",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CREATORID",
                table: "RESINCIDENTCAUSE",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CREATORID",
                table: "CLAIMINCIDENTRISKMOTOR",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CLAIM",
                table: "CLAIM",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RESINCIDENTLEVEL",
                table: "RESINCIDENTLEVEL",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RESINCIDENTCAUSE",
                table: "RESINCIDENTCAUSE",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CLAIMINCIDENTRISKMOTOR",
                table: "CLAIMINCIDENTRISKMOTOR",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CLAIMINCIDENT",
                table: "CLAIMINCIDENT",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIM_REFERENCE_PROLINEO",
                table: "CLAIM",
                column: "LOBID",
                principalTable: "pro_line_of_business",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIM_REFERENCE_RESCLAIM",
                table: "CLAIM",
                column: "CLAIMTYPEID",
                principalTable: "res_claim_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIM_REFERENCE_RESPARTN",
                table: "CLAIM",
                column: "INSURERID",
                principalTable: "res_partner",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIM_hr_employee_CANCELEMPLOYEEID",
                table: "CLAIM",
                column: "CANCELEMPLOYEEID",
                principalTable: "hr_employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIM_hr_employee_CLOSEEMPLOYEEID",
                table: "CLAIM",
                column: "CLOSEEMPLOYEEID",
                principalTable: "hr_employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIM_hr_employee_OPENEMPLOYEEID",
                table: "CLAIM",
                column: "OPENEMPLOYEEID",
                principalTable: "hr_employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIM_res_reason_CANCELREASONID",
                table: "CLAIM",
                column: "CANCELREASONID",
                principalTable: "res_reason",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIMINCIDENT_res_object_type_OBJECTTYPEID",
                table: "CLAIMINCIDENT",
                column: "OBJECTTYPEID",
                principalTable: "res_object_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIMINCIDENT_res_partner_ASSESSMENTPARTNERID",
                table: "CLAIMINCIDENT",
                column: "ASSESSMENTPARTNERID",
                principalTable: "res_partner",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIMINCIDENT_res_province_INCIDENTPROVINCEID",
                table: "CLAIMINCIDENT",
                column: "INCIDENTPROVINCEID",
                principalTable: "res_province",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIMINCIDENT_res_ward_INCIDENTWARDID",
                table: "CLAIMINCIDENT",
                column: "INCIDENTWARDID",
                principalTable: "res_ward",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIMINC_REFERENCE_CLAIM",
                table: "CLAIMINCIDENT",
                column: "INCIDENTID",
                principalTable: "CLAIM",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIMINC_REFERENCE_RESINCID_CAUSE",
                table: "CLAIMINCIDENT",
                column: "INCIDENTCAUSEID",
                principalTable: "RESINCIDENTCAUSE",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIMINC_REFERENCE_RESINCID_LEVEL",
                table: "CLAIMINCIDENT",
                column: "INCIDENTLEVELID",
                principalTable: "RESINCIDENTLEVEL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLAIMINC_REFERENCE_CLAIMINC",
                table: "CLAIMINCIDENTRISKMOTOR",
                column: "INCIDENTOBJECTID",
                principalTable: "CLAIMINCIDENT",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
