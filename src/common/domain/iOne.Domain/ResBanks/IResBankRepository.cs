using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResBanks;

public interface IResBankRepository : IRepository<ResBank, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}

