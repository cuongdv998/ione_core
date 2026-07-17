using System;
using Volo.Abp.Application.Services;

namespace iOne.Policy.PolicyRiskMotors;

public interface IPolicyRiskMotorAppService : ICrudAppService<
    PolicyRiskMotorDto,
    Guid,
    GetPolicyRiskMotorsInput,
    CreatePolicyRiskMotorDto,
    UpdatePolicyRiskMotorDto>
{
    // No custom methods for now, following basic CRUD pattern
}
