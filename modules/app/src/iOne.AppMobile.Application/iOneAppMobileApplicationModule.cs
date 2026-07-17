using iOne.AppMobile.Firebases;
using iOne.AppMobile.Localization;
using iOne.AppMobile.Navigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
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

namespace iOne.AppMobile;

[DependsOn(
    typeof(iOneDomainModule),
    typeof(iOneAppMobileApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpAuditLoggingApplicationModule)
    )]
public class iOneAppMobileApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<iOneAppMobileApplicationModule>();
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("AppMobile", typeof(AppMobileResource));
        });

        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new AppMobileMenuContributor());
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = context.ServiceProvider.GetRequiredService<ILogger<FirebaseAppService>>();
        
        // Initialize Firebase at application startup
        FirebaseAppService.InitializeFirebase(configuration, logger);
    }
}
