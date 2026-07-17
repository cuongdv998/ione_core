using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class ProProductDistribution_Refactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROPRODUCTDISTRIBUTION_hr_employee_role_EMPLOYEEROLEID",
                table: "PROPRODUCTDISTRIBUTION");

            migrationBuilder.DropForeignKey(
                name: "FK_PROPRODUCTDISTRIBUTION_pro_product_PRODUCTID",
                table: "PROPRODUCTDISTRIBUTION");

            migrationBuilder.DropForeignKey(
                name: "FK_PROPRODUCTDISTRIBUTION_res_app_channel_APPCHANNELID",
                table: "PROPRODUCTDISTRIBUTION");

            migrationBuilder.DropForeignKey(
                name: "FK_PROPRODUCTDISTRIBUTION_res_channel_CHANNELID",
                table: "PROPRODUCTDISTRIBUTION");

            migrationBuilder.DropForeignKey(
                name: "FK_PROPRODUCTPLANDEFINITION_pro_product_PRODUCTID",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PROPRODUCTPLANDEFINITION",
                table: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PROPRODUCTDISTRIBUTION",
                table: "PROPRODUCTDISTRIBUTION");

            migrationBuilder.RenameTable(
                name: "PROPRODUCTPLANDEFINITION",
                newName: "pro_product_plan_definition");

            migrationBuilder.RenameTable(
                name: "PROPRODUCTDISTRIBUTION",
                newName: "pro_product_distribution");

            migrationBuilder.RenameColumn(
                name: "STATUS",
                table: "pro_product_plan_definition",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "pro_product_plan_definition",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PRODUCTID",
                table: "pro_product_plan_definition",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "PLANNAME",
                table: "pro_product_plan_definition",
                newName: "plan_name");

            migrationBuilder.RenameColumn(
                name: "PLANCODE",
                table: "pro_product_plan_definition",
                newName: "plan_code");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFIERID",
                table: "pro_product_plan_definition",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFICATIONTIME",
                table: "pro_product_plan_definition",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "CREATORID",
                table: "pro_product_plan_definition",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CREATIONTIME",
                table: "pro_product_plan_definition",
                newName: "creation_time");

            migrationBuilder.RenameIndex(
                name: "IX_PROPRODUCTPLANDEFINITION_PRODUCTID",
                table: "pro_product_plan_definition",
                newName: "ix_pro_product_plan_definition_product_id");

            migrationBuilder.RenameIndex(
                name: "ix_proproductplandefinition_plancode",
                table: "pro_product_plan_definition",
                newName: "ix_pro_product_plan_definition_plan_code");

            migrationBuilder.RenameColumn(
                name: "STATUS",
                table: "pro_product_distribution",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "pro_product_distribution",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PRODUCTID",
                table: "pro_product_distribution",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFIERID",
                table: "pro_product_distribution",
                newName: "last_modifier_id");

            migrationBuilder.RenameColumn(
                name: "LASTMODIFICATIONTIME",
                table: "pro_product_distribution",
                newName: "last_modification_time");

            migrationBuilder.RenameColumn(
                name: "EMPLOYEEROLEID",
                table: "pro_product_distribution",
                newName: "employee_role_id");

            migrationBuilder.RenameColumn(
                name: "CREATORID",
                table: "pro_product_distribution",
                newName: "creator_id");

            migrationBuilder.RenameColumn(
                name: "CREATIONTIME",
                table: "pro_product_distribution",
                newName: "creation_time");

            migrationBuilder.RenameColumn(
                name: "CHANNELID",
                table: "pro_product_distribution",
                newName: "channel_id");

            migrationBuilder.RenameColumn(
                name: "APPCHANNELID",
                table: "pro_product_distribution",
                newName: "app_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_PROPRODUCTDISTRIBUTION_PRODUCTID",
                table: "pro_product_distribution",
                newName: "ix_pro_product_distribution_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_PROPRODUCTDISTRIBUTION_EMPLOYEEROLEID",
                table: "pro_product_distribution",
                newName: "IX_pro_product_distribution_employee_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_PROPRODUCTDISTRIBUTION_CHANNELID",
                table: "pro_product_distribution",
                newName: "IX_pro_product_distribution_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_PROPRODUCTDISTRIBUTION_APPCHANNELID",
                table: "pro_product_distribution",
                newName: "IX_pro_product_distribution_app_channel_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_pro_product_plan_definition",
                table: "pro_product_plan_definition",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_pro_product_distribution",
                table: "pro_product_distribution",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_pro_product_distribution_hr_employee_role_employee_role_id",
                table: "pro_product_distribution",
                column: "employee_role_id",
                principalTable: "hr_employee_role",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_pro_product_distribution_pro_product_product_id",
                table: "pro_product_distribution",
                column: "product_id",
                principalTable: "pro_product",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_pro_product_distribution_res_app_channel_app_channel_id",
                table: "pro_product_distribution",
                column: "app_channel_id",
                principalTable: "res_app_channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_pro_product_distribution_res_channel_channel_id",
                table: "pro_product_distribution",
                column: "channel_id",
                principalTable: "res_channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_pro_product_plan_definition_pro_product_product_id",
                table: "pro_product_plan_definition",
                column: "product_id",
                principalTable: "pro_product",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pro_product_distribution_hr_employee_role_employee_role_id",
                table: "pro_product_distribution");

            migrationBuilder.DropForeignKey(
                name: "fk_pro_product_distribution_pro_product_product_id",
                table: "pro_product_distribution");

            migrationBuilder.DropForeignKey(
                name: "fk_pro_product_distribution_res_app_channel_app_channel_id",
                table: "pro_product_distribution");

            migrationBuilder.DropForeignKey(
                name: "fk_pro_product_distribution_res_channel_channel_id",
                table: "pro_product_distribution");

            migrationBuilder.DropForeignKey(
                name: "fk_pro_product_plan_definition_pro_product_product_id",
                table: "pro_product_plan_definition");

            migrationBuilder.DropPrimaryKey(
                name: "pk_pro_product_plan_definition",
                table: "pro_product_plan_definition");

            migrationBuilder.DropPrimaryKey(
                name: "pk_pro_product_distribution",
                table: "pro_product_distribution");

            migrationBuilder.RenameTable(
                name: "pro_product_plan_definition",
                newName: "PROPRODUCTPLANDEFINITION");

            migrationBuilder.RenameTable(
                name: "pro_product_distribution",
                newName: "PROPRODUCTDISTRIBUTION");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "STATUS");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "PRODUCTID");

            migrationBuilder.RenameColumn(
                name: "plan_name",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "PLANNAME");

            migrationBuilder.RenameColumn(
                name: "plan_code",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "PLANCODE");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "LASTMODIFIERID");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "LASTMODIFICATIONTIME");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "CREATORID");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "CREATIONTIME");

            migrationBuilder.RenameIndex(
                name: "ix_pro_product_plan_definition_product_id",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "IX_PROPRODUCTPLANDEFINITION_PRODUCTID");

            migrationBuilder.RenameIndex(
                name: "ix_pro_product_plan_definition_plan_code",
                table: "PROPRODUCTPLANDEFINITION",
                newName: "ix_proproductplandefinition_plancode");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "STATUS");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "PRODUCTID");

            migrationBuilder.RenameColumn(
                name: "last_modifier_id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "LASTMODIFIERID");

            migrationBuilder.RenameColumn(
                name: "last_modification_time",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "LASTMODIFICATIONTIME");

            migrationBuilder.RenameColumn(
                name: "employee_role_id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "EMPLOYEEROLEID");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "CREATORID");

            migrationBuilder.RenameColumn(
                name: "creation_time",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "CREATIONTIME");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "CHANNELID");

            migrationBuilder.RenameColumn(
                name: "app_channel_id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "APPCHANNELID");

            migrationBuilder.RenameIndex(
                name: "ix_pro_product_distribution_product_id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "IX_PROPRODUCTDISTRIBUTION_PRODUCTID");

            migrationBuilder.RenameIndex(
                name: "IX_pro_product_distribution_employee_role_id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "IX_PROPRODUCTDISTRIBUTION_EMPLOYEEROLEID");

            migrationBuilder.RenameIndex(
                name: "IX_pro_product_distribution_channel_id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "IX_PROPRODUCTDISTRIBUTION_CHANNELID");

            migrationBuilder.RenameIndex(
                name: "IX_pro_product_distribution_app_channel_id",
                table: "PROPRODUCTDISTRIBUTION",
                newName: "IX_PROPRODUCTDISTRIBUTION_APPCHANNELID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PROPRODUCTPLANDEFINITION",
                table: "PROPRODUCTPLANDEFINITION",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PROPRODUCTDISTRIBUTION",
                table: "PROPRODUCTDISTRIBUTION",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PROPRODUCTDISTRIBUTION_hr_employee_role_EMPLOYEEROLEID",
                table: "PROPRODUCTDISTRIBUTION",
                column: "EMPLOYEEROLEID",
                principalTable: "hr_employee_role",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PROPRODUCTDISTRIBUTION_pro_product_PRODUCTID",
                table: "PROPRODUCTDISTRIBUTION",
                column: "PRODUCTID",
                principalTable: "pro_product",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PROPRODUCTDISTRIBUTION_res_app_channel_APPCHANNELID",
                table: "PROPRODUCTDISTRIBUTION",
                column: "APPCHANNELID",
                principalTable: "res_app_channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PROPRODUCTDISTRIBUTION_res_channel_CHANNELID",
                table: "PROPRODUCTDISTRIBUTION",
                column: "CHANNELID",
                principalTable: "res_channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PROPRODUCTPLANDEFINITION_pro_product_PRODUCTID",
                table: "PROPRODUCTPLANDEFINITION",
                column: "PRODUCTID",
                principalTable: "pro_product",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
