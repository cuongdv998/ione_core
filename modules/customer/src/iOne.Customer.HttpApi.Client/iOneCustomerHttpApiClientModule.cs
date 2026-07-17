using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.VirtualFileSystem;
using iOne.Customer;

namespace iOne.Customer;

[DependsOn(
    typeof(iOneCustomerApplicationContractsModule),
    typeof(AbpPermissionManagementHttpApiClientModule),
    typeof(AbpFeatureManagementHttpApiClientModule),
    typeof(AbpAccountHttpApiClientModule),
    typeof(AbpIdentityHttpApiClientModule),
    typeof(AbpSettingManagementHttpApiClientModule)
    )]
public class iOneCustomerHttpApiClientModule : AbpModule
{
    public const string RemoteServiceName = CustomerRemoteServiceConsts.RemoteServiceName;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(iOneCustomerApplicationContractsModule).Assembly,
            RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<iOneCustomerHttpApiClientModule>();
        });
    }
}

