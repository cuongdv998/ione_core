using iOne.PartnerIntegration.Pti;
using iOne.PartnerIntegration.Vni;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace iOne.PartnerIntegration;

[DependsOn(
    typeof(iOneDomainModule),
    typeof(iOnePartnerIntegrationApplicationContractsModule)
)]
public class iOnePartnerIntegrationApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient<IVniApiClient, VniApiClient>();
        context.Services.AddHttpClient<IPtiApiClient, PtiApiClient>();
    }
}
