using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResObjectTypeItems;

public interface IResObjectTypeItemAppService : ICrudAppService<
    ResObjectTypeItemDto,
    Guid,
    GetResObjectTypeItemsInput,
    CreateResObjectTypeItemDto,
    UpdateResObjectTypeItemDto>
{
    Task<ImportResObjectTypeItemResultDto> ImportExcelAsync(byte[] fileBytes);
    Task<byte[]> ExportTemplateAsync();
}
