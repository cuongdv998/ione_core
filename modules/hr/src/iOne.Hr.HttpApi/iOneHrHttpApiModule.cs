using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AuditLogging;

namespace iOne.Hr;

[DependsOn(
    typeof(iOneHrApplicationContractsModule),
    typeof(iOneHrApplicationModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpAuditLoggingHttpApiModule)
    )]
public class iOneHrHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Create conventional controllers for HR module, but exclude services that have manual controllers
            options.ConventionalControllers.Create(
                typeof(iOneHrApplicationModule).Assembly,
                opts =>
                {
                    opts.TypePredicate = type => 
                        type.Name != "HrDepartmentTypeAppService" && 
                        type.Name != "HrDepartmentAppService" &&
                        type.Name != "HrEmployeeRoleAppService" &&
                        type.Name != "HrEmployeeLevelAppService" &&
                        type.Name != "HrEmployeePositionAppService" &&
                        type.Name != "HrEmployeeAppService";
                });
        });
    }
}

