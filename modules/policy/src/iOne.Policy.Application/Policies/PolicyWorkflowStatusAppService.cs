using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Policies;
using iOne.WorkTasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace iOne.Policy.Policies;

public class PolicyWorkflowStatusAppService : ApplicationService, IPolicyWorkflowStatusAppService
{
    private const string PolicyBusinessName = "policyVersion";

    private readonly IRepository<iOne.Policies.Policy, Guid> _policyRepository;
    private readonly IRepository<PolicyVersion, Guid> _policyVersionRepository;
    private readonly IWorkTaskRepository _workTaskRepository;
    private readonly IClock _clock;

    public PolicyWorkflowStatusAppService(
        IRepository<iOne.Policies.Policy, Guid> policyRepository,
        IRepository<PolicyVersion, Guid> policyVersionRepository,
        IWorkTaskRepository workTaskRepository,
        IClock clock)
    {
        _policyRepository = policyRepository;
        _policyVersionRepository = policyVersionRepository;
        _workTaskRepository = workTaskRepository;
        _clock = clock;
    }

    public async Task UpdateStatusAsync(UpdatePolicyStatusInput input)
    {
        if (string.IsNullOrWhiteSpace(input.PolicyVersionId) || !Guid.TryParse(input.PolicyVersionId.Trim(), out var policyVersionId))
            throw new BusinessException("Policy:UpdateStatus:InvalidPolicyId");

        var action = input.Action?.Trim();
        if (string.IsNullOrWhiteSpace(action))
            throw new BusinessException("Policy:UpdateStatus:ActionRequired");

        var policyVersion = await _policyVersionRepository.FindAsync(policyVersionId);
        if (policyVersion == null)
            throw new BusinessException("Policy:UpdateStatus:PolicyNotFound");

        if (string.Equals(action, "approve", StringComparison.OrdinalIgnoreCase))
        {
            var query = await _policyRepository.GetQueryableAsync();
            var policy = await query
                .Include(p => p.PolicyVersions)
                .Where(p => p.Id == policyVersion.PolicyId)
                .FirstOrDefaultAsync();

            if (policy == null)
                throw new BusinessException("Policy:UpdateStatus:PolicyNotFound");
            
            policy.UpdateStatus(PolicyStatus.Active);
            policy.UpdateApprovalStatus("approved");
            await _policyRepository.UpdateAsync(policy);

            var currentVersion = policy.PolicyVersions
                .FirstOrDefault(v => v.Id == policyVersionId);
            if (currentVersion != null)
            {
                // Sync latest policy_version.status and approval status when policy is approved
                currentVersion.UpdateStatus(PolicyStatus.Active.ToString().ToLowerInvariant());
                currentVersion.UpdateApprovalStatus("approved");
                var workTaskQuery = await _workTaskRepository.GetQueryableAsync();
                var latestWorkTask = await workTaskQuery
                    .Where(wt => wt.BusinessName == PolicyBusinessName && wt.BusinessKey == policyVersionId)
                    .OrderByDescending(wt => wt.CreationTime)
                    .FirstOrDefaultAsync();
                currentVersion.UpdateApproverId(latestWorkTask?.AssigneeId);
                currentVersion.UpdateApprovalDate(_clock.Now);
                await _policyVersionRepository.UpdateAsync(currentVersion);
            }
        }
        else if (string.Equals(action, "reject", StringComparison.OrdinalIgnoreCase))
        {
            var query = await _policyRepository.GetQueryableAsync();
            var policy = await query
                .Include(p => p.PolicyVersions)
                .Where(p => p.Id == policyVersion.PolicyId)
                .FirstOrDefaultAsync();

            if (policy == null)
                throw new BusinessException("Policy:UpdateStatus:PolicyNotFound");

            var currentVersion = policy.PolicyVersions
                .FirstOrDefault(v => v.Id == policyVersionId);
            if (currentVersion != null)
            {
                currentVersion.UpdateStatus("rejected");
                currentVersion.UpdateApprovalStatus("rejected");
                await _policyVersionRepository.UpdateAsync(currentVersion);
            }
        }
        else if (string.Equals(action, "terminate", StringComparison.OrdinalIgnoreCase))
        {
            var query = await _policyRepository.GetQueryableAsync();
            var policy = await query
                .Include(p => p.PolicyVersions)
                .Where(p => p.Id == policyVersion.PolicyId)
                .FirstOrDefaultAsync();

            if (policy == null)
                throw new BusinessException("Policy:UpdateStatus:PolicyNotFound");

            policy.UpdateStatus(PolicyStatus.Terminated);
            policy.UpdateApprovalStatus("approved");
            await _policyRepository.UpdateAsync(policy);

            foreach (var version in policy.PolicyVersions)
            {
                version.UpdateStatus("terminated");
                version.UpdateApprovalStatus("approved");
                version.UpdateTerminationStatus(PolicyTerminationStatus.Approved);
            }
            await _policyVersionRepository.UpdateManyAsync(policy.PolicyVersions);
        }
        else
        {
            throw new BusinessException("Policy:UpdateStatus:InvalidAction");
        }
    }

    public async Task UpdateTerminationStatusAsync(UpdateTerminationStatusInput input)
    {
        if (string.IsNullOrWhiteSpace(input.PolicyVersionId) || !Guid.TryParse(input.PolicyVersionId.Trim(), out var policyVersionId))
            throw new BusinessException("Policy:UpdateStatus:InvalidPolicyId");

        var action = input.Action?.Trim();
        if (string.IsNullOrWhiteSpace(action))
            throw new BusinessException("Policy:UpdateStatus:ActionRequired");

        var policyVersion = await _policyVersionRepository.FindAsync(policyVersionId);
        if (policyVersion == null)
            throw new BusinessException("Policy:UpdateStatus:PolicyNotFound");

        PolicyTerminationStatus? status = null;
        if (string.Equals(action, "approve", StringComparison.OrdinalIgnoreCase))
            status = PolicyTerminationStatus.Approved;
        else if (string.Equals(action, "reject", StringComparison.OrdinalIgnoreCase))
        {
            status = PolicyTerminationStatus.Rejected;
            policyVersion.UpdateStatus("active");
        }
        else
            throw new BusinessException("Policy:UpdateStatus:InvalidAction");

        policyVersion.UpdateTerminationStatus(status);
        await _policyVersionRepository.UpdateAsync(policyVersion);
    }
}
