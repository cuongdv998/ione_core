using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyRiskMotors;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policy-risk-motors")]
[Authorize(PolicyRiskMotorPermissions.Default)]
public class PolicyRiskMotorController : AbpControllerBase
{
    protected IPolicyRiskMotorAppService AppService { get; }

    public PolicyRiskMotorController(IPolicyRiskMotorAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(PolicyRiskMotorPermissions.View)]
    public virtual Task<PagedResultDto<PolicyRiskMotorDto>> GetListAsync(GetPolicyRiskMotorsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyRiskMotorPermissions.View)]
    public virtual Task<PolicyRiskMotorDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(PolicyRiskMotorPermissions.Create)]
    public virtual Task<PolicyRiskMotorDto> CreateAsync(CreatePolicyRiskMotorDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyRiskMotorPermissions.Edit)]
    public virtual Task<PolicyRiskMotorDto> UpdateAsync(Guid id, UpdatePolicyRiskMotorDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyRiskMotorPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
