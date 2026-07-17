using System;
using iOne.Policies;

namespace iOne.PartnerIntegration;

public class PartnerPolicyInquiryContext
{
    public Guid PolicyId { get; set; }

    public string PartnerCode { get; set; } = null!;

    public Policy Policy { get; set; } = null!;
}
