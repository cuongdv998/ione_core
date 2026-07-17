using System;
using Volo.Abp.Application.Services;

namespace iOne.Policy.PolicyCoverages;

public interface IPolicyCoverageAppService : ICrudAppService<
    PolicyCoverageDto,
    Guid,
    GetPolicyCoveragesInput,
    CreatePolicyCoverageDto,
    UpdatePolicyCoverageDto>
{
    // No custom methods for now, following basic CRUD pattern
}
