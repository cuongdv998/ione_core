using Volo.Abp.Modularity;

namespace iOne.PartnerIntegration;

[DependsOn(
    typeof(iOneDomainSharedModule)
)]
public class iOnePartnerIntegrationApplicationContractsModule : AbpModule
{
}
