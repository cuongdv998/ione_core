using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCarBrands;

public interface IResCarBrandRepository : IRepository<ResCarBrand, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}



