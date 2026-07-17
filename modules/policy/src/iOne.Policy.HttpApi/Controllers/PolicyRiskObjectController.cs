using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyRiskObjects;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policy-risk-objects")]
[Authorize(PolicyRiskObjectPermissions.Default)]
public class PolicyRiskObjectController : AbpControllerBase
{
    protected IPolicyRiskObjectAppService AppService { get; }

    public PolicyRiskObjectController(IPolicyRiskObjectAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(PolicyRiskObjectPermissions.View)]
    public virtual Task<PagedResultDto<PolicyRiskObjectDto>> GetListAsync(GetPolicyRiskObjectsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyRiskObjectPermissions.View)]
    public virtual Task<PolicyRiskObjectDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(PolicyRiskObjectPermissions.Create)]
    public virtual Task<PolicyRiskObjectDto> CreateAsync(CreatePolicyRiskObjectDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyRiskObjectPermissions.Edit)]
    public virtual Task<PolicyRiskObjectDto> UpdateAsync(Guid id, UpdatePolicyRiskObjectDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyRiskObjectPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
