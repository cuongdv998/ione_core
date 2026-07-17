using System;
using Volo.Abp.Application.Services;

namespace iOne.Policy.PolicyProducts;

public interface IPolicyProductAppService : ICrudAppService<
    PolicyProductDto,
    Guid,
    GetPolicyProductsInput,
    CreatePolicyProductDto,
    UpdatePolicyProductDto>
{
    // No custom methods for now, following basic CRUD pattern
}
