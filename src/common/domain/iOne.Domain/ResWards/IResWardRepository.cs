using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResWards;

public interface IResWardRepository : IRepository<ResWard, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> AnyByProvinceIdAsync(Guid provinceId);
    Task<ResWard?> FindByCodeAsync(string code);
}

