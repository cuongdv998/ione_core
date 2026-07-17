using System;
using System.Collections.Generic;
using iOne.Policies;

namespace iOne.PartnerIntegration;

public class PartnerIssueBillInfo
{
    public bool IsChoice { get; set; }
    public string? BillActor { get; set; }
    public string? Email { get; set; }
}

public class PartnerIssuePolicyContext
{
    public Guid PolicyId { get; set; }

    public string PartnerCode { get; set; } = null!;

    public Policy Policy { get; set; } = null!;

    public PolicyVersion CurrentVersion { get; set; } = null!;

    public List<PolicyProduct> Products { get; set; } = new();

    public PolicyRiskMotor? FirstMotor { get; set; }

    public PartnerIssueBillInfo? BillInfo { get; set; }

    public PartnerIssueOwnerInfo? OwnerInfo { get; set; }
}

public class PartnerIssueOwnerInfo
{
    public bool IsCustomer { get; set; } = true;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
}
