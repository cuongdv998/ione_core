using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResObjectItemDepreciations;

public interface IResObjectItemDepreciationAppService : ICrudAppService<
    ResObjectItemDepreciationDto,
    Guid,
    GetResObjectItemDepreciationsInput,
    CreateResObjectItemDepreciationDto,
    UpdateResObjectItemDepreciationDto>
{
    Task<ImportResObjectItemDepreciationResultDto> ImportExcelAsync(byte[] fileBytes);
    Task<byte[]> ExportTemplateAsync();
}
