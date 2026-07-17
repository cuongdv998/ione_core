using System.Threading.Tasks;

namespace iOne.Data;

public interface IiOneDbSchemaMigrator
{
    Task MigrateAsync();
}
