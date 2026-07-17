using System.IO;
using Volo.Abp.VirtualFileSystem;
using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Uow;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Studio;
using iOne.HrDepartmentTypes;
using iOne.HrEmployeeRoles;
using iOne.HrEmployeeLevels;
using iOne.HrEmployeePositions;
using iOne.ResPartnerTypes;
using iOne.ResCountries;
using iOne.ResCarBrands;
using iOne.ResCarLines;
using iOne.ResCarGroups;
using iOne.ResCarModels;
using iOne.ResCarTypes;
using iOne.ResCarCategories;
using iOne.InsurerDictionaries;
using iOne.PolicyTypes;
using iOne.Policies;
using iOne.PolicyContracts;
using iOne.ResAppChannels;
using iOne.ResMotorClasses;
using iOne.ResProvinces;
using iOne.ResWards;
using iOne.ResChannels;
using iOne.ResAgreementTerms;
using iOne.ResDocumentTypes;
using iOne.ResDocuments;
using iOne.AccountPaymentRequests;
using iOne.AdminConfigs;
using iOne.ApiKeys;
using iOne.ResPartners;
using iOne.ResBanks;
using iOne.HrDepartments;
using iOne.ResObjectTypes;
using iOne.ResRisks;
using iOne.ResDamageLevels;
using iOne.HrEmployees;
using iOne.ResIndustries;
using iOne.ResCustomers;
using iOne.ResClaimTypes;
using iOne.Claims;
using iOne.ClaimIncidents;
using iOne.ClaimIncidentRiskMotors;
using iOne.ResIncidentCauses;
using iOne.ResIncidentLevels;
using iOne.ResSequences;
using iOne.ResCurrencies;
using iOne.ResReasonGroups;
using iOne.ResReasons;
using iOne.ResFeeItems;
using iOne.ResPaymentMethods;
using iOne.ResPaymentTypes;
using iOne.ResUomClasses;
using iOne.ResUoms;
using iOne.ResObjectItemTypes;
using iOne.ResObjectTypeItems;
using iOne.ResObjectItemDepreciations;
using iOne.BusinessFlows;
using iOne.WorkInstances;
using iOne.WorkTasks;
using iOne.ResBusinessAssignees;
using iOne.ResBusinessAuthorities;
using iOne.ResEvents;
using iOne.ResTaskCategories;
using iOne.ResUserDevices;
using iOne.SystemEventNotifies;
using iOne.ProLineOfBusinesses;
using iOne.ProCoverageTypes;
using iOne.ProCoverageLevelTypes;
using iOne.ProCoverageLevelBases;
using iOne.ProCoverageGroups;
using iOne.ProProductCategorys;
using iOne.ProProductTypes;
using iOne.ProRuleTypes;
using iOne.ProRules;
using iOne.ProProductPlanDefinitions;
using iOne.ProAttributes;
using iOne.ProTableRates;
using iOne.ProTableRateVariables;
using iOne.ProTableRateLines;
using iOne.ResTaxes;
using iOne.EntityFrameworkCore.HrDepartmentTypes;
using iOne.EntityFrameworkCore.HrEmployeeRoles;
using iOne.EntityFrameworkCore.HrEmployeeLevels;
using iOne.EntityFrameworkCore.HrEmployeePositions;
using iOne.EntityFrameworkCore.ResPartnerTypes;
using iOne.EntityFrameworkCore.ResCountries;
using iOne.EntityFrameworkCore.ResCarBrands;
using iOne.EntityFrameworkCore.ResCarLines;
using iOne.EntityFrameworkCore.ResCarGroups;
using iOne.EntityFrameworkCore.ResCarModels;
using iOne.EntityFrameworkCore.ResCarTypes;
using iOne.EntityFrameworkCore.ResCarCategories;
using iOne.EntityFrameworkCore.InsurerDictionaries;
using iOne.EntityFrameworkCore.PolicyTypes;
using iOne.EntityFrameworkCore.PolicyContracts;
using iOne.EntityFrameworkCore.ResAppChannels;
using iOne.EntityFrameworkCore.ResMotorClasses;
using iOne.EntityFrameworkCore.ResProvinces;
using iOne.EntityFrameworkCore.ResWards;
using iOne.EntityFrameworkCore.ResChannels;
using iOne.EntityFrameworkCore.ResAgreementTerms;
using iOne.EntityFrameworkCore.ResDocumentTypes;
using iOne.EntityFrameworkCore.ResDocuments;
using iOne.EntityFrameworkCore.AccountPaymentRequests;
using iOne.EntityFrameworkCore.AdminConfigs;
using iOne.EntityFrameworkCore.ApiKeys;
using iOne.EntityFrameworkCore.ResPartners;
using iOne.EntityFrameworkCore.ResBanks;
using iOne.EntityFrameworkCore.HrDepartments;
using iOne.EntityFrameworkCore.ResObjectTypes;
using iOne.EntityFrameworkCore.ResRisks;
using iOne.EntityFrameworkCore.ResDamageLevels;
using iOne.EntityFrameworkCore.HrEmployees;
using iOne.EntityFrameworkCore.Policies;
using iOne.EntityFrameworkCore.ResIndustries;
using iOne.EntityFrameworkCore.ResCustomers;
using iOne.EntityFrameworkCore.ResClaimTypes;
using iOne.EntityFrameworkCore.Claims;
using iOne.EntityFrameworkCore.ClaimIncidents;
using iOne.EntityFrameworkCore.ClaimIncidentRiskMotors;
using iOne.EntityFrameworkCore.ResIncidentCauses;
using iOne.EntityFrameworkCore.ResIncidentLevels;
using iOne.EntityFrameworkCore.ResSequences;
using iOne.EntityFrameworkCore.ResCurrencies;
using iOne.EntityFrameworkCore.ResReasonGroups;
using iOne.EntityFrameworkCore.ResReasons;
using iOne.EntityFrameworkCore.ResFeeItems;
using iOne.EntityFrameworkCore.ResPaymentMethods;
using iOne.EntityFrameworkCore.ResPaymentTypes;
using iOne.EntityFrameworkCore.ResUomClasses;
using iOne.EntityFrameworkCore.ResUoms;
using iOne.EntityFrameworkCore.ResObjectItemTypes;
using iOne.EntityFrameworkCore.ResObjectTypeItems;
using iOne.EntityFrameworkCore.ResObjectItemDepreciations;
using iOne.EntityFrameworkCore.BusinessFlows;
using iOne.EntityFrameworkCore.WorkInstances;
using iOne.EntityFrameworkCore.WorkTasks;
using iOne.EntityFrameworkCore.ResBusinessAssignees;
using iOne.EntityFrameworkCore.ResBusinessAuthorities;
using iOne.EntityFrameworkCore.ResEvents;
using iOne.EntityFrameworkCore.ResTaskCategories;
using iOne.EntityFrameworkCore.ResUserDevices;
using iOne.EntityFrameworkCore.SystemEventNotifies;
using iOne.EntityFrameworkCore.ProLineOfBusinesses;
using iOne.EntityFrameworkCore.ProCoverageTypes;
using iOne.EntityFrameworkCore.ProCoverageLevelTypes;
using iOne.EntityFrameworkCore.ProCoverageLevelBasis;
using iOne.EntityFrameworkCore.ProCoverageGroups;
using iOne.EntityFrameworkCore.ProProductCategorys;
using iOne.EntityFrameworkCore.ProProductTypes;
using iOne.EntityFrameworkCore.ProRuleTypes;
using iOne.EntityFrameworkCore.ProRules;
using iOne.EntityFrameworkCore.ProProductPlanDefinitions;
using iOne.EntityFrameworkCore.ProProductDistributions;
using iOne.EntityFrameworkCore.ProAttributes;
using iOne.EntityFrameworkCore.ProCoverages;
using iOne.EntityFrameworkCore.ProProducts;
using iOne.ProCoverages;
using iOne.EntityFrameworkCore.ProTableRates;
using iOne.EntityFrameworkCore.ProTableRateVariables;
using iOne.EntityFrameworkCore.ProTableRateLines;
using iOne.EntityFrameworkCore.ResTaxes;
using iOne.ProProducts;
using iOne.Reports;

