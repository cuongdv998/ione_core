using Volo.Abp.Modularity;

namespace iOne;

/* Inherit from this class for your domain layer tests. */
public abstract class iOneDomainTestBase<TStartupModule> : iOneTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
