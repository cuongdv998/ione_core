using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProTableRateLines;

public interface IProTableRateLineAppService : ICrudAppService<
    ProTableRateLineDto,
    Guid,
    GetProTableRateLinesInput,
    CreateProTableRateLineDto,
    UpdateProTableRateLineDto>
{
    Task<ImportProTableRateLineResultDto> ImportExcelAsync(byte[] fileBytes, Guid tableRateId);
    Task<byte[]> ExportTemplateAsync(Guid tableRateId);
}
