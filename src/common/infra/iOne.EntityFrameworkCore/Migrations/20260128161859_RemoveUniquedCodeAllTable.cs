using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniquedCodeAllTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_res_incident_level_code",
                table: "RESINCIDENTLEVEL");

            migrationBuilder.DropIndex(
                name: "ix_res_incident_cause_code",
                table: "RESINCIDENTCAUSE");

            migrationBuilder.DropIndex(
                name: "ix_res_ward_code",
                table: "res_ward");

            migrationBuilder.DropIndex(
                name: "ix_res_tax_code",
                table: "res_tax");

            migrationBuilder.DropIndex(
                name: "ix_res_sequence_code",
                table: "res_sequence");

            migrationBuilder.DropIndex(
                name: "ix_res_risk_code",
                table: "res_risk");

            migrationBuilder.DropIndex(
                name: "ix_res_reason_group_code",
                table: "res_reason_group");

            migrationBuilder.DropIndex(
                name: "ix_res_reason_code",
                table: "res_reason");

            migrationBuilder.DropIndex(
                name: "ix_res_province_code",
                table: "res_province");

            migrationBuilder.DropIndex(
                name: "ix_res_payment_method_code",
                table: "res_payment_method");

            migrationBuilder.DropIndex(
                name: "ix_res_partner_type_code",
                table: "res_partner_type");

            migrationBuilder.DropIndex(
                name: "ix_res_partner_code",
                table: "res_partner");

            migrationBuilder.DropIndex(
                name: "ix_res_organization_type_code",
                table: "res_organization_type");

            migrationBuilder.DropIndex(
                name: "ix_res_industry_code",
                table: "res_industry");

            migrationBuilder.DropIndex(
                name: "ix_res_fee_item_code",
                table: "res_fee_item");

            migrationBuilder.DropIndex(
                name: "ix_res_document_type_code",
                table: "res_document_type");

            migrationBuilder.DropIndex(
                name: "ix_res_currency_code",
                table: "res_currency");

            migrationBuilder.DropIndex(
                name: "ix_res_country_code",
                table: "res_country");

            migrationBuilder.DropIndex(
                name: "ix_res_claim_type_code",
                table: "res_claim_type");

            migrationBuilder.DropIndex(
                name: "ix_res_channel_code",
                table: "res_channel");

            migrationBuilder.DropIndex(
                name: "ix_res_bank_code",
                table: "res_bank");

            migrationBuilder.DropIndex(
                name: "ix_res_app_channel_code",
                table: "res_app_channel");

            migrationBuilder.DropIndex(
                name: "ix_res_agreement_term_code",
                table: "res_agreement_term");

            migrationBuilder.DropIndex(
                name: "ix_pro_table_rate_code",
                table: "pro_table_rate");

            migrationBuilder.DropIndex(
                name: "ix_pro_rule_type_code",
                table: "pro_rule_type");

            migrationBuilder.DropIndex(
                name: "ix_pro_product_type_code",
                table: "pro_product_type");

            migrationBuilder.DropIndex(
                name: "ix_pro_product_category_code",
                table: "pro_product_category");

            migrationBuilder.DropIndex(
                name: "ix_pro_product_code",
                table: "pro_product");

            migrationBuilder.DropIndex(
                name: "ix_pro_line_of_business_code",
                table: "pro_line_of_business");

            migrationBuilder.DropIndex(
                name: "ix_pro_coverage_level_type_code",
                table: "pro_coverage_level_type");

            migrationBuilder.DropIndex(
                name: "ix_pro_coverage_level_basis_code",
                table: "pro_coverage_level_basis");

            migrationBuilder.DropIndex(
                name: "ix_pro_coverage_group_code",
                table: "pro_coverage_group");

            migrationBuilder.DropIndex(
                name: "ix_pro_coverage_code",
                table: "pro_coverage");

            migrationBuilder.DropIndex(
                name: "ix_pro_attribute_code",
                table: "pro_attribute");

            migrationBuilder.DropIndex(
                name: "ix_policy_type_code",
                table: "policy_type");

            migrationBuilder.DropIndex(
                name: "ix_policy_contract_code",
                table: "policy_contract");

            migrationBuilder.DropIndex(
                name: "ix_hr_employee_role_code",
                table: "hr_employee_role");

            migrationBuilder.DropIndex(
                name: "ix_hr_employee_position_code",
                table: "hr_employee_position");

            migrationBuilder.DropIndex(
                name: "ix_hr_employee_level_code",
                table: "hr_employee_level");

            migrationBuilder.DropIndex(
                name: "ix_hr_employee_code",
                table: "hr_employee");

            migrationBuilder.DropIndex(
                name: "ix_hr_department_type_code",
                table: "hr_department_type");

            migrationBuilder.DropIndex(
                name: "ix_hr_department_code",
                table: "hr_department");

            migrationBuilder.DropIndex(
                name: "ix_claim_code",
                table: "CLAIM");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_res_incident_level_code",
                table: "RESINCIDENTLEVEL",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_incident_cause_code",
                table: "RESINCIDENTCAUSE",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_ward_code",
                table: "res_ward",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_tax_code",
                table: "res_tax",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_sequence_code",
                table: "res_sequence",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_risk_code",
                table: "res_risk",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_reason_group_code",
                table: "res_reason_group",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_reason_code",
                table: "res_reason",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_province_code",
                table: "res_province",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_payment_method_code",
                table: "res_payment_method",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_type_code",
                table: "res_partner_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_partner_code",
                table: "res_partner",
                column: "code",
                unique: true,
                filter: "\"code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_res_organization_type_code",
                table: "res_organization_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_industry_code",
                table: "res_industry",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_fee_item_code",
                table: "res_fee_item",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_document_type_code",
                table: "res_document_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_currency_code",
                table: "res_currency",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_country_code",
                table: "res_country",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_claim_type_code",
                table: "res_claim_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_channel_code",
                table: "res_channel",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_bank_code",
                table: "res_bank",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_app_channel_code",
                table: "res_app_channel",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_agreement_term_code",
                table: "res_agreement_term",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_table_rate_code",
                table: "pro_table_rate",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_rule_type_code",
                table: "pro_rule_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_product_type_code",
                table: "pro_product_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_product_category_code",
                table: "pro_product_category",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_product_code",
                table: "pro_product",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_line_of_business_code",
                table: "pro_line_of_business",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_coverage_level_type_code",
                table: "pro_coverage_level_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_coverage_level_basis_code",
                table: "pro_coverage_level_basis",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_coverage_group_code",
                table: "pro_coverage_group",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_coverage_code",
                table: "pro_coverage",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pro_attribute_code",
                table: "pro_attribute",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_policy_type_code",
                table: "policy_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_policy_contract_code",
                table: "policy_contract",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_role_code",
                table: "hr_employee_role",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_position_code",
                table: "hr_employee_position",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_level_code",
                table: "hr_employee_level",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_employee_code",
                table: "hr_employee",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_department_type_code",
                table: "hr_department_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hr_department_code",
                table: "hr_department",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_claim_code",
                table: "CLAIM",
                column: "CODE",
                unique: true);
        }
    }
}
