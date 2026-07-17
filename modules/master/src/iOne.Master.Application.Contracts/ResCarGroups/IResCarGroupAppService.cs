using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCarGroups;

public interface IResCarGroupAppService : ICrudAppService<
    ResCarGroupDto,
    Guid,
    GetResCarGroupsInput,
    CreateResCarGroupDto,
    UpdateResCarGroupDto>
{
    Task<ImportResCarGroupResultDto> ImportExcelAsync(byte[] fileBytes);
    Task<byte[]> ExportTemplateAsync();
    Task<ListResultDto<ResCarGroupDto>> GetLookupListAsync();
}
