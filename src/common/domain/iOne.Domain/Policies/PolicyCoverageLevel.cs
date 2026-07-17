using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Policies;

[Table("policy_coverage_level")]
public class PolicyCoverageLevel : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    [Required]
    public virtual Guid PolicyCoverageId { get; private set; }

    [Required]
    public virtual Guid CoverageLevelTypeId { get; private set; }

    [Required]
    public virtual Guid CoverageLevelBasisId { get; private set; }

    public virtual string? ConditionScript { get; private set; }

    public virtual string? ComputeScript { get; private set; }

    [Required]
    [MaxLength(15)]
    public virtual string AmountType { get; private set; } = null!;

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal FromAmount { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal ToAmount { get; private set; }

    // Navigation Properties
    public virtual PolicyCoverage PolicyCoverage { get; set; } = null!;

    protected PolicyCoverageLevel()
    {
        // For ORM
    }

    public PolicyCoverageLevel(
        Guid id,
        Guid policyCoverageId,
        Guid coverageLevelTypeId,
        Guid coverageLevelBasisId,
        string amountType,
        decimal fromAmount,
        decimal toAmount,
        string? conditionScript = null,
        string? computeScript = null)
        : base(id)
    {
        SetPolicyCoverageId(policyCoverageId);
        SetCoverageLevelTypeId(coverageLevelTypeId);
        SetCoverageLevelBasisId(coverageLevelBasisId);
        SetConditionScript(conditionScript);
        SetComputeScript(computeScript);
        SetAmountType(amountType);
        SetFromAmount(fromAmount);
        SetToAmount(toAmount);
    }

    // Private setters with validation
    private void SetPolicyCoverageId(Guid policyCoverageId)
    {
        if (policyCoverageId == Guid.Empty)
        {
            throw new ArgumentException("PolicyCoverageId cannot be empty.", nameof(policyCoverageId));
        }
        PolicyCoverageId = policyCoverageId;
    }

    private void SetCoverageLevelTypeId(Guid coverageLevelTypeId)
    {
        if (coverageLevelTypeId == Guid.Empty)
        {
            throw new ArgumentException("CoverageLevelTypeId cannot be empty.", nameof(coverageLevelTypeId));
        }
        CoverageLevelTypeId = coverageLevelTypeId;
    }

    private void SetCoverageLevelBasisId(Guid coverageLevelBasisId)
    {
        if (coverageLevelBasisId == Guid.Empty)
        {
            throw new ArgumentException("CoverageLevelBasisId cannot be empty.", nameof(coverageLevelBasisId));
        }
        CoverageLevelBasisId = coverageLevelBasisId;
    }

    private void SetConditionScript(string? conditionScript)
    {
        ConditionScript = conditionScript;
    }

    private void SetComputeScript(string? computeScript)
    {
        ComputeScript = computeScript;
    }

    private void SetAmountType(string amountType)
    {
        if (string.IsNullOrWhiteSpace(amountType))
        {
            throw new ArgumentException("AmountType cannot be null or empty.", nameof(amountType));
        }
        if (amountType.Length > 15)
        {
            throw new ArgumentException("AmountType cannot exceed 15 characters.", nameof(amountType));
        }
        if (amountType != "percent" && amountType != "fix" && amountType != "quantity")
        {
            throw new ArgumentException("AmountType must be 'percent', 'fix', or 'quantity'.", nameof(amountType));
        }
        AmountType = amountType;
    }

    private void SetFromAmount(decimal fromAmount)
    {
        if (fromAmount < 0)
        {
            throw new ArgumentException("FromAmount cannot be negative.", nameof(fromAmount));
        }
        FromAmount = fromAmount;
    }

    private void SetToAmount(decimal toAmount)
    {
        if (toAmount < 0)
        {
            throw new ArgumentException("ToAmount cannot be negative.", nameof(toAmount));
        }
        if (toAmount < FromAmount)
        {
            throw new ArgumentException("ToAmount must be greater than or equal to FromAmount.", nameof(toAmount));
        }
        ToAmount = toAmount;
    }

    // Public update methods
    public virtual void UpdateCoverageLevelTypeId(Guid coverageLevelTypeId)
    {
        SetCoverageLevelTypeId(coverageLevelTypeId);
    }

    public virtual void UpdateCoverageLevelBasisId(Guid coverageLevelBasisId)
    {
        SetCoverageLevelBasisId(coverageLevelBasisId);
    }

    public virtual void UpdateConditionScript(string? conditionScript)
    {
        SetConditionScript(conditionScript);
    }

    public virtual void UpdateComputeScript(string? computeScript)
    {
        SetComputeScript(computeScript);
    }

    public virtual void UpdateAmountType(string amountType)
    {
        SetAmountType(amountType);
    }

    public virtual void UpdateFromAmount(decimal fromAmount)
    {
        SetFromAmount(fromAmount);
    }

    public virtual void UpdateToAmount(decimal toAmount)
    {
        SetToAmount(toAmount);
    }
}
