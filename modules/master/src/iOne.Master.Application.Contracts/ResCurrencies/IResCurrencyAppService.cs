using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCurrencies;

public interface IResCurrencyAppService : ICrudAppService<
    ResCurrencyDto,
    Guid,
    GetResCurrenciesInput,
    CreateResCurrencyDto,
    UpdateResCurrencyDto>
{
    Task<ImportResCurrencyResultDto> ImportExcelAsync(byte[] fileBytes);
    Task<byte[]> ExportTemplateAsync();
}
