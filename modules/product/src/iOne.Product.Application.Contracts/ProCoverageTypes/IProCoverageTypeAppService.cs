using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProCoverageTypes;

public interface IProCoverageTypeAppService : ICrudAppService<
    ProCoverageTypeDto,
    Guid,
    GetProCoverageTypesInput,
    CreateProCoverageTypeDto,
    UpdateProCoverageTypeDto>
{
}

