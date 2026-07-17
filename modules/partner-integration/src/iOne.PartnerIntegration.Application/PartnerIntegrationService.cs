using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iOne.Policies;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace iOne.PartnerIntegration;

public class PartnerIntegrationService : IPartnerIntegrationService, ITransientDependency
{
    private readonly IEnumerable<IPartnerIntegrationStrategy> _strategies;
    private readonly ILogger<PartnerIntegrationService> _logger;

    public PartnerIntegrationService(
        IEnumerable<IPartnerIntegrationStrategy> strategies,
        ILogger<PartnerIntegrationService> logger)
    {
        _strategies = strategies;
        _logger = logger;
    }

    public async Task<List<PartnerIntegrationResult>> IssuePolicyAsync(
        Policy policy, CancellationToken cancellationToken = default)
    {
        var partner = policy.Partner;
        if (partner == null)
            return new List<PartnerIntegrationResult>();

        var partnerCode = partner.Code?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(partnerCode))
            return new List<PartnerIntegrationResult>();

        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(partnerCode));
        if (strategy == null)
        {
            _logger.LogWarning("No partner integration strategy found for partner code: {PartnerCode}", partnerCode);
            return new List<PartnerIntegrationResult>();
        }

        var context = BuildContext(policy, partnerCode);
        if (context == null)
            return new List<PartnerIntegrationResult>();

        return await strategy.IssuePolicyAsync(context, cancellationToken);
    }

    public async Task<MotorPolicyIssueResult> IssueMotorPolicyAsync(
        Policy policy,
        PartnerIssueBillInfo? billInfo = null,
        PartnerIssueOwnerInfo? ownerInfo = null,
        CancellationToken cancellationToken = default)
    {
        var partner = policy.Partner;
        if (partner == null)
            throw new BusinessException("Policy:Partner:PartnerNotLoaded")
                .WithData("PolicyId", policy.Id);

        var partnerCode = partner.Code?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(partnerCode))
            throw new BusinessException("Policy:Partner:PartnerCodeMissing")
                .WithData("PolicyId", policy.Id);

        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(partnerCode));
        if (strategy == null)
        {
            _logger.LogWarning(
                "No partner integration strategy found for partner code: {PartnerCode}. Motor policy issue skipped.",
                partnerCode);
            // Return an empty result so non-PTI partners don't block policy creation
            return new MotorPolicyIssueResult();
        }

        var context = BuildContext(policy, partnerCode);
        if (context == null)
            throw new BusinessException("Policy:Partner:PolicyVersionOrProductsMissing")
                .WithData("PolicyId", policy.Id);

        context.BillInfo = billInfo;
        context.OwnerInfo = ownerInfo;

        return await strategy.IssueMotorPolicyAsync(context, cancellationToken);
    }

    public async Task<PartnerPolicyInquiryResult> InquiryPolicyAsync(
        Policy policy, CancellationToken cancellationToken = default)
    {
        var partner = policy.Partner;
        if (partner == null)
            throw new BusinessException("Policy:Partner:PartnerNotLoaded")
                .WithData("PolicyId", policy.Id);

        var partnerCode = partner.Code?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(partnerCode))
            throw new BusinessException("Policy:Partner:PartnerCodeMissing")
                .WithData("PolicyId", policy.Id);

        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(partnerCode));
        if (strategy == null)
            throw new BusinessException("Policy:Partner:StrategyNotFound")
                .WithData("PartnerCode", partnerCode)
                .WithData("PolicyId", policy.Id);

        var context = new PartnerPolicyInquiryContext
        {
            PolicyId = policy.Id,
            PartnerCode = partnerCode,
            Policy = policy
        };

        return await strategy.InquiryPolicyAsync(context, cancellationToken);
    }

    private static PartnerIssuePolicyContext? BuildContext(Policy policy, string partnerCode)
    {
        var currentVersion = policy.PolicyVersions
            .FirstOrDefault(v => v.Id == policy.LastVersionId);
        if (currentVersion == null)
            return null;

        var products = currentVersion.PolicyProducts.ToList();
        if (products.Count == 0)
            return null;

        var firstMotor = currentVersion.PolicyRiskObjects
            .SelectMany(ro => ro.PolicyRiskMotors)
            .FirstOrDefault();

        return new PartnerIssuePolicyContext
        {
            PolicyId = policy.Id,
            PartnerCode = partnerCode,
            Policy = policy,
            CurrentVersion = currentVersion,
            Products = products,
            FirstMotor = firstMotor
        };
    }
}
