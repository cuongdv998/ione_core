using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProCoverageLevelBasis;

public interface IProCoverageLevelBasisAppService : ICrudAppService<
    ProCoverageLevelBasisDto,
    Guid,
    GetProCoverageLevelBasisInput,
    CreateProCoverageLevelBasisDto,
    UpdateProCoverageLevelBasisDto>
{
}
