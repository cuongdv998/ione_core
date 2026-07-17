using iOne.Master.Localization;
using iOne.Master.Navigation;
using iOne.Master.ResDocuments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

namespace iOne.Master;

[DependsOn(
    typeof(iOneDomainModule),
    typeof(iOneMasterApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpAuditLoggingApplicationModule)
    )]
public class iOneMasterApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<iOneMasterApplicationModule>();
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Master", typeof(MasterResource));
            options.MapCodeNamespace("ResEvent", typeof(MasterResource));
        });

        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new MasterMenuContributor());
        });

        // Configure MinIO
        var configuration = context.Services.GetConfiguration();
        Configure<MinIOOptions>(configuration.GetSection("MinIO"));
        
        // Register MinIO service
        context.Services.AddSingleton<IMinIOService, MinIOService>();
    }
}

