using iOne.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace iOne.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(iOneEntityFrameworkCoreModule),
    typeof(iOneApplicationContractsModule)
)]
public class iOneDbMigratorModule : AbpModule
{
}
