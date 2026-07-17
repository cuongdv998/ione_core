using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyProducts;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policy-products")]
[Authorize(PolicyProductPermissions.Default)]
public class PolicyProductController : AbpControllerBase
{
    protected IPolicyProductAppService AppService { get; }

    public PolicyProductController(IPolicyProductAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(PolicyProductPermissions.View)]
    public virtual Task<PagedResultDto<PolicyProductDto>> GetListAsync(GetPolicyProductsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyProductPermissions.View)]
    public virtual Task<PolicyProductDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(PolicyProductPermissions.Create)]
    public virtual Task<PolicyProductDto> CreateAsync(CreatePolicyProductDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyProductPermissions.Edit)]
    public virtual Task<PolicyProductDto> UpdateAsync(Guid id, UpdatePolicyProductDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyProductPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
