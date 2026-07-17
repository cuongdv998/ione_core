using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResSequences;

public interface IResSequenceRepository : IRepository<ResSequence, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}

