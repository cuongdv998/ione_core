using System;
using Volo.Abp.Application.Services;

namespace iOne.Policy.PolicyAmounts;

public interface IPolicyAmountAppService : ICrudAppService<
    PolicyAmountDto,
    Guid,
    GetPolicyAmountsInput,
    CreatePolicyAmountDto,
    UpdatePolicyAmountDto>
{
}
