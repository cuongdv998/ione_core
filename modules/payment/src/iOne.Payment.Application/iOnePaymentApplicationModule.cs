using iOne.Payment.Localization;
using iOne.Payment.Navigation;
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
using Microsoft.Extensions.DependencyInjection;
using iOne.Payment.VnPay;

namespace iOne.Payment;

[DependsOn(
    typeof(iOneDomainModule),
    typeof(iOnePaymentApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpAuditLoggingApplicationModule)
    )]
public class iOnePaymentApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<iOnePaymentApplicationModule>();
        });

        var configuration = context.Services.GetConfiguration();
        Configure<VnPayOptions>(configuration.GetSection("VnPay"));

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Payment", typeof(PaymentResource));
        });

        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new PaymentMenuContributor());
        });
    }
}

