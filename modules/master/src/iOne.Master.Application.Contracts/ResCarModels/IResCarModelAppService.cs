using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCarModels;

public interface IResCarModelAppService : ICrudAppService<
    ResCarModelDto,
    Guid,
    GetResCarModelsInput,
    CreateResCarModelDto,
    UpdateResCarModelDto>
{
    Task<ImportResCarModelResultDto> ImportExcelAsync(byte[] fileBytes);
    Task<byte[]> ExportTemplateAsync();
}

