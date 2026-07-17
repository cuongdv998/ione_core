using Volo.Abp.Modularity;

namespace iOne;

[DependsOn(
    typeof(iOneDomainModule),
    typeof(iOneTestBaseModule)
)]
public class iOneDomainTestModule : AbpModule
{

}
