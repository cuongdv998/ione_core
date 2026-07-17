using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCarCategories;

public interface IResCarCategoryAppService : ICrudAppService<
    ResCarCategoryDto,
    Guid,
    GetResCarCategoriesInput,
    CreateResCarCategoryDto,
    UpdateResCarCategoryDto>
{
    Task<ImportResCarCategoryResultDto> ImportExcelAsync(byte[] fileBytes);
    Task<byte[]> ExportTemplateAsync();
}


