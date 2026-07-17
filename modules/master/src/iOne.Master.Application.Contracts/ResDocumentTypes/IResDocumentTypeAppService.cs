using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResDocumentTypes;

public interface IResDocumentTypeAppService : ICrudAppService<
    ResDocumentTypeDto,
    Guid,
    GetResDocumentTypesInput,
    CreateResDocumentTypeDto,
    UpdateResDocumentTypeDto>
{
    Task<ResDocumentTypeDto> GetByCodeAsync(string code);
    Task<List<ResDocumentTypeDto>> GetListByDocumentGroupCodeAsync(string documentGroupCode);
}
