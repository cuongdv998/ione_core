using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCarTypes;

public interface IResCarTypeAppService : ICrudAppService<
    ResCarTypeDto,
    Guid,
    GetResCarTypesInput,
    CreateResCarTypeDto,
    UpdateResCarTypeDto>
{
    Task<ImportResCarTypeResultDto> ImportExcelAsync(byte[] fileBytes);
    Task<byte[]> ExportTemplateAsync();
}

