using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Policy.Permissions;
using iOne.Policy.Policies;

namespace iOne.Policy.Controllers;

[Authorize]
[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Area(PolicyRemoteServiceConsts.ModuleName)]
[Route("api/policy")]
public class PolicyWorkflowApiController : AbpControllerBase
{
    protected IPolicyPartnerAppService PolicyPartnerAppService { get; }
    protected IPolicyWorkflowStatusAppService PolicyWorkflowStatusAppService { get; }
    protected IPolicyAppService PolicyAppService { get; }
    protected IPolicyMotorPartnerAppService PolicyMotorPartnerAppService { get; }

    public PolicyWorkflowApiController(
        IPolicyPartnerAppService policyPartnerAppService,
        IPolicyWorkflowStatusAppService policyWorkflowStatusAppService,
        IPolicyAppService policyAppService,
        IPolicyMotorPartnerAppService policyMotorPartnerAppService)
    {
        PolicyPartnerAppService = policyPartnerAppService;
        PolicyWorkflowStatusAppService = policyWorkflowStatusAppService;
        PolicyAppService = policyAppService;
        PolicyMotorPartnerAppService = policyMotorPartnerAppService;
    }

    /// <summary>
    /// Creates a PolicyAmount record for policy termination (fee item TERMINATE_REFUND_AMOUNT).
    /// Called by the termination workflow; does not update policy status.
    /// </summary>
    [HttpPost("create-terminate-policy-amount")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task CreateTerminatePolicyAmountAsync(CreateTerminatePolicyAmountInput input)
    {
        return PolicyAppService.CreateTerminatePolicyAmountAsync(input);
    }

    [HttpPost("partner-create-policy")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task<PartnerCreatePolicyResultDto> PartnerCreatePolicyAsync(PartnerCreatePolicyInput input)
    {
        return PolicyPartnerAppService.PartnerCreatePolicyAsync(input);
    }

    [HttpPost("update-status")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task UpdateStatusAsync(UpdatePolicyStatusInput input)
    {
        return PolicyWorkflowStatusAppService.UpdateStatusAsync(input);
    }

    [HttpPost("update-prev-effect-date")]
    [Authorize(PolicyPermissions.Edit)]
    public virtual Task<UpdatePrevEffectDateResultDto> UpdatePrevEffectDateAsync(UpdatePrevEffectDateInputDto input)
    {
        return PolicyAppService.UpdatePrevEffectDateAsync(input);
    }

    /// <summary>
    /// Queries the partner's API for the policy with the given <paramref name="policyId"/> and
    /// returns the raw partner response. Uses Policy.InsurerPolicyNo as the partner-side key.
    /// </summary>
    [HttpGet("partner-policy-inquiry")]
    [Authorize(PolicyPermissions.View)]
    public virtual Task<PartnerPolicyInquiryResultDto> PartnerPolicyInquiryAsync([FromQuery] string policyId)
    {
        return PolicyPartnerAppService.PartnerPolicyInquiryAsync(policyId);
    }

    /// <summary>
    /// Calls the partner's motorbike policy issuance API for the given policy.
    /// On success, persists the partner identifiers (InsurerPolicyNo, InsurerContractCode)
    /// and creates an AccountPaymentRequest (PendingApproval) without updating policy_amount payment_status (stays new).
    /// Throws when the policy is not a motorbike LOB policy.
    /// </summary>
    [HttpPost("partner-motor-policy-issue")]
    [Authorize(PolicyPermissions.Create)]
    public virtual Task<PartnerMotorPolicyIssueResultDto> PartnerMotorPolicyIssueAsync(
        PartnerMotorPolicyIssueInput input)
    {
        return PolicyMotorPartnerAppService.IssueMotorPolicyAsync(input);
    }
}
