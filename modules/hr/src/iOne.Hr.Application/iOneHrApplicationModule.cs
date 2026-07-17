using iOne.Hr.Localization;
using iOne.Hr.Navigation;
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

namespace iOne.Hr;

[DependsOn(
    typeof(iOneDomainModule),
    typeof(iOneHrApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpAuditLoggingApplicationModule)
    )]
public class iOneHrApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<iOneHrApplicationModule>();
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("HrDepartment", typeof(HrResource));
            options.MapCodeNamespace("HrDepartmentType", typeof(HrResource));
            options.MapCodeNamespace("HrEmployeeRole", typeof(HrResource));
            options.MapCodeNamespace("HrEmployeeLevel", typeof(HrResource));
            options.MapCodeNamespace("HrEmployeePosition", typeof(HrResource));
            options.MapCodeNamespace("HrEmployee", typeof(HrResource));
            options.MapCodeNamespace("HrEmployeeRoleRel", typeof(HrResource));
        });

        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new HrMenuContributor());
        });
    }
}

