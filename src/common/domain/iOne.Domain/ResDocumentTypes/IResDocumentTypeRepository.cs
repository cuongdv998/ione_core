using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResDocumentTypes;

public interface IResDocumentTypeRepository : IRepository<ResDocumentType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<ResDocumentType?> FindByCodeAsync(string code);
    Task<List<ResDocumentType>> GetListByDocumentGroupCodeAsync(string documentGroupCode);
}
