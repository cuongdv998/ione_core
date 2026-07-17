using System;
using Volo.Abp.Application.Services;

namespace iOne.Policy.PolicyCoverageLevels;

public interface IPolicyCoverageLevelAppService : ICrudAppService<
    PolicyCoverageLevelDto,
    Guid,
    GetPolicyCoverageLevelsInput,
    CreatePolicyCoverageLevelDto,
    UpdatePolicyCoverageLevelDto>
{
    // No custom methods for now, following basic CRUD pattern
}
