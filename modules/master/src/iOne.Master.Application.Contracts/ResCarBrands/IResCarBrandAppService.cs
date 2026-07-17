using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCarBrands;

public interface IResCarBrandAppService : ICrudAppService<
    ResCarBrandDto,
    Guid,
    GetResCarBrandsInput,
    CreateResCarBrandDto,
    UpdateResCarBrandDto>
{
    Task<ImportResCarBrandResultDto> ImportExcelAsync(byte[] fileBytes);
    Task<byte[]> ExportTemplateAsync();
}

