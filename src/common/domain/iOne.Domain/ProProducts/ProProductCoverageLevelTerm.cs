using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ProCoverageLevelTypes;
using iOne.ProCoverageLevelBases;

namespace iOne.ProProducts;

[Table("pro_product_coverage_level_term")]
public class ProProductCoverageLevelTerm : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? ProductCoverageLevelId { get; private set; }

    [Required]
    public virtual Guid CoverageLevelTypeId { get; private set; }

    public virtual Guid? CoverageLevelBasisId { get; private set; }

    [Required]
    [MaxLength(15)]
    public virtual string AmountType { get; private set; } = null!;

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal FromAmount { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal ToAmount { get; private set; }

    public virtual string? ConditionScript { get; private set; }

    public virtual string? ComputeScript { get; private set; }

    [MaxLength(15)]
    public virtual string? IsDefault { get; private set; }

    // Navigation properties
    public virtual ProProductCoverageLevel? ProductCoverageLevel { get; set; }
    public virtual ProCoverageLevelType? CoverageLevelType { get; set; }
    public virtual ProCoverageLevelBasis? CoverageLevelBasis { get; set; }

    protected ProProductCoverageLevelTerm()
    {
        // For ORM
    }

    public ProProductCoverageLevelTerm(
        Guid id,
        Guid coverageLevelTypeId,
        string amountType,
        decimal fromAmount,
        decimal toAmount,
        Guid? productCoverageLevelId = null,
        Guid? coverageLevelBasisId = null,
        string? conditionScript = null,
        string? computeScript = null,
        string? isDefault = null)
        : base(id)
    {
        SetProductCoverageLevelId(productCoverageLevelId);
        SetCoverageLevelTypeId(coverageLevelTypeId);
        SetCoverageLevelBasisId(coverageLevelBasisId);
        SetAmountType(amountType);
        SetFromAmount(fromAmount);
        SetToAmount(toAmount);
        SetConditionScript(conditionScript);
        SetComputeScript(computeScript);
        SetIsDefault(isDefault);
    }

    private void SetProductCoverageLevelId(Guid? productCoverageLevelId)
    {
        if (productCoverageLevelId.HasValue && productCoverageLevelId.Value == Guid.Empty)
        {
            throw new ArgumentException("ProductCoverageLevelId cannot be empty if provided.", nameof(productCoverageLevelId));
        }

        ProductCoverageLevelId = productCoverageLevelId;
    }

    private void SetCoverageLevelTypeId(Guid coverageLevelTypeId)
    {
        if (coverageLevelTypeId == Guid.Empty)
        {
            throw new ArgumentException("CoverageLevelTypeId cannot be empty.", nameof(coverageLevelTypeId));
        }

        CoverageLevelTypeId = coverageLevelTypeId;
    }

    private void SetCoverageLevelBasisId(Guid? coverageLevelBasisId)
    {
        if (coverageLevelBasisId.HasValue && coverageLevelBasisId.Value == Guid.Empty)
        {
            throw new ArgumentException("CoverageLevelBasisId cannot be empty if provided.", nameof(coverageLevelBasisId));
        }

        CoverageLevelBasisId = coverageLevelBasisId;
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

    private void SetConditionScript(string? conditionScript)
    {
        ConditionScript = conditionScript;
    }

    private void SetComputeScript(string? computeScript)
    {
        ComputeScript = computeScript;
    }

    private void SetIsDefault(string? isDefault)
    {
        if (isDefault != null && isDefault.Length > 15)
        {
            throw new ArgumentException("IsDefault cannot exceed 15 characters.", nameof(isDefault));
        }

        if (isDefault != null && isDefault != "Y" && isDefault != "N")
        {
            throw new ArgumentException("IsDefault must be 'Y' or 'N' if provided.", nameof(isDefault));
        }

        IsDefault = isDefault;
    }

    // Public update methods
    public virtual void UpdateProductCoverageLevelId(Guid? productCoverageLevelId)
    {
        SetProductCoverageLevelId(productCoverageLevelId);
    }

    public virtual void UpdateCoverageLevelTypeId(Guid coverageLevelTypeId)
    {
        SetCoverageLevelTypeId(coverageLevelTypeId);
    }

    public virtual void UpdateCoverageLevelBasisId(Guid? coverageLevelBasisId)
    {
        SetCoverageLevelBasisId(coverageLevelBasisId);
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

    public virtual void UpdateConditionScript(string? conditionScript)
    {
        SetConditionScript(conditionScript);
    }

    public virtual void UpdateComputeScript(string? computeScript)
    {
        SetComputeScript(computeScript);
    }

    public virtual void UpdateIsDefault(string? isDefault)
    {
        SetIsDefault(isDefault);
    }
}