namespace iOne.EntityFrameworkCore;

[DependsOn(
    typeof(iOneDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(BlobStoringDatabaseEntityFrameworkCoreModule)
    )]
public class iOneEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        iOneEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<iOneDbContext>(options =>
        {
            /* Remove "includeAllEntities: true" to create
             * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);

            // Add custom repository
            options.AddRepository<HrDepartmentType, EfCoreHrDepartmentTypeRepository>();
            options.AddRepository<HrEmployeeRole, EfCoreHrEmployeeRoleRepository>();
            options.AddRepository<HrEmployeeLevel, EfCoreHrEmployeeLevelRepository>();
            options.AddRepository<HrEmployeePosition, EfCoreHrEmployeePositionRepository>();
            options.AddRepository<ProLineOfBusiness, EfCoreProLineOfBusinessRepository>();
            options.AddRepository<ProCoverageType, EfCoreProCoverageTypeRepository>();
            options.AddRepository<ProCoverageLevelType, EfCoreProCoverageLevelTypeRepository>();
            options.AddRepository<ProCoverageLevelBases.ProCoverageLevelBasis, EfCoreProCoverageLevelBasisRepository>();
            options.AddRepository<ProCoverageGroup, EfCoreProCoverageGroupRepository>();
            options.AddRepository<ProCoverage, EfCoreProCoverageRepository>();
            options.AddRepository<ProProductCategory, EfCoreProProductCategoryRepository>();
            options.AddRepository<ProProductType, EfCoreProProductTypeRepository>();
            options.AddRepository<ProRuleType, EfCoreProRuleTypeRepository>();
            options.AddRepository<iOne.ProRules.ProRule, EfCoreProRuleRepository>();
            options.AddRepository<iOne.ProProductPlanDefinitions.ProProductPlanDefinition, EfCoreProProductPlanDefinitionRepository>();
            options.AddRepository<iOne.ProProductDistributions.ProProductDistribution, EfCoreProProductDistributionRepository>();
            options.AddRepository<ProAttribute, EfCoreProAttributeRepository>();
            options.AddRepository<ProTableRate, EfCoreProTableRateRepository>();
            // Note: ProTableRateVariable is part of ProTableRate aggregate, no separate repository needed
            options.AddRepository<ProTableRateLine, EfCoreProTableRateLineRepository>();
            options.AddRepository<ResTax, EfCoreResTaxRepository>();
            options.AddRepository<ResPartnerType, EfCoreResPartnerTypeRepository>();
            options.AddRepository<ResCountry, EfCoreResCountryRepository>();
            options.AddRepository<ResCarBrand, EfCoreResCarBrandRepository>();
            options.AddRepository<ResCarLine, EfCoreResCarLineRepository>();
            options.AddRepository<ResCarGroup, EfCoreResCarGroupRepository>();
            options.AddRepository<ResCarModel, EfCoreResCarModelRepository>();
            options.AddRepository<ResCarType, EfCoreResCarTypeRepository>();
            options.AddRepository<ResCarCategory, EfCoreResCarCategoryRepository>();
            options.AddRepository<InsurerDictionary, EfCoreInsurerDictionaryRepository>();
            options.AddRepository<PolicyType, EfCorePolicyTypeRepository>();
            options.AddRepository<Policy, EfCorePolicyRepository>();
            options.AddRepository<PolicyVersion, EfCorePolicyVersionRepository>();
            options.AddRepository<PolicyCertificate, EfCorePolicyCertificateRepository>();
            options.AddRepository<PolicyDocument, EfCorePolicyDocumentRepository>();
            options.AddRepository<PolicyAmount, EfCorePolicyAmountRepository>();
            options.AddRepository<PolicyRiskObject, EfCorePolicyRiskObjectRepository>();
            options.AddRepository<PolicyRiskMotor, EfCorePolicyRiskMotorRepository>();
            options.AddRepository<PolicyProduct, EfCorePolicyProductRepository>();
            options.AddRepository<PolicyCoverage, EfCorePolicyCoverageRepository>();
            options.AddRepository<PolicyCoverageLevel, EfCorePolicyCoverageLevelRepository>();
            options.AddRepository<ProProductCoverageLevelTerm, EfCoreProProductCoverageLevelTermRepository>();
            options.AddRepository<PolicyContract, EfCorePolicyContractRepository>();
            options.AddRepository<ResAppChannel, EfCoreResAppChannelRepository>();
            options.AddRepository<ResMotorClass, EfCoreResMotorClassRepository>();
            options.AddRepository<ResProvince, EfCoreResProvinceRepository>();
            options.AddRepository<ResWard, EfCoreResWardRepository>();
            options.AddRepository<ResChannel, EfCoreResChannelRepository>();
            options.AddRepository<ResAgreementTerm, EfCoreResAgreementTermRepository>();
            options.AddRepository<ResDocumentType, EfCoreResDocumentTypeRepository>();
            options.AddRepository<ResDocument, EfCoreResDocumentRepository>();
            options.AddRepository<AccountPaymentRequest, EfCoreAccountPaymentRequestRepository>();
            options.AddRepository<AdminConfig, EfCoreAdminConfigRepository>();
            options.AddRepository<ApiKey, EfCoreApiKeyRepository>();
            options.AddRepository<ResPartner, EfCoreResPartnerRepository>();
            options.AddRepository<ResBank, EfCoreResBankRepository>();
            options.AddRepository<HrDepartment, EfCoreHrDepartmentRepository>();
            options.AddRepository<ResObjectType, EfCoreResObjectTypeRepository>();
            options.AddRepository<ResRisk, EfCoreResRiskRepository>();
            options.AddRepository<ResDamageLevel, EfCoreResDamageLevelRepository>();
            options.AddRepository<HrEmployee, EfCoreHrEmployeeRepository>();
            options.AddRepository<HrEmployeeRoleRel, EfCoreHrEmployeeRoleRelRepository>();
            options.AddRepository<ResIndustry, EfCoreResIndustryRepository>();
            options.AddRepository<ResCustomer, EfCoreResCustomerRepository>();
            options.AddRepository<ResClaimType, EfCoreResClaimTypeRepository>();
            options.AddRepository<Claim, EfCoreClaimRepository>();
            options.AddRepository<ClaimIncident, EfCoreClaimIncidentRepository>();
            options.AddRepository<ClaimIncidentRiskMotor, EfCoreClaimIncidentRiskMotorRepository>();
            options.AddRepository<ResIncidentCause, EfCoreResIncidentCauseRepository>();
            options.AddRepository<ResIncidentLevel, EfCoreResIncidentLevelRepository>();
            options.AddRepository<ResSequence, EfCoreResSequenceRepository>();
            options.AddRepository<ResCurrency, EfCoreResCurrencyRepository>();
            options.AddRepository<ResReasonGroup, EfCoreResReasonGroupRepository>();
            options.AddRepository<ResReason, EfCoreResReasonRepository>();
            options.AddRepository<ResFeeItem, EfCoreResFeeItemRepository>();
            options.AddRepository<ResPaymentMethod, EfCoreResPaymentMethodRepository>();
            options.AddRepository<ResPaymentType, EfCoreResPaymentTypeRepository>();
            options.AddRepository<ResUomClass, EfCoreResUomClassRepository>();
            options.AddRepository<ResUom, EfCoreResUomRepository>();
            options.AddRepository<ResObjectItemType, EfCoreResObjectItemTypeRepository>();
            options.AddRepository<ResObjectTypeItem, EfCoreResObjectTypeItemRepository>();
            options.AddRepository<ResObjectItemDepreciation, ResObjectItemDepreciationRepository>();
            options.AddRepository<ResBusinessAssignee, EfCoreResBusinessAssigneeRepository>();
            options.AddRepository<ResBusinessAuthority, EfCoreResBusinessAuthorityRepository>();
            options.AddRepository<ResTaskCategory, EfCoreResTaskCategoryRepository>();
            options.AddRepository<BusinessFlow, EfCoreBusinessFlowRepository>();
            options.AddRepository<WorkInstance, EfCoreWorkInstanceRepository>();
            options.AddRepository<WorkTask, EfCoreWorkTaskRepository>();
            options.AddRepository<ResEvent, EfCoreResEventRepository>();
            options.AddRepository<ResEventNotifyTemplate, EfCoreResEventNotifyTemplateRepository>();
            options.AddRepository<ResUserDevice, EfCoreResUserDeviceRepository>();
            options.AddRepository<SystemEventNotify, EfCoreSystemEventNotifyRepository>();
            options.AddRepository<ReportTemplate, EfReportTemplateRepository>();
            options.AddRepository<ReportTemplateSql, EfReportTemplateSqlRepository>();
            options.AddRepository<ReportTemplateParameter, EfReportTemplateParameterRepository>();
            options.AddRepository<ReportTemplateSqlParameter, EfReportTemplateSqlParameterRepository>();
        });

        context.Services.AddTransient<IPolicyRequestApprovalQueryRepository, EfCorePolicyRequestApprovalQueryRepository>();

        if (AbpStudioAnalyzeHelper.IsInAnalyzeMode)
        {
            return;
        }

        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also iOneDbContextFactory for EF Core tooling. */

            options.UseNpgsql();

        });

    }
}
