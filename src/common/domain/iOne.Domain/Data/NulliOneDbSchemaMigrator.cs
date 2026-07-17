using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace iOne.Data;

/* This is used if database provider does't define
 * IiOneDbSchemaMigrator implementation.
 */
public class NulliOneDbSchemaMigrator : IiOneDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
