using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AuditLogging;

namespace iOne.Partner;

[DependsOn(
    typeof(iOnePartnerApplicationContractsModule),
    typeof(iOnePartnerApplicationModule), // Cần reference để access Assembly
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpAuditLoggingHttpApiModule)
    )]
public class iOnePartnerHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Create conventional controllers for Partner module, but exclude services that have manual controllers
            options.ConventionalControllers.Create(
                typeof(iOnePartnerApplicationModule).Assembly,
                opts =>
                {
                    opts.TypePredicate = type =>
                        type.Name != "ResPartnerTypeAppService" &&
                        type.Name != "ResOrganizationTypeAppService" &&
                        type.Name != "ResChannelAppService" &&
                        type.Name != "ResAgreementTermAppService" &&
                        type.Name != "ResPartnerAppService"; // Exclude các AppService có manual controller
                });
        });
    }
}

