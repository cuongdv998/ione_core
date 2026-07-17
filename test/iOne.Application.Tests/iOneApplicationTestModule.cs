using Volo.Abp.Modularity;

namespace iOne;

[DependsOn(
    typeof(iOneApplicationModule),
    typeof(iOneDomainTestModule)
)]
public class iOneApplicationTestModule : AbpModule
{

}
