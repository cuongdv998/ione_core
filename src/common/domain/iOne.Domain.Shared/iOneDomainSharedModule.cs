using System.IO;
using iOne.Localization;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.OpenIddict;
using Volo.Abp.BlobStoring.Database;

namespace iOne;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpFeatureManagementDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpSettingManagementDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(BlobStoringDatabaseDomainSharedModule)
    )]
public class iOneDomainSharedModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        iOneGlobalFeatureConfigurator.Configure();
        iOneModuleExtensionConfigurator.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<iOneDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<iOneResource>("vi-VN")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/iOne");

            options.DefaultResourceType = typeof(iOneResource);

            options.Languages.Add(new LanguageInfo("vi-VN", "vi-VN", "Vietnamese (Vietnam)"));
            options.Languages.Add(new LanguageInfo("en", "en", "English"));

        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("iOne", typeof(iOneResource));

            // Domain entities - Master module
            options.MapCodeNamespace("Master", typeof(iOneResource));

            // Domain entities - Customer module
            options.MapCodeNamespace("Customer", typeof(iOneResource));

            // Domain entities - Partner module
            options.MapCodeNamespace("Partner", typeof(iOneResource));

            // Domain entities - Hr module
            options.MapCodeNamespace("Hr", typeof(iOneResource));

            // Domain entities - Policy module
            options.MapCodeNamespace("Policy", typeof(iOneResource));

            // Domain entities - Product module
            options.MapCodeNamespace("Product", typeof(iOneResource));

            // Domain entities without module prefix - Pro entities
            options.MapCodeNamespace("ProLineOfBusiness", typeof(iOneResource));
            options.MapCodeNamespace("ProAttribute", typeof(iOneResource));
            options.MapCodeNamespace("ProCoverage", typeof(iOneResource));
            options.MapCodeNamespace("ProCoverageGroup", typeof(iOneResource));
            options.MapCodeNamespace("ProCoverageType", typeof(iOneResource));
            options.MapCodeNamespace("ProProductCategory", typeof(iOneResource));
            options.MapCodeNamespace("ProProductType", typeof(iOneResource));
            options.MapCodeNamespace("ProTableRate", typeof(iOneResource));

            // Domain entities without module prefix - Hr entities
            options.MapCodeNamespace("HrDepartmentType", typeof(iOneResource));
            options.MapCodeNamespace("HrEmployeeLevel", typeof(iOneResource));
            options.MapCodeNamespace("HrEmployeePosition", typeof(iOneResource));
            options.MapCodeNamespace("HrEmployeeRole", typeof(iOneResource));

            // Domain entities without module prefix - Res entities
            options.MapCodeNamespace("ResOrganizationType", typeof(iOneResource));
            //options.MapCodeNamespace("ResPartner", typeof(iOneResource));
            options.MapCodeNamespace("ResPartnerType", typeof(iOneResource));
            options.MapCodeNamespace("ResTax", typeof(iOneResource));
        });
    }
}
