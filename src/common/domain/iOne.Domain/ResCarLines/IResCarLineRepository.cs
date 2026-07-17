using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCarLines;

public interface IResCarLineRepository : IRepository<ResCarLine, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}


