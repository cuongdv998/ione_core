using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_claim_adjust_at_location_claim_ClaimId1",
                table: "claim_adjust_at_location");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_adjust_at_location_hr_employee_AdjustorId1",
                table: "claim_adjust_at_location");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_adjust_at_location_res_partner_GarageId1",
                table: "claim_adjust_at_location");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_document_claim_ClaimId1",
                table: "claim_document");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_document_claim_adjust_at_location_AdjustAtLocationId1",
                table: "claim_document");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_document_claim_folder_ClaimFolderId1",
                table: "claim_document");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_document_res_document_DocumentId1",
                table: "claim_document");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_claim_ClaimId1",
                table: "claim_folder");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_hr_employee_CloseEmployeeId1",
                table: "claim_folder");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_hr_employee_OpenEmployeeId1",
                table: "claim_folder");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_res_claim_type_ClaimTypeId1",
                table: "claim_folder");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_res_partner_InsurerId1",
                table: "claim_folder");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_exposure_claim_folder_ClaimFolderId1",
                table: "claim_folder_exposure");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_exposure_estimate_claim_folder_ClaimFolderId1",
                table: "claim_folder_exposure_estimate");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_exposure_estimate_res_fee_item_FeeItemId1",
                table: "claim_folder_exposure_estimate");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_incident_object_claim_folder_ClaimFolderId1",
                table: "claim_folder_incident_object");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_incident_object_res_object_type_ObjectTypeId1",
                table: "claim_folder_incident_object");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_claim_folder_ClaimFolderId1",
                table: "claim_folder_item");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_claim_folder_exposure_ClaimFolderExposure~",
                table: "claim_folder_item");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_claim_folder_incident_object_ClaimFolderI~",
                table: "claim_folder_item");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_res_damage_level_DemageLevelId1",
                table: "claim_folder_item");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_res_object_type_item_ItemId1",
                table: "claim_folder_item");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_res_risk_RiskId1",
                table: "claim_folder_item");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_res_uom_UomId1",
                table: "claim_folder_item");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_plan_claim_folder_item_ClaimFolderItemId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_plan_hr_employee_AdjustorId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_plan_res_claim_plan_ClaimPlanId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_item_plan_res_partner_PartnerId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_quotation_claim_folder_ClaimFolderId1",
                table: "claim_folder_quotation");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_folder_quotation_res_partner_PartnerId1",
                table: "claim_folder_quotation");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_sla_res_claim_stage_ClaimStageId1",
                table: "claim_sla");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_sla_res_partner_InsurerId1",
                table: "claim_sla");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_sla_res_task_category_TaskId1",
                table: "claim_sla");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_stage_claim_ClaimId1",
                table: "claim_stage");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_stage_claim_folder_ClaimFolderId1",
                table: "claim_stage");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_stage_res_claim_stage_StageId1",
                table: "claim_stage");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_stage_res_partner_PartnerId1",
                table: "claim_stage");

            migrationBuilder.DropForeignKey(
                name: "FK_res_claim_stage_res_claim_type_ClaimTypeId1",
                table: "res_claim_stage");

            migrationBuilder.DropForeignKey(
                name: "FK_res_claim_stage_task_res_claim_stage_ClaimStageId1",
                table: "res_claim_stage_task");

            migrationBuilder.DropForeignKey(
                name: "FK_res_claim_stage_task_res_task_category_TaskCategoryId1",
                table: "res_claim_stage_task");

            migrationBuilder.DropIndex(
                name: "IX_res_claim_stage_task_ClaimStageId1",
                table: "res_claim_stage_task");

            migrationBuilder.DropIndex(
                name: "IX_res_claim_stage_task_TaskCategoryId1",
                table: "res_claim_stage_task");

            migrationBuilder.DropIndex(
                name: "IX_res_claim_stage_ClaimTypeId1",
                table: "res_claim_stage");

            migrationBuilder.DropIndex(
                name: "IX_claim_stage_ClaimFolderId1",
                table: "claim_stage");

            migrationBuilder.DropIndex(
                name: "IX_claim_stage_ClaimId1",
                table: "claim_stage");

            migrationBuilder.DropIndex(
                name: "IX_claim_stage_PartnerId1",
                table: "claim_stage");

            migrationBuilder.DropIndex(
                name: "IX_claim_stage_StageId1",
                table: "claim_stage");

            migrationBuilder.DropIndex(
                name: "IX_claim_sla_ClaimStageId1",
                table: "claim_sla");

            migrationBuilder.DropIndex(
                name: "IX_claim_sla_InsurerId1",
                table: "claim_sla");

            migrationBuilder.DropIndex(
                name: "IX_claim_sla_TaskId1",
                table: "claim_sla");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_quotation_ClaimFolderId1",
                table: "claim_folder_quotation");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_quotation_PartnerId1",
                table: "claim_folder_quotation");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_plan_AdjustorId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_plan_ClaimFolderItemId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_plan_ClaimPlanId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_plan_PartnerId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_ClaimFolderExposureId1",
                table: "claim_folder_item");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_ClaimFolderId1",
                table: "claim_folder_item");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_ClaimFolderIncidentObjectId1",
                table: "claim_folder_item");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_DemageLevelId1",
                table: "claim_folder_item");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_ItemId1",
                table: "claim_folder_item");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_RiskId1",
                table: "claim_folder_item");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_item_UomId1",
                table: "claim_folder_item");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_incident_object_ClaimFolderId1",
                table: "claim_folder_incident_object");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_incident_object_ObjectTypeId1",
                table: "claim_folder_incident_object");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_exposure_estimate_ClaimFolderId1",
                table: "claim_folder_exposure_estimate");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_exposure_estimate_FeeItemId1",
                table: "claim_folder_exposure_estimate");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_exposure_ClaimFolderId1",
                table: "claim_folder_exposure");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_ClaimId1",
                table: "claim_folder");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_ClaimTypeId1",
                table: "claim_folder");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_CloseEmployeeId1",
                table: "claim_folder");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_InsurerId1",
                table: "claim_folder");

            migrationBuilder.DropIndex(
                name: "IX_claim_folder_OpenEmployeeId1",
                table: "claim_folder");

            migrationBuilder.DropIndex(
                name: "IX_claim_document_AdjustAtLocationId1",
                table: "claim_document");

            migrationBuilder.DropIndex(
                name: "IX_claim_document_ClaimFolderId1",
                table: "claim_document");

            migrationBuilder.DropIndex(
                name: "IX_claim_document_ClaimId1",
                table: "claim_document");

            migrationBuilder.DropIndex(
                name: "IX_claim_document_DocumentId1",
                table: "claim_document");

            migrationBuilder.DropIndex(
                name: "IX_claim_adjust_at_location_AdjustorId1",
                table: "claim_adjust_at_location");

            migrationBuilder.DropIndex(
                name: "IX_claim_adjust_at_location_ClaimId1",
                table: "claim_adjust_at_location");

            migrationBuilder.DropIndex(
                name: "IX_claim_adjust_at_location_GarageId1",
                table: "claim_adjust_at_location");

            migrationBuilder.DropColumn(
                name: "ClaimStageId1",
                table: "res_claim_stage_task");

            migrationBuilder.DropColumn(
                name: "TaskCategoryId1",
                table: "res_claim_stage_task");

            migrationBuilder.DropColumn(
                name: "ClaimTypeId1",
                table: "res_claim_stage");

            migrationBuilder.DropColumn(
                name: "ClaimFolderId1",
                table: "claim_stage");

            migrationBuilder.DropColumn(
                name: "ClaimId1",
                table: "claim_stage");

            migrationBuilder.DropColumn(
                name: "PartnerId1",
                table: "claim_stage");

            migrationBuilder.DropColumn(
                name: "StageId1",
                table: "claim_stage");

            migrationBuilder.DropColumn(
                name: "ClaimStageId1",
                table: "claim_sla");

            migrationBuilder.DropColumn(
                name: "InsurerId1",
                table: "claim_sla");

            migrationBuilder.DropColumn(
                name: "TaskId1",
                table: "claim_sla");

            migrationBuilder.DropColumn(
                name: "ClaimFolderId1",
                table: "claim_folder_quotation");

            migrationBuilder.DropColumn(
                name: "PartnerId1",
                table: "claim_folder_quotation");

            migrationBuilder.DropColumn(
                name: "AdjustorId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "ClaimFolderItemId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "ClaimPlanId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "PartnerId1",
                table: "claim_folder_item_plan");

            migrationBuilder.DropColumn(
                name: "ClaimFolderExposureId1",
                table: "claim_folder_item");

            migrationBuilder.DropColumn(
                name: "ClaimFolderId1",
                table: "claim_folder_item");

            migrationBuilder.DropColumn(
                name: "ClaimFolderIncidentObjectId1",
                table: "claim_folder_item");

            migrationBuilder.DropColumn(
                name: "DemageLevelId1",
                table: "claim_folder_item");

            migrationBuilder.DropColumn(
                name: "ItemId1",
                table: "claim_folder_item");

            migrationBuilder.DropColumn(
                name: "RiskId1",
                table: "claim_folder_item");

            migrationBuilder.DropColumn(
                name: "UomId1",
                table: "claim_folder_item");

            migrationBuilder.DropColumn(
                name: "ClaimFolderId1",
                table: "claim_folder_incident_object");

            migrationBuilder.DropColumn(
                name: "ObjectTypeId1",
                table: "claim_folder_incident_object");

            migrationBuilder.DropColumn(
                name: "ClaimFolderId1",
                table: "claim_folder_exposure_estimate");

            migrationBuilder.DropColumn(
                name: "FeeItemId1",
                table: "claim_folder_exposure_estimate");

            migrationBuilder.DropColumn(
                name: "ClaimFolderId1",
                table: "claim_folder_exposure");

            migrationBuilder.DropColumn(
                name: "ClaimId1",
                table: "claim_folder");

            migrationBuilder.DropColumn(
                name: "ClaimTypeId1",
                table: "claim_folder");

            migrationBuilder.DropColumn(
                name: "CloseEmployeeId1",
                table: "claim_folder");

            migrationBuilder.DropColumn(
                name: "InsurerId1",
                table: "claim_folder");

            migrationBuilder.DropColumn(
                name: "OpenEmployeeId1",
                table: "claim_folder");

            migrationBuilder.DropColumn(
                name: "AdjustAtLocationId1",
                table: "claim_document");

            migrationBuilder.DropColumn(
                name: "ClaimFolderId1",
                table: "claim_document");

            migrationBuilder.DropColumn(
                name: "ClaimId1",
                table: "claim_document");

            migrationBuilder.DropColumn(
                name: "DocumentId1",
                table: "claim_document");

            migrationBuilder.DropColumn(
                name: "AdjustorId1",
                table: "claim_adjust_at_location");

            migrationBuilder.DropColumn(
                name: "ClaimId1",
                table: "claim_adjust_at_location");

            migrationBuilder.DropColumn(
                name: "GarageId1",
                table: "claim_adjust_at_location");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClaimStageId1",
                table: "res_claim_stage_task",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaskCategoryId1",
                table: "res_claim_stage_task",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimTypeId1",
                table: "res_claim_stage",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimFolderId1",
                table: "claim_stage",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimId1",
                table: "claim_stage",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerId1",
                table: "claim_stage",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StageId1",
                table: "claim_stage",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimStageId1",
                table: "claim_sla",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InsurerId1",
                table: "claim_sla",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaskId1",
                table: "claim_sla",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimFolderId1",
                table: "claim_folder_quotation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerId1",
                table: "claim_folder_quotation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AdjustorId1",
                table: "claim_folder_item_plan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimFolderItemId1",
                table: "claim_folder_item_plan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimPlanId1",
                table: "claim_folder_item_plan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerId1",
                table: "claim_folder_item_plan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimFolderExposureId1",
                table: "claim_folder_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimFolderId1",
                table: "claim_folder_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimFolderIncidentObjectId1",
                table: "claim_folder_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DemageLevelId1",
                table: "claim_folder_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ItemId1",
                table: "claim_folder_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RiskId1",
                table: "claim_folder_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UomId1",
                table: "claim_folder_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimFolderId1",
                table: "claim_folder_incident_object",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ObjectTypeId1",
                table: "claim_folder_incident_object",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimFolderId1",
                table: "claim_folder_exposure_estimate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FeeItemId1",
                table: "claim_folder_exposure_estimate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimFolderId1",
                table: "claim_folder_exposure",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimId1",
                table: "claim_folder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimTypeId1",
                table: "claim_folder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CloseEmployeeId1",
                table: "claim_folder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InsurerId1",
                table: "claim_folder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OpenEmployeeId1",
                table: "claim_folder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AdjustAtLocationId1",
                table: "claim_document",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimFolderId1",
                table: "claim_document",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimId1",
                table: "claim_document",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentId1",
                table: "claim_document",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AdjustorId1",
                table: "claim_adjust_at_location",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimId1",
                table: "claim_adjust_at_location",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GarageId1",
                table: "claim_adjust_at_location",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_res_claim_stage_task_ClaimStageId1",
                table: "res_claim_stage_task",
                column: "ClaimStageId1");

            migrationBuilder.CreateIndex(
                name: "IX_res_claim_stage_task_TaskCategoryId1",
                table: "res_claim_stage_task",
                column: "TaskCategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_res_claim_stage_ClaimTypeId1",
                table: "res_claim_stage",
                column: "ClaimTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_ClaimFolderId1",
                table: "claim_stage",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_ClaimId1",
                table: "claim_stage",
                column: "ClaimId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_PartnerId1",
                table: "claim_stage",
                column: "PartnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_StageId1",
                table: "claim_stage",
                column: "StageId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_sla_ClaimStageId1",
                table: "claim_sla",
                column: "ClaimStageId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_sla_InsurerId1",
                table: "claim_sla",
                column: "InsurerId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_sla_TaskId1",
                table: "claim_sla",
                column: "TaskId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_quotation_ClaimFolderId1",
                table: "claim_folder_quotation",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_quotation_PartnerId1",
                table: "claim_folder_quotation",
                column: "PartnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_AdjustorId1",
                table: "claim_folder_item_plan",
                column: "AdjustorId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_ClaimFolderItemId1",
                table: "claim_folder_item_plan",
                column: "ClaimFolderItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_ClaimPlanId1",
                table: "claim_folder_item_plan",
                column: "ClaimPlanId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_PartnerId1",
                table: "claim_folder_item_plan",
                column: "PartnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_ClaimFolderExposureId1",
                table: "claim_folder_item",
                column: "ClaimFolderExposureId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_ClaimFolderId1",
                table: "claim_folder_item",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_ClaimFolderIncidentObjectId1",
                table: "claim_folder_item",
                column: "ClaimFolderIncidentObjectId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_DemageLevelId1",
                table: "claim_folder_item",
                column: "DemageLevelId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_ItemId1",
                table: "claim_folder_item",
                column: "ItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_RiskId1",
                table: "claim_folder_item",
                column: "RiskId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_UomId1",
                table: "claim_folder_item",
                column: "UomId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_incident_object_ClaimFolderId1",
                table: "claim_folder_incident_object",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_incident_object_ObjectTypeId1",
                table: "claim_folder_incident_object",
                column: "ObjectTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_exposure_estimate_ClaimFolderId1",
                table: "claim_folder_exposure_estimate",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_exposure_estimate_FeeItemId1",
                table: "claim_folder_exposure_estimate",
                column: "FeeItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_exposure_ClaimFolderId1",
                table: "claim_folder_exposure",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_ClaimId1",
                table: "claim_folder",
                column: "ClaimId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_ClaimTypeId1",
                table: "claim_folder",
                column: "ClaimTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_CloseEmployeeId1",
                table: "claim_folder",
                column: "CloseEmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_InsurerId1",
                table: "claim_folder",
                column: "InsurerId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_OpenEmployeeId1",
                table: "claim_folder",
                column: "OpenEmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_AdjustAtLocationId1",
                table: "claim_document",
                column: "AdjustAtLocationId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_ClaimFolderId1",
                table: "claim_document",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_ClaimId1",
                table: "claim_document",
                column: "ClaimId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_DocumentId1",
                table: "claim_document",
                column: "DocumentId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_adjust_at_location_AdjustorId1",
                table: "claim_adjust_at_location",
                column: "AdjustorId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_adjust_at_location_ClaimId1",
                table: "claim_adjust_at_location",
                column: "ClaimId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_adjust_at_location_GarageId1",
                table: "claim_adjust_at_location",
                column: "GarageId1");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_adjust_at_location_claim_ClaimId1",
                table: "claim_adjust_at_location",
                column: "ClaimId1",
                principalTable: "claim",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_adjust_at_location_hr_employee_AdjustorId1",
                table: "claim_adjust_at_location",
                column: "AdjustorId1",
                principalTable: "hr_employee",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_adjust_at_location_res_partner_GarageId1",
                table: "claim_adjust_at_location",
                column: "GarageId1",
                principalTable: "res_partner",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_document_claim_ClaimId1",
                table: "claim_document",
                column: "ClaimId1",
                principalTable: "claim",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_document_claim_adjust_at_location_AdjustAtLocationId1",
                table: "claim_document",
                column: "AdjustAtLocationId1",
                principalTable: "claim_adjust_at_location",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_document_claim_folder_ClaimFolderId1",
                table: "claim_document",
                column: "ClaimFolderId1",
                principalTable: "claim_folder",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_document_res_document_DocumentId1",
                table: "claim_document",
                column: "DocumentId1",
                principalTable: "res_document",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_claim_ClaimId1",
                table: "claim_folder",
                column: "ClaimId1",
                principalTable: "claim",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_hr_employee_CloseEmployeeId1",
                table: "claim_folder",
                column: "CloseEmployeeId1",
                principalTable: "hr_employee",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_hr_employee_OpenEmployeeId1",
                table: "claim_folder",
                column: "OpenEmployeeId1",
                principalTable: "hr_employee",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_res_claim_type_ClaimTypeId1",
                table: "claim_folder",
                column: "ClaimTypeId1",
                principalTable: "res_claim_type",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_res_partner_InsurerId1",
                table: "claim_folder",
                column: "InsurerId1",
                principalTable: "res_partner",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_exposure_claim_folder_ClaimFolderId1",
                table: "claim_folder_exposure",
                column: "ClaimFolderId1",
                principalTable: "claim_folder",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_exposure_estimate_claim_folder_ClaimFolderId1",
                table: "claim_folder_exposure_estimate",
                column: "ClaimFolderId1",
                principalTable: "claim_folder",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_exposure_estimate_res_fee_item_FeeItemId1",
                table: "claim_folder_exposure_estimate",
                column: "FeeItemId1",
                principalTable: "res_fee_item",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_incident_object_claim_folder_ClaimFolderId1",
                table: "claim_folder_incident_object",
                column: "ClaimFolderId1",
                principalTable: "claim_folder",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_incident_object_res_object_type_ObjectTypeId1",
                table: "claim_folder_incident_object",
                column: "ObjectTypeId1",
                principalTable: "res_object_type",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_claim_folder_ClaimFolderId1",
                table: "claim_folder_item",
                column: "ClaimFolderId1",
                principalTable: "claim_folder",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_claim_folder_exposure_ClaimFolderExposure~",
                table: "claim_folder_item",
                column: "ClaimFolderExposureId1",
                principalTable: "claim_folder_exposure",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_claim_folder_incident_object_ClaimFolderI~",
                table: "claim_folder_item",
                column: "ClaimFolderIncidentObjectId1",
                principalTable: "claim_folder_incident_object",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_res_damage_level_DemageLevelId1",
                table: "claim_folder_item",
                column: "DemageLevelId1",
                principalTable: "res_damage_level",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_res_object_type_item_ItemId1",
                table: "claim_folder_item",
                column: "ItemId1",
                principalTable: "res_object_type_item",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_res_risk_RiskId1",
                table: "claim_folder_item",
                column: "RiskId1",
                principalTable: "res_risk",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_res_uom_UomId1",
                table: "claim_folder_item",
                column: "UomId1",
                principalTable: "res_uom",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_plan_claim_folder_item_ClaimFolderItemId1",
                table: "claim_folder_item_plan",
                column: "ClaimFolderItemId1",
                principalTable: "claim_folder_item",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_plan_hr_employee_AdjustorId1",
                table: "claim_folder_item_plan",
                column: "AdjustorId1",
                principalTable: "hr_employee",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_plan_res_claim_plan_ClaimPlanId1",
                table: "claim_folder_item_plan",
                column: "ClaimPlanId1",
                principalTable: "res_claim_plan",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_item_plan_res_partner_PartnerId1",
                table: "claim_folder_item_plan",
                column: "PartnerId1",
                principalTable: "res_partner",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_quotation_claim_folder_ClaimFolderId1",
                table: "claim_folder_quotation",
                column: "ClaimFolderId1",
                principalTable: "claim_folder",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_folder_quotation_res_partner_PartnerId1",
                table: "claim_folder_quotation",
                column: "PartnerId1",
                principalTable: "res_partner",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_sla_res_claim_stage_ClaimStageId1",
                table: "claim_sla",
                column: "ClaimStageId1",
                principalTable: "res_claim_stage",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_sla_res_partner_InsurerId1",
                table: "claim_sla",
                column: "InsurerId1",
                principalTable: "res_partner",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_sla_res_task_category_TaskId1",
                table: "claim_sla",
                column: "TaskId1",
                principalTable: "res_task_category",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_stage_claim_ClaimId1",
                table: "claim_stage",
                column: "ClaimId1",
                principalTable: "claim",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_stage_claim_folder_ClaimFolderId1",
                table: "claim_stage",
                column: "ClaimFolderId1",
                principalTable: "claim_folder",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_stage_res_claim_stage_StageId1",
                table: "claim_stage",
                column: "StageId1",
                principalTable: "res_claim_stage",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_stage_res_partner_PartnerId1",
                table: "claim_stage",
                column: "PartnerId1",
                principalTable: "res_partner",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_res_claim_stage_res_claim_type_ClaimTypeId1",
                table: "res_claim_stage",
                column: "ClaimTypeId1",
                principalTable: "res_claim_type",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_res_claim_stage_task_res_claim_stage_ClaimStageId1",
                table: "res_claim_stage_task",
                column: "ClaimStageId1",
                principalTable: "res_claim_stage",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_res_claim_stage_task_res_task_category_TaskCategoryId1",
                table: "res_claim_stage_task",
                column: "TaskCategoryId1",
                principalTable: "res_task_category",
                principalColumn: "id");
        }
    }
}
