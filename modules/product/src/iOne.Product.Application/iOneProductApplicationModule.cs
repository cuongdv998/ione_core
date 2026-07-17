using iOne.Product.Localization;
using iOne.Product.Navigation;
using Volo.Abp.AutoMapper;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AuditLogging;
using Volo.Abp.UI.Navigation;

namespace iOne.Product;

[DependsOn(
    typeof(iOneDomainModule),
    typeof(iOneProductApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpAuditLoggingApplicationModule)
    )]
public class iOneProductApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<iOneProductApplicationModule>();
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Product", typeof(ProductResource));
            options.MapCodeNamespace("ProProductCategory", typeof(ProductResource));
            options.MapCodeNamespace("ProCoverage", typeof(ProductResource));
        });

        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new ProductMenuContributor());
        });
    }
}

