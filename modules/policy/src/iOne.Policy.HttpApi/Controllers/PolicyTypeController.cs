using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyTypes;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policy-types")]
[Authorize(PolicyTypePermissions.Default)]
public class PolicyTypeController : AbpControllerBase
{
    protected IPolicyTypeAppService AppService { get; }

    public PolicyTypeController(IPolicyTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(PolicyTypePermissions.View)]
    public virtual Task<PagedResultDto<PolicyTypeDto>> GetListAsync(GetPolicyTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyTypePermissions.View)]
    public virtual Task<PolicyTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(PolicyTypePermissions.Create)]
    public virtual Task<PolicyTypeDto> CreateAsync(CreatePolicyTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyTypePermissions.Edit)]
    public virtual Task<PolicyTypeDto> UpdateAsync(Guid id, UpdatePolicyTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyTypePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
