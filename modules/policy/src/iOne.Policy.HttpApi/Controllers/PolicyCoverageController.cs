using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyCoverages;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policy-coverages")]
[Authorize(PolicyCoveragePermissions.Default)]
public class PolicyCoverageController : AbpControllerBase
{
    protected IPolicyCoverageAppService AppService { get; }

    public PolicyCoverageController(IPolicyCoverageAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(PolicyCoveragePermissions.View)]
    public virtual Task<PagedResultDto<PolicyCoverageDto>> GetListAsync(GetPolicyCoveragesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyCoveragePermissions.View)]
    public virtual Task<PolicyCoverageDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(PolicyCoveragePermissions.Create)]
    public virtual Task<PolicyCoverageDto> CreateAsync(CreatePolicyCoverageDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyCoveragePermissions.Edit)]
    public virtual Task<PolicyCoverageDto> UpdateAsync(Guid id, UpdatePolicyCoverageDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyCoveragePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
