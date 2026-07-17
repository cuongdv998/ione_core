using System;
using Volo.Abp.Application.Services;

namespace iOne.Policy.PolicyTypes;

public interface IPolicyTypeAppService : ICrudAppService<
    PolicyTypeDto,
    Guid,
    GetPolicyTypesInput,
    CreatePolicyTypeDto,
    UpdatePolicyTypeDto>
{
    // No custom methods like Import/Export for now, similar to ResCarBrand's basic CRUD
}
