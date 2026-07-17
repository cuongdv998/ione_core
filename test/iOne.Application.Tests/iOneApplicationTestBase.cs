using Volo.Abp.Modularity;

namespace iOne;

public abstract class iOneApplicationTestBase<TStartupModule> : iOneTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
