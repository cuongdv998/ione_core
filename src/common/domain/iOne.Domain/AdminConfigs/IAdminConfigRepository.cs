using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.AdminConfigs;

public interface IAdminConfigRepository : IRepository<AdminConfig, Guid>
{
    Task<bool> IsCodeSubCodeExistsAsync(string code, string subCode, Guid? excludeId = null);
    Task<AdminConfig?> FindByCodeSubCodeAsync(string code, string subCode);
}

