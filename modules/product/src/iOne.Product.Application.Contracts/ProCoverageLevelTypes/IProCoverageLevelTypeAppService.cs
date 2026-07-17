using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProCoverageLevelTypes;

public interface IProCoverageLevelTypeAppService : ICrudAppService<
    ProCoverageLevelTypeDto,
    Guid,
    GetProCoverageLevelTypesInput,
    CreateProCoverageLevelTypeDto,
    UpdateProCoverageLevelTypeDto>
{
}
