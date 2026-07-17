using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyVersions;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policy-versions")]
[Authorize(PolicyVersionPermissions.Default)]
public class PolicyVersionController : AbpControllerBase
{
    protected IPolicyVersionAppService AppService { get; }

    public PolicyVersionController(IPolicyVersionAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(PolicyVersionPermissions.View)]
    public virtual Task<PagedResultDto<PolicyVersionDto>> GetListAsync(GetPolicyVersionsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyVersionPermissions.View)]
    public virtual Task<PolicyVersionDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(PolicyVersionPermissions.Create)]
    public virtual Task<PolicyVersionDto> CreateAsync(CreatePolicyVersionDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyVersionPermissions.Edit)]
    public virtual Task<PolicyVersionDto> UpdateAsync(Guid id, UpdatePolicyVersionDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyVersionPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
