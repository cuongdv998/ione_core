using System;
using Volo.Abp.Application.Services;

namespace iOne.Policy.PolicyVersions;

public interface IPolicyVersionAppService : ICrudAppService<
    PolicyVersionDto,
    Guid,
    GetPolicyVersionsInput,
    CreatePolicyVersionDto,
    UpdatePolicyVersionDto>
{
    // No custom methods for now, following basic CRUD pattern
}
