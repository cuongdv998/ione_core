using iOne.Customer.Localization;
using iOne.Customer.Navigation;
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

namespace iOne.Customer;

[DependsOn(
    typeof(iOneDomainModule),
    typeof(iOneCustomerApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpAuditLoggingApplicationModule)
    )]
public class iOneCustomerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<iOneCustomerApplicationModule>();
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Customer", typeof(CustomerResource));
            options.MapCodeNamespace("ResIndustry", typeof(CustomerResource));
        });

        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new CustomerMenuContributor());
        });
    }
}

