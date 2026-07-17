using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AuditLogging;

namespace iOne.Customer;

[DependsOn(
    typeof(iOneCustomerApplicationContractsModule),
    typeof(iOneCustomerApplicationModule), // Cần reference để access Assembly
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpAuditLoggingHttpApiModule)
    )]
public class iOneCustomerHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Create conventional controllers for Customer module, but exclude services that have manual controllers
            options.ConventionalControllers.Create(
                typeof(iOneCustomerApplicationModule).Assembly,
                opts =>
                {
                    opts.TypePredicate = type => 
                        type.Name != "ResIndustryAppService" &&
                        type.Name != "ResCustomerAppService";
                });
        });
    }
}

