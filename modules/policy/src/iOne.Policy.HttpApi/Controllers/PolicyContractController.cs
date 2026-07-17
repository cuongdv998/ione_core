using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyContracts;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policy-contracts")]
public class PolicyContractController : AbpControllerBase
{
    protected IPolicyContractAppService AppService { get; }

    public PolicyContractController(IPolicyContractAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(PolicyContractPermissions.View)]
    public virtual Task<PagedResultDto<PolicyContractDto>> GetListAsync(GetPolicyContractsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("search")]
    [Authorize(PolicyContractPermissions.View)]
    public virtual Task<PagedResultDto<PolicyContractSearchResultDto>> SearchAsync([FromQuery] PolicyContractSearchInput input)
    {
        return AppService.SearchAsync(input);
    }

    [HttpGet("export")]
    [Authorize(PolicyContractPermissions.View)]
    public virtual async Task<IActionResult> ExportAsync([FromQuery] PolicyContractSearchInput input)
    {
        var bytes = await AppService.ExportAsync(input);
        return File(bytes, "text/csv", "policy-contracts.csv");
    }

    [HttpGet("export-excel")]
    [Authorize(PolicyContractPermissions.View)]
    public virtual async Task<IActionResult> ExportExcelAsync([FromQuery] PolicyContractSearchInput input)
    {
        var bytes = await AppService.ExportExcelAsync(input);
        var fileName = $"policy-contracts-{DateTime.Now:yyyy-MM-dd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyContractPermissions.View)]
    public virtual Task<PolicyContractDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(PolicyContractPermissions.Create)]
    public virtual Task<PolicyContractDto> CreateAsync(CreatePolicyContractDto input)
    {
        return AppService.CreateWithResultAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyContractPermissions.Edit)]
    public virtual Task<PolicyContractDto> UpdateAsync(Guid id, UpdatePolicyContractDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyContractPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpPost("{id}/cancel")]
    [Authorize(PolicyContractPermissions.Edit)]
    public virtual Task CancelAsync(Guid id)
    {
        return AppService.CancelAsync(id);
    }

    [HttpPost("terminate")]
    [Authorize(PolicyContractPermissions.Edit)]
    public virtual Task TerminateContractAsync([FromBody] TerminateContractInput input)
    {
        return AppService.TerminateContractAsync(input);
    }
}
