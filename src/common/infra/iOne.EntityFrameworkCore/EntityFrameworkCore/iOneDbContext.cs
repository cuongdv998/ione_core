using iOne.AccountPaymentRequests;
using iOne.AdminConfigs;
using iOne.ApiKeys;
using iOne.ResPartnerMessageLoggings;
using iOne.EntityFrameworkCore.ResPartnerMessageLoggings;
using iOne.BusinessFlows;
using iOne.WorkInstances;
using iOne.WorkTasks;
using iOne.ClaimIncidentRiskMotors;
using iOne.ClaimIncidents;
using iOne.Claims;
using iOne.EntityFrameworkCore.AccountPaymentRequests;
using iOne.EntityFrameworkCore.AdminConfigs;
using iOne.EntityFrameworkCore.BusinessFlows;
using iOne.EntityFrameworkCore.WorkInstances;
using iOne.EntityFrameworkCore.WorkTasks;
using iOne.EntityFrameworkCore.ClaimIncidentRiskMotors;
using iOne.EntityFrameworkCore.ClaimIncidents;
using iOne.EntityFrameworkCore.Claims;
using iOne.EntityFrameworkCore.HrDepartments;
using iOne.EntityFrameworkCore.HrDepartmentTypes;
using iOne.EntityFrameworkCore.HrEmployeeLevels;
using iOne.EntityFrameworkCore.HrEmployeePositions;
using iOne.EntityFrameworkCore.HrEmployeeRoles;
using iOne.EntityFrameworkCore.HrEmployees;
using iOne.EntityFrameworkCore.InsurerDictionaries;
using iOne.EntityFrameworkCore.Policies;
using iOne.EntityFrameworkCore.PolicyContracts;
using iOne.EntityFrameworkCore.PolicyTypes;
using iOne.EntityFrameworkCore.ProAttributes;
using iOne.EntityFrameworkCore.ProCoverageGroups;
using iOne.EntityFrameworkCore.ProCoverageLevelBasis;
using iOne.EntityFrameworkCore.ProCoverageLevelTypes;
using iOne.EntityFrameworkCore.ProCoverages;
using iOne.EntityFrameworkCore.ProCoverageTypes;
using iOne.EntityFrameworkCore.ProLineOfBusinesses;
using iOne.EntityFrameworkCore.ProProductCategorys;
using iOne.EntityFrameworkCore.ProProductDistributions;
using iOne.EntityFrameworkCore.ProProductPlanDefinitions;
using iOne.EntityFrameworkCore.ProProducts;
using iOne.EntityFrameworkCore.ProProductTypes;
using iOne.EntityFrameworkCore.ProRules;
using iOne.EntityFrameworkCore.ProRuleTypes;
using iOne.EntityFrameworkCore.ProTableRateLines;
using iOne.EntityFrameworkCore.ProTableRates;
using iOne.EntityFrameworkCore.ProTableRateVariables;
using iOne.EntityFrameworkCore.ResAgreementTerms;
using iOne.EntityFrameworkCore.ResAppChannels;
using iOne.EntityFrameworkCore.ResBanks;
using iOne.EntityFrameworkCore.ResBusinessAssignees;
using iOne.EntityFrameworkCore.ResBusinessAuthorities;
using iOne.EntityFrameworkCore.ResCarBrands;
using iOne.EntityFrameworkCore.ResCarCategories;
using iOne.EntityFrameworkCore.ResCarGroups;
using iOne.EntityFrameworkCore.ResCarLines;
using iOne.EntityFrameworkCore.ResCarModels;
using iOne.EntityFrameworkCore.ResCarTypes;
using iOne.EntityFrameworkCore.ResChannels;
using iOne.EntityFrameworkCore.ResClaimTypes;
using iOne.EntityFrameworkCore.ResCountries;
using iOne.EntityFrameworkCore.ResCurrencies;
using iOne.EntityFrameworkCore.ResCustomers;
using iOne.EntityFrameworkCore.ResDamageLevels;
using iOne.EntityFrameworkCore.ResDocuments;
using iOne.EntityFrameworkCore.ResDocumentTypes;
using iOne.EntityFrameworkCore.ResEvents;
using iOne.EntityFrameworkCore.ResFeeItems;
using iOne.EntityFrameworkCore.ResIncidentCauses;
using iOne.EntityFrameworkCore.ResIncidentLevels;
using iOne.EntityFrameworkCore.ResIndustries;
using iOne.EntityFrameworkCore.ResMotorClasses;
using iOne.EntityFrameworkCore.ResObjectItemDepreciations;
using iOne.EntityFrameworkCore.ResObjectItemTypes;
using iOne.EntityFrameworkCore.ResObjectTypes;
using iOne.EntityFrameworkCore.ResOrganizationTypes;
using iOne.EntityFrameworkCore.ResPartners;
using iOne.EntityFrameworkCore.ResPartnerTypes;
using iOne.EntityFrameworkCore.ResPaymentMethods;
using iOne.EntityFrameworkCore.ResPaymentTypes;
using iOne.EntityFrameworkCore.ResProvinces;
using iOne.EntityFrameworkCore.ResReasonGroups;
using iOne.EntityFrameworkCore.ResReasons;
using iOne.EntityFrameworkCore.ResClaimPlans;
using iOne.EntityFrameworkCore.ResRisks;
using iOne.EntityFrameworkCore.ResSequences;
using iOne.EntityFrameworkCore.ResTaskCategories;
using iOne.EntityFrameworkCore.ResTaxes;
using iOne.EntityFrameworkCore.ResUomClasses;
using iOne.EntityFrameworkCore.ResUoms;
using iOne.EntityFrameworkCore.ResUserDevices;
using iOne.EntityFrameworkCore.ResWards;
using iOne.EntityFrameworkCore.ApiKeys;
using iOne.EntityFrameworkCore.SystemEventNotifies;
using iOne.HrDepartments;
using iOne.HrDepartmentTypes;
using iOne.HrEmployeeLevels;
using iOne.HrEmployeePositions;
using iOne.HrEmployeeRoles;
using iOne.HrEmployees;
using iOne.InsurerDictionaries;
using iOne.Policies;
using iOne.PolicyContracts;
using iOne.PolicyTypes;
using iOne.ProAttributes;
using iOne.ProCoverageGroups;
using iOne.ProCoverageLevelTypes;
using iOne.ProCoverages;
using iOne.ProLineOfBusinesses;
using iOne.ProProductCategorys;
using iOne.ProProducts;
using iOne.ProProductTypes;
using iOne.ProRules;
using iOne.ProRuleTypes;
using iOne.ProTableRateLines;
using iOne.ProTableRates;
using iOne.ProTableRateVariables;
using iOne.Reports;
using iOne.ResAgreementTerms;
using iOne.ResAppChannels;
using iOne.ResBanks;
using iOne.ResBusinessAuthorities;
using iOne.ResCarBrands;
using iOne.ResCarCategories;
using iOne.ResCarGroups;
using iOne.ResCarLines;
using iOne.ResCarModels;
using iOne.ResCarTypes;
using iOne.ResChannels;
using iOne.ResClaimTypes;
using iOne.ResCountries;
using iOne.ResCurrencies;
using iOne.ResCustomers;
using iOne.ResDamageLevels;
using iOne.ResDocuments;
using iOne.ResDocumentTypes;
using iOne.ResEvents;
using iOne.ResFeeItems;
using iOne.ResIncidentCauses;
using iOne.ResIncidentLevels;
using iOne.ResIndustries;
using iOne.ResMotorClasses;
using iOne.ResObjectItemDepreciations;
using iOne.ResObjectItemTypes;
using iOne.ResObjectTypes;
using iOne.ResOrganizationTypes;
using iOne.ResPartners;
using iOne.ResPartnerTypes;
using iOne.ResPaymentMethods;
using iOne.ResPaymentTypes;
using iOne.ResProvinces;
using iOne.ResReasonGroups;
using iOne.ResReasons;
using iOne.ResRisks;
using iOne.ResSequences;
using iOne.ResTaskCategories;
using iOne.ResTaxes;
using iOne.ResUomClasses;
using iOne.ResUoms;
using iOne.ResUserDevices;
using iOne.ResWards;
using iOne.SystemEventNotifies;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using ProCoverageTypeEntity = iOne.ProCoverageTypes.ProCoverageType;
using ProCoverageLevelBasisEntity = iOne.ProCoverageLevelBases.ProCoverageLevelBasis;
using iOne.ClaimFolders;
using iOne.ClaimFolderItems;
using iOne.ClaimFolderItemPlans;
using iOne.ClaimFolderIncidentObjects;
using iOne.ClaimFolderExposures;
using iOne.ClaimFolderExposureEstimates;
using iOne.ClaimAdjustAtLocations;
using iOne.ClaimDocuments;
using iOne.ClaimFolderQuotations;
using iOne.ClaimFolderQuotationApprovals;
using iOne.ClaimFolderQuotationDetails;
using iOne.ClaimStages;
using iOne.ResClaimStages;
using iOne.ResClaimStageTasks;
using iOne.ClaimSlas;
using iOne.ResClaimPlans;
using iOne.EntityFrameworkCore.ClaimFolders;
using iOne.EntityFrameworkCore.ClaimFolderIncidentObjects;
using iOne.EntityFrameworkCore.ClaimFolderExposures;
using iOne.EntityFrameworkCore.ClaimFolderExposureEstimates;
using iOne.EntityFrameworkCore.ClaimFolderItems;
using iOne.EntityFrameworkCore.ClaimFolderItemPlans;
using iOne.EntityFrameworkCore.ClaimAdjustAtLocations;
using iOne.EntityFrameworkCore.ClaimDocuments;
using iOne.EntityFrameworkCore.ClaimStages;
using iOne.EntityFrameworkCore.ResClaimStages;
using iOne.EntityFrameworkCore.ResClaimStageTasks;
using iOne.EntityFrameworkCore.ClaimSlas;
using iOne.EntityFrameworkCore.ClaimFolderQuotations;
using iOne.EntityFrameworkCore.ClaimFolderQuotationApprovals;
using iOne.EntityFrameworkCore.ClaimFolderQuotationDetails;
using iOne.ClaimFolderEvaluates;
using iOne.ClaimFolderEvaluateDetails;
using iOne.ResClaimEvaluateItems;
using iOne.EntityFrameworkCore.ClaimFolderEvaluates;
using iOne.EntityFrameworkCore.ClaimFolderEvaluateDetails;
using iOne.EntityFrameworkCore.ResClaimEvaluateItems;

