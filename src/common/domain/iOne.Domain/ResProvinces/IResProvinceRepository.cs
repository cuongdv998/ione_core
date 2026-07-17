using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResProvinces;

public interface IResProvinceRepository : IRepository<ResProvince, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}

