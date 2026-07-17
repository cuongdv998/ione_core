using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.InsurerDictionaries;

public interface IInsurerDictionaryAppService : ICrudAppService<
    InsurerDictionaryDto,
    Guid,
    GetInsurerDictionariesInput,
    CreateInsurerDictionaryDto,
    UpdateInsurerDictionaryDto>
{
    Task<byte[]> ExportTemplateAsync();
    Task<ImportInsurerDictionaryResultDto> ImportExcelAsync(byte[] fileBytes);
}
