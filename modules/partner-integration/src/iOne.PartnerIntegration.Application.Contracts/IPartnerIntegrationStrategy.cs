using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iOne.PartnerIntegration;

public interface IPartnerIntegrationStrategy
{
    bool CanHandle(string partnerCode);

    Task<List<PartnerIntegrationResult>> IssuePolicyAsync(
        PartnerIssuePolicyContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a motorbike policy to the partner and returns a structured result containing
    /// the partner-side policy key, contract number, and payment information.
    /// Implementations should throw on failure (allowing UoW rollback).
    /// Default: <see cref="NotSupportedException"/> for strategies that don't handle motor policies.
    /// </summary>
    Task<MotorPolicyIssueResult> IssueMotorPolicyAsync(
        PartnerIssuePolicyContext context, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            $"Partner strategy for '{context.PartnerCode}' does not support IssueMotorPolicyAsync.");
    }

    /// <summary>
    /// Queries the partner's API for an existing policy and returns the raw partner response.
    /// Default: <see cref="NotSupportedException"/> for strategies that don't support inquiry.
    /// </summary>
    Task<PartnerPolicyInquiryResult> InquiryPolicyAsync(
        PartnerPolicyInquiryContext context, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            $"Partner strategy for '{context.PartnerCode}' does not support InquiryPolicyAsync.");
    }
}
