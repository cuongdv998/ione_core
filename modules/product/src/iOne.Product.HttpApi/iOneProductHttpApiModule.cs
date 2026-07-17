using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AuditLogging;

namespace iOne.Product;

[DependsOn(
    typeof(iOneProductApplicationContractsModule),
    typeof(iOneProductApplicationModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpAuditLoggingHttpApiModule)
    )]
public class iOneProductHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Create conventional controllers for Product module, but exclude services that have manual controllers
            options.ConventionalControllers.Create(
                typeof(iOneProductApplicationModule).Assembly,
                opts =>
                {
                    opts.TypePredicate = type =>
                        type.Name != "ProLineOfBusinessAppService" &&
                        type.Name != "ProCoverageTypeAppService" &&
                        type.Name != "ProCoverageLevelTypeAppService" &&
                        type.Name != "ProProductCategoryAppService" &&
                        type.Name != "ProProductTypeAppService" &&
                        type.Name != "ProCoverageGroupAppService" &&
                        type.Name != "ProCoverageAppService" &&
                        type.Name != "ProAttributeAppService" &&
                        type.Name != "ResTaxAppService";
                });
        });
    }
}

