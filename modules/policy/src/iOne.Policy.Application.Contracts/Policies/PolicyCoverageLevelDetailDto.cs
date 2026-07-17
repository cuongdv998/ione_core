using System;

namespace iOne.Policy.Policies;

public class PolicyCoverageLevelDetailDto
{
    public Guid Id { get; set; } // policy_coverage_level.id

    public Guid CoverageLevelTypeId { get; set; }
    public Guid CoverageLevelBasisId { get; set; }

    public string AmountType { get; set; } = null!;
    public decimal FromAmount { get; set; }
    public decimal ToAmount { get; set; }

    public string? ConditionScript { get; set; }
    public string? ComputeScript { get; set; }
}