namespace iOne.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class iOneDbContext :
    AbpDbContext<iOneDbContext>,
    IIdentityDbContext
{
    public iOneDbContext(DbContextOptions<iOneDbContext> options)
        : base(options)
    {
    }

    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    public DbSet<HrDepartmentType> HrDepartmentTypes { get; set; }
    public DbSet<HrEmployeeRole> HrEmployeeRoles { get; set; }
    public DbSet<HrEmployeeLevel> HrEmployeeLevels { get; set; }
    public DbSet<HrEmployeePosition> HrEmployeePositions { get; set; }
    public DbSet<ProLineOfBusiness> ProLineOfBusinesses { get; set; }
    public DbSet<ProCoverage> ProCoverages { get; set; }
    public DbSet<ProCoverageTypeEntity> ProCoverageTypes { get; set; }
    public DbSet<ProCoverageLevelType> ProCoverageLevelTypes { get; set; }
    public DbSet<ProCoverageLevelBasisEntity> ProCoverageLevelBasis { get; set; }
    public DbSet<ProCoverageGroup> ProCoverageGroups { get; set; }
    public DbSet<ProProductCategory> ProProductCategorys { get; set; }
    public DbSet<ProProductType> ProProductTypes { get; set; }
    public DbSet<ProProduct> ProProducts { get; set; }
    public DbSet<ProRuleType> ProRuleTypes { get; set; }
    public DbSet<ProRule> ProRules { get; set; }
    public DbSet<ProProductTableRate> ProProductTableRates { get; set; }
    public DbSet<ProProductAttribute> ProProductAttributes { get; set; }
    public DbSet<ProProductCoverage> ProProductCoverages { get; set; }
    public DbSet<ProProductCoverageInteraction> ProProductCoverageInteractions { get; set; }
    public DbSet<ProProductCoverageLevel> ProProductCoverageLevels { get; set; }
    public DbSet<ProProductCoverageLevelTerm> ProProductCoverageLevelTerms { get; set; }
    public DbSet<ProAttribute> ProAttributes { get; set; }
    public DbSet<ProTableRate> ProTableRates { get; set; }
    public DbSet<ProTableRateVariable> ProTableRateVariables { get; set; }
    public DbSet<ProTableRateLine> ProTableRateLines { get; set; }
    public DbSet<ResTax> ResTaxes { get; set; }
    public DbSet<ResPartnerType> ResPartnerTypes { get; set; }
    public DbSet<ResOrganizationType> ResOrganizationTypes { get; set; }
    public DbSet<ResCountry> ResCountries { get; set; }
    public DbSet<ResCarBrand> ResCarBrands { get; set; }
    public DbSet<ResCarLine> ResCarLines { get; set; }
    public DbSet<ResCarGroup> ResCarGroups { get; set; }
    public DbSet<ResCarModel> ResCarModels { get; set; }
    public DbSet<ResCarType> ResCarTypes { get; set; }
    public DbSet<ResCarCategory> ResCarCategories { get; set; }
    public DbSet<InsurerDictionary> InsurerDictionaries { get; set; }
    public DbSet<PolicyType> PolicyTypes { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<PolicyVersion> PolicyVersions { get; set; }
    public DbSet<PolicyContract> PolicyContracts { get; set; }
    public DbSet<PolicyDocument> PolicyDocuments { get; set; }
    public DbSet<PolicyContractDocument> PolicyContractDocuments { get; set; }
    public DbSet<PolicyAmount> PolicyAmounts { get; set; }
    public DbSet<PolicyRiskObjectDocument> PolicyRiskObjectDocuments { get; set; }
    public DbSet<ResAppChannel> ResAppChannels { get; set; }
    public DbSet<ResMotorClass> ResMotorClasses { get; set; }
    public DbSet<ResProvince> ResProvinces { get; set; }
    public DbSet<ResWard> ResWards { get; set; }
    public DbSet<ResChannel> ResChannels { get; set; }
    public DbSet<ResAgreementTerm> ResAgreementTerms { get; set; }
    public DbSet<ResDocumentType> ResDocumentTypes { get; set; }
    public DbSet<ResDocument> ResDocuments { get; set; }
    public DbSet<AccountPaymentRequest> AccountPaymentRequests { get; set; }
    public DbSet<AdminConfig> AdminConfigs { get; set; }
    public DbSet<ResPartner> ResPartners { get; set; }
    public DbSet<ResPartnerAgreement> ResPartnerAgreements { get; set; }
    public DbSet<ResBank> ResBanks { get; set; }
    public DbSet<HrDepartment> HrDepartments { get; set; }
    public DbSet<ResObjectType> ResObjectTypes { get; set; }
    public DbSet<ResRisk> ResRisks { get; set; }
    public DbSet<ResDamageLevel> ResDamageLevels { get; set; }
    public DbSet<HrEmployee> HrEmployees { get; set; }
    public DbSet<HrEmployeeRoleRel> HrEmployeeRoleRels { get; set; }
    public DbSet<ResIndustry> ResIndustries { get; set; }
    public DbSet<ResCustomer> ResCustomers { get; set; }
    public DbSet<ResClaimType> ResClaimTypes { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<ClaimIncident> ClaimIncidents { get; set; }
    public DbSet<ClaimIncidentRiskMotor> ClaimIncidentRiskMotors { get; set; }
    public DbSet<ClaimFolder> ClaimFolders { get; set; }
    public DbSet<ClaimFolderItem> ClaimFolderItems { get; set; }
    public DbSet<ClaimFolderItemPlan> ClaimFolderItemPlans { get; set; }
    public DbSet<ClaimFolderIncidentObject> ClaimFolderIncidentObjects { get; set; }
    public DbSet<ClaimFolderExposure> ClaimFolderExposures { get; set; }
    public DbSet<ClaimFolderExposureEstimate> ClaimFolderExposureEstimates { get; set; }
    public DbSet<ClaimAdjustAtLocation> ClaimAdjustAtLocations { get; set; }
    public DbSet<ClaimDocument> ClaimDocuments { get; set; }
    public DbSet<ClaimFolderQuotation> ClaimFolderQuotations { get; set; }
    public DbSet<ClaimFolderQuotationApproval> ClaimFolderQuotationApprovals { get; set; }
    public DbSet<ClaimFolderQuotationDetail> ClaimFolderQuotationDetails { get; set; }
    public DbSet<ClaimFolderEvaluate> ClaimFolderEvaluates { get; set; }
    public DbSet<ClaimFolderEvaluateDetail> ClaimFolderEvaluateDetails { get; set; }
    public DbSet<ResClaimEvaluateItem> ResClaimEvaluateItems { get; set; }
    public DbSet<ClaimStage> ClaimStages { get; set; }
    public DbSet<ResClaimStage> ResClaimStages { get; set; }
    public DbSet<ResClaimStageTask> ResClaimStageTasks { get; set; }
    public DbSet<ClaimSla> ClaimSlas { get; set; }
    public DbSet<ResIncidentCause> ResIncidentCauses { get; set; }
    public DbSet<ResIncidentLevel> ResIncidentLevels { get; set; }
    public DbSet<ResSequence> ResSequences { get; set; }
    public DbSet<ResCurrency> ResCurrencies { get; set; }
    public DbSet<ResReasonGroup> ResReasonGroups { get; set; }
    public DbSet<ResReason> ResReasons { get; set; }
    public DbSet<ResFeeItem> ResFeeItems { get; set; }
    public DbSet<ResClaimPlan> ResClaimPlans { get; set; }
    public DbSet<ResPaymentMethod> ResPaymentMethods { get; set; }
    public DbSet<ResPaymentType> ResPaymentTypes { get; set; }
    public DbSet<ResUomClass> ResUomClasses { get; set; }
    public DbSet<ResUom> ResUoms { get; set; }
    public DbSet<ResObjectItemType> ResObjectItemTypes { get; set; }
    public DbSet<ResObjectItemDepreciation> ResObjectItemDepreciations { get; set; }
    public DbSet<BusinessFlow> BusinessFlows { get; set; }
    public DbSet<ResBusinessAuthority> ResBusinessAuthorities { get; set; }
    public DbSet<ResTaskCategory> ResTaskCategories { get; set; }
    public DbSet<WorkInstance> WorkInstances { get; set; }
    public DbSet<WorkTask> WorkTasks { get; set; }
    public DbSet<ResEvent> ResEvents { get; set; }
    public DbSet<ResEventNotifyTemplate> ResEventNotifyTemplates { get; set; }
    public DbSet<ResUserDevice> ResUserDevices { get; set; }
    public DbSet<SystemEventNotify> SystemEventNotifies { get; set; }
    public DbSet<ReportTemplate> ReportTemplates { get; set; }
    public DbSet<ReportTemplateSql> ReportTemplateSqls { get; set; }
    public DbSet<ReportTemplateParameter> ReportTemplateParameters { get; set; }
    public DbSet<ReportTemplateSqlParameter> ReportTemplateSqlParameters { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<ResPartnerMessageLogging> ResPartnerMessageLoggings { get; set; }
    public DbSet<PartnerConsent> PartnerConsents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */

        builder.ApplyConfiguration(new HrDepartmentTypeConfiguration());
        builder.ApplyConfiguration(new HrEmployeeRoleConfiguration());
        builder.ApplyConfiguration(new HrEmployeeLevelConfiguration());
        builder.ApplyConfiguration(new HrEmployeePositionConfiguration());
        builder.ApplyConfiguration(new ProLineOfBusinessConfiguration());
        builder.ApplyConfiguration(new ProCoverageTypeConfiguration());
        builder.ApplyConfiguration(new ProCoverageLevelTypeConfiguration());
        builder.ApplyConfiguration(new ProCoverageLevelBasisConfiguration());
        builder.ApplyConfiguration(new ProCoverageGroupConfiguration());
        builder.ApplyConfiguration(new ProCoverageConfiguration());
        builder.ApplyConfiguration(new ProProductCategoryConfiguration());
        builder.ApplyConfiguration(new ProProductTypeConfiguration());
        builder.ApplyConfiguration(new ProProductConfiguration());
        builder.ApplyConfiguration(new ProRuleTypeConfiguration());
        builder.ApplyConfiguration(new ProRuleConfiguration());
        builder.ApplyConfiguration(new ProProductPlanDefinitionConfiguration());
        builder.ApplyConfiguration(new ProProductDistributionConfiguration());
        builder.ApplyConfiguration(new ProProductTableRateConfiguration());
        builder.ApplyConfiguration(new ProProductAttributeConfiguration());
        builder.ApplyConfiguration(new ProProductCoverageConfiguration());
        builder.ApplyConfiguration(new ProProductCoverageInteractionConfiguration());
        builder.ApplyConfiguration(new ProProductCoverageLevelConfiguration());
        builder.ApplyConfiguration(new ProProductCoverageLevelTermConfiguration());
        builder.ApplyConfiguration(new ProAttributeConfiguration());
        builder.ApplyConfiguration(new ProTableRateConfiguration());
        builder.ApplyConfiguration(new ProTableRateVariableConfiguration());
        builder.ApplyConfiguration(new ProTableRateLineConfiguration());
        builder.ApplyConfiguration(new ResTaxConfiguration());
        builder.ApplyConfiguration(new ResPartnerTypeConfiguration());
        builder.ApplyConfiguration(new ResOrganizationTypeConfiguration());
        builder.ApplyConfiguration(new ResCountryConfiguration());
        builder.ApplyConfiguration(new ResCarBrandConfiguration());
        builder.ApplyConfiguration(new ResCarLineConfiguration());
        builder.ApplyConfiguration(new ResCarGroupConfiguration());
        builder.ApplyConfiguration(new ResCarModelConfiguration());
        builder.ApplyConfiguration(new ResCarTypeConfiguration());
        builder.ApplyConfiguration(new ResCarCategoryConfiguration());
        builder.ApplyConfiguration(new InsurerDictionaryConfiguration());
        builder.ApplyConfiguration(new PolicyTypeConfiguration());
        builder.ApplyConfiguration(new PolicyConfiguration());
        builder.ApplyConfiguration(new PolicyVersionConfiguration());
        builder.ApplyConfiguration(new PolicyCertificateConfiguration());
        builder.ApplyConfiguration(new PolicyDocumentConfiguration());
        builder.ApplyConfiguration(new PolicyContractDocumentConfiguration());
        builder.ApplyConfiguration(new PolicyAmountConfiguration());
        builder.ApplyConfiguration(new PolicyRiskObjectConfiguration());
        builder.ApplyConfiguration(new PolicyRiskObjectDocumentConfiguration());
        builder.ApplyConfiguration(new PolicyRiskMotorConfiguration());
        builder.ApplyConfiguration(new PolicyProductConfiguration());
        builder.ApplyConfiguration(new PolicyCoverageConfiguration());
        builder.ApplyConfiguration(new PolicyCoverageLevelConfiguration());
        builder.ApplyConfiguration(new PolicyContractConfiguration());
        builder.ApplyConfiguration(new ResAppChannelConfiguration());
        builder.ApplyConfiguration(new ResMotorClassConfiguration());
        builder.ApplyConfiguration(new ResProvinceConfiguration());
        builder.ApplyConfiguration(new ResWardConfiguration());
        builder.ApplyConfiguration(new ResChannelConfiguration());
        builder.ApplyConfiguration(new ResAgreementTermConfiguration());
        builder.ApplyConfiguration(new ResDocumentTypeConfiguration());
        builder.ApplyConfiguration(new ResDocumentConfiguration());
        builder.ApplyConfiguration(new AccountPaymentRequestConfiguration());
        builder.ApplyConfiguration(new AdminConfigConfiguration());
        builder.ApplyConfiguration(new ResPartnerConfiguration());
        builder.ApplyConfiguration(new ResPartnerAgreementConfiguration());
        builder.ApplyConfiguration(new ResBankConfiguration());
        builder.ApplyConfiguration(new HrDepartmentConfiguration());
        builder.ApplyConfiguration(new ResObjectTypeConfiguration());
        builder.ApplyConfiguration(new ResRiskConfiguration());
        builder.ApplyConfiguration(new ResDamageLevelConfiguration());
        builder.ApplyConfiguration(new HrEmployeeConfiguration());
        builder.ApplyConfiguration(new HrEmployeeRoleRelConfiguration());
        builder.ApplyConfiguration(new ResIndustryConfiguration());
        builder.ApplyConfiguration(new ResCustomerConfiguration());
        builder.ApplyConfiguration(new ResClaimTypeConfiguration());
        builder.ApplyConfiguration(new ClaimConfiguration());
        builder.ApplyConfiguration(new ClaimIncidentConfiguration());
        builder.ApplyConfiguration(new ClaimIncidentRiskMotorConfiguration());
        builder.ApplyConfiguration(new ClaimFolderConfiguration());
        builder.ApplyConfiguration(new ClaimFolderIncidentObjectConfiguration());
        builder.ApplyConfiguration(new ClaimFolderExposureConfiguration());
        builder.ApplyConfiguration(new ClaimFolderExposureEstimateConfiguration());
        builder.ApplyConfiguration(new ClaimFolderItemConfiguration());
        builder.ApplyConfiguration(new ClaimFolderItemPlanConfiguration());
        builder.ApplyConfiguration(new ClaimAdjustAtLocationConfiguration());
        builder.ApplyConfiguration(new ClaimDocumentConfiguration());
        builder.ApplyConfiguration(new ClaimStageConfiguration());
        builder.ApplyConfiguration(new ResClaimStageConfiguration());
        builder.ApplyConfiguration(new ResClaimStageTaskConfiguration());
        builder.ApplyConfiguration(new ClaimSlaConfiguration());
        builder.ApplyConfiguration(new ClaimFolderQuotationConfiguration());
        builder.ApplyConfiguration(new ClaimFolderQuotationApprovalConfiguration());
        builder.ApplyConfiguration(new ClaimFolderQuotationDetailConfiguration());
        builder.ApplyConfiguration(new ClaimFolderEvaluateConfiguration());
        builder.ApplyConfiguration(new ClaimFolderEvaluateDetailConfiguration());
        builder.ApplyConfiguration(new ResClaimEvaluateItemConfiguration());
        builder.ApplyConfiguration(new ClaimDocumentConfiguration());
        builder.ApplyConfiguration(new ResIncidentCauseConfiguration());
        builder.ApplyConfiguration(new ResIncidentLevelConfiguration());
        builder.ApplyConfiguration(new ResSequenceConfiguration());
        builder.ApplyConfiguration(new ResCurrencyConfiguration());
        builder.ApplyConfiguration(new ResReasonGroupConfiguration());
        builder.ApplyConfiguration(new ResReasonConfiguration());
        builder.ApplyConfiguration(new ResFeeItemConfiguration());
        builder.ApplyConfiguration(new ResClaimPlanConfiguration());
        builder.ApplyConfiguration(new ResPaymentMethodConfiguration());
        builder.ApplyConfiguration(new ResPaymentTypeConfiguration());
        builder.ApplyConfiguration(new ResUomClassConfiguration());
        builder.ApplyConfiguration(new ResUomConfiguration());
        builder.ApplyConfiguration(new ResObjectItemTypeConfiguration());
        builder.ApplyConfiguration(new ResObjectItemDepreciationConfiguration());
        builder.ApplyConfiguration(new ResBusinessAssigneeConfiguration());
        builder.ApplyConfiguration(new ResBusinessAuthorityConfiguration());
        builder.ApplyConfiguration(new ResTaskCategoryConfiguration());
        builder.ApplyConfiguration(new BusinessFlowConfiguration());
        builder.ApplyConfiguration(new WorkInstanceConfiguration());
        builder.ApplyConfiguration(new WorkTaskConfiguration());
        builder.ApplyConfiguration(new ResEventConfiguration());
        builder.ApplyConfiguration(new ResEventNotifyTemplateConfiguration());
        builder.ApplyConfiguration(new ResUserDeviceConfiguration());
        builder.ApplyConfiguration(new SystemEventNotifyConfiguration());
        builder.ApplyConfiguration(new ReportTemplateConfiguration());
        builder.ApplyConfiguration(new ReportTemplateSqlConfiguration());
        builder.ApplyConfiguration(new ReportTemplateParameterConfiguration());
        builder.ApplyConfiguration(new ReportTemplateSqlParameterConfiguration());
        builder.ApplyConfiguration(new ApiKeyConfiguration());
        builder.ApplyConfiguration(new ResPartnerMessageLoggingConfiguration());
        builder.ApplyConfiguration(new PartnerConsentConfiguration());
    }


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext .
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    #endregion
}
