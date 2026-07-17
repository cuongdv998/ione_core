using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResIndustries;

public interface IResIndustryRepository : IRepository<ResIndustry, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}


