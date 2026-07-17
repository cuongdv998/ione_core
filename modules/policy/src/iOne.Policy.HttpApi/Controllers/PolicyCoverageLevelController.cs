using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyCoverageLevels;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policy-coverage-levels")]
[Authorize(PolicyCoverageLevelPermissions.Default)]
public class PolicyCoverageLevelController : AbpControllerBase
{
    protected IPolicyCoverageLevelAppService AppService { get; }

    public PolicyCoverageLevelController(IPolicyCoverageLevelAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(PolicyCoverageLevelPermissions.View)]
    public virtual Task<PagedResultDto<PolicyCoverageLevelDto>> GetListAsync(GetPolicyCoverageLevelsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyCoverageLevelPermissions.View)]
    public virtual Task<PolicyCoverageLevelDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(PolicyCoverageLevelPermissions.Create)]
    public virtual Task<PolicyCoverageLevelDto> CreateAsync(CreatePolicyCoverageLevelDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyCoverageLevelPermissions.Edit)]
    public virtual Task<PolicyCoverageLevelDto> UpdateAsync(Guid id, UpdatePolicyCoverageLevelDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyCoverageLevelPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
