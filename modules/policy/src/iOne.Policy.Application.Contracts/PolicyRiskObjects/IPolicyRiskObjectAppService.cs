using System;
using Volo.Abp.Application.Services;

namespace iOne.Policy.PolicyRiskObjects;

public interface IPolicyRiskObjectAppService : ICrudAppService<
    PolicyRiskObjectDto,
    Guid,
    GetPolicyRiskObjectsInput,
    CreatePolicyRiskObjectDto,
    UpdatePolicyRiskObjectDto>
{
    // No custom methods for now, following basic CRUD pattern
}
