using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProCoverages;

public interface IProCoverageAppService : ICrudAppService<
    ProCoverageDto,
    Guid,
    GetProCoveragesInput,
    CreateProCoverageDto,
    UpdateProCoverageDto>
{
}
