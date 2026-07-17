using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyCertificates;

namespace iOne.Policy.Controllers;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy/policy-certificates")]
[Authorize(PolicyCertificatePermissions.Default)]
public class PolicyCertificateController : AbpControllerBase
{
    protected IPolicyCertificateAppService AppService { get; }

    public PolicyCertificateController(IPolicyCertificateAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(PolicyCertificatePermissions.View)]
    public virtual Task<PagedResultDto<PolicyCertificateDto>> GetListAsync(GetPolicyCertificatesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("url-by-policy/{policyId}")]
    [Authorize(PolicyCertificatePermissions.View)]
    public virtual Task<PolicyCertificateDto> GetUrlByPolicyIdAsync(Guid policyId, [FromQuery] Guid? policyVersionId)
    {
        return AppService.GetUrlByPolicyIdAsync(policyId, policyVersionId);
    }

    [HttpGet("{id}")]
    [Authorize(PolicyCertificatePermissions.View)]
    public virtual Task<PolicyCertificateDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(PolicyCertificatePermissions.Create)]
    public virtual Task<PolicyCertificateDto> CreateAsync(CreatePolicyCertificateDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyCertificatePermissions.Edit)]
    public virtual Task<PolicyCertificateDto> UpdateAsync(Guid id, UpdatePolicyCertificateDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(PolicyCertificatePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
