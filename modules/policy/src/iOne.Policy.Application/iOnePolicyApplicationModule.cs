using iOne.Policy.Localization;
using iOne.Policy.Navigation;
using iOne.PartnerIntegration;
using Volo.Abp.AutoMapper;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AuditLogging;
using Volo.Abp.UI.Navigation;

namespace iOne.Policy;

[DependsOn(
    typeof(iOneDomainModule),
    typeof(iOnePolicyApplicationContractsModule),
    typeof(iOnePartnerIntegrationApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpAuditLoggingApplicationModule)
    )]
public class iOnePolicyApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<iOnePolicyApplicationModule>();
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Policy", typeof(PolicyResource));
        });

        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new PolicyMenuContributor());
        });
    }
}
