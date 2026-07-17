using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyAmounts;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policy-amounts")]
[Authorize(PolicyAmountPermissions.Default)]
public class PolicyAmountController : AbpControllerBase
{
    protected IPolicyAmountAppService AppService { get; }

    public PolicyAmountController(IPolicyAmountAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(PolicyAmountPermissions.View)]
    public virtual Task<PagedResultDto<PolicyAmountDto>> GetListAsync(GetPolicyAmountsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyAmountPermissions.View)]
    public virtual Task<PolicyAmountDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(PolicyAmountPermissions.Create)]
    public virtual Task<PolicyAmountDto> CreateAsync(CreatePolicyAmountDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyAmountPermissions.Edit)]
    public virtual Task<PolicyAmountDto> UpdateAsync(Guid id, UpdatePolicyAmountDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyAmountPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
