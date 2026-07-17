using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ProCoverages;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Policies;

[Table("policy_coverage")]
public class PolicyCoverage : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    [Required]
    public virtual Guid PolicyProductId { get; private set; }

    [Required]
    public virtual Guid CoverageId { get; private set; }

    public virtual Guid? CoverageParentId { get; private set; }

    [MaxLength(50)]
    public virtual string? InsurerCoverageCode { get; private set; }

    [Required]
    public virtual Guid UomId { get; private set; }

    public virtual Guid? TableRateLineId { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? AmountLiability { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal Quantity { get; private set; }

    [Required]
    public virtual Guid TaxId { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? NetRate { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? BaseRate { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? FlatRate { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? Loading { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal PremiumRate { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal PremiumTotal { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal Premium { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal Vat { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? Discount { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? DiscountRate { get; private set; }

    // Navigation Properties
    public virtual PolicyProduct PolicyProduct { get; set; } = null!;
    public virtual ProCoverage Coverage { get; set; } = null!;
    public virtual ICollection<PolicyCoverageLevel> PolicyCoverageLevels { get; set; } = new List<PolicyCoverageLevel>();

    protected PolicyCoverage()
    {
        // For ORM
    }

    public PolicyCoverage(
        Guid id,
        Guid policyProductId,
        Guid coverageId,
        Guid uomId,
        decimal quantity,
        Guid taxId,
        decimal premiumRate,
        decimal premiumTotal,
        decimal premium,
        decimal vat,
        Guid? coverageParentId = null,
        string? insurerCoverageCode = null,
        Guid? tableRateLineId = null,
        decimal? amountLiability = null,
        decimal? netRate = null,
        decimal? baseRate = null,
        decimal? flatRate = null,
        decimal? loading = null,
        decimal? discount = null,
        decimal? discountRate = null)
        : base(id)
    {
        SetPolicyProductId(policyProductId);
        SetCoverageId(coverageId);
        SetCoverageParentId(coverageParentId);
        SetInsurerCoverageCode(insurerCoverageCode);
        SetUomId(uomId);
        SetTableRateLineId(tableRateLineId);
        SetAmountLiability(amountLiability);
        SetQuantity(quantity);
        SetTaxId(taxId);
        SetNetRate(netRate);
        SetBaseRate(baseRate);
        SetFlatRate(flatRate);
        SetLoading(loading);
        SetPremiumRate(premiumRate);
        SetPremiumTotal(premiumTotal);
        SetPremium(premium);
        SetVat(vat);
        SetDiscount(discount);
        SetDiscountRate(discountRate);
    }

    // Private setters with validation
    private void SetPolicyProductId(Guid policyProductId)
    {
        if (policyProductId == Guid.Empty)
        {
            throw new ArgumentException("PolicyProductId cannot be empty.", nameof(policyProductId));
        }
        PolicyProductId = policyProductId;
    }

    private void SetCoverageId(Guid coverageId)
    {
        if (coverageId == Guid.Empty)
        {
            throw new ArgumentException("CoverageId cannot be empty.", nameof(coverageId));
        }
        CoverageId = coverageId;
    }

    private void SetCoverageParentId(Guid? coverageParentId)
    {
        CoverageParentId = coverageParentId;
    }

    private void SetInsurerCoverageCode(string? insurerCoverageCode)
    {
        if (insurerCoverageCode != null && insurerCoverageCode.Length > 50)
        {
            throw new ArgumentException("InsurerCoverageCode cannot exceed 50 characters.", nameof(insurerCoverageCode));
        }
        InsurerCoverageCode = insurerCoverageCode;
    }

    private void SetUomId(Guid uomId)
    {
        if (uomId == Guid.Empty)
        {
            throw new ArgumentException("UomId cannot be empty.", nameof(uomId));
        }
        UomId = uomId;
    }

    private void SetTableRateLineId(Guid? tableRateLineId)
    {
        TableRateLineId = tableRateLineId;
    }

    private void SetAmountLiability(decimal? amountLiability)
    {
        if (amountLiability.HasValue && amountLiability.Value < 0)
        {
            throw new ArgumentException("AmountLiability cannot be negative.", nameof(amountLiability));
        }
        AmountLiability = amountLiability;
    }

    private void SetQuantity(decimal quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));
        }
        Quantity = quantity;
    }

    private void SetTaxId(Guid taxId)
    {
        if (taxId == Guid.Empty)
        {
            throw new ArgumentException("TaxId cannot be empty.", nameof(taxId));
        }
        TaxId = taxId;
    }

    private void SetNetRate(decimal? netRate)
    {
        if (netRate.HasValue && netRate.Value < 0)
        {
            throw new ArgumentException("NetRate cannot be negative.", nameof(netRate));
        }
        NetRate = netRate;
    }

    private void SetBaseRate(decimal? baseRate)
    {
        if (baseRate.HasValue && baseRate.Value < 0)
        {
            throw new ArgumentException("BaseRate cannot be negative.", nameof(baseRate));
        }
        BaseRate = baseRate;
    }

    private void SetFlatRate(decimal? flatRate)
    {
        if (flatRate.HasValue && flatRate.Value < 0)
        {
            throw new ArgumentException("FlatRate cannot be negative.", nameof(flatRate));
        }
        FlatRate = flatRate;
    }

    private void SetLoading(decimal? loading)
    {
        if (loading.HasValue && loading.Value < 0)
        {
            throw new ArgumentException("Loading cannot be negative.", nameof(loading));
        }
        Loading = loading;
    }

    private void SetPremiumRate(decimal premiumRate)
    {
        if (premiumRate < 0)
        {
            throw new ArgumentException("PremiumRate cannot be negative.", nameof(premiumRate));
        }
        PremiumRate = premiumRate;
    }

    private void SetPremiumTotal(decimal premiumTotal)
    {
        if (premiumTotal < 0)
        {
            throw new ArgumentException("PremiumTotal cannot be negative.", nameof(premiumTotal));
        }
        PremiumTotal = premiumTotal;
    }

    private void SetPremium(decimal premium)
    {
        if (premium < 0)
        {
            throw new ArgumentException("Premium cannot be negative.", nameof(premium));
        }
        Premium = premium;
    }

    private void SetVat(decimal vat)
    {
        if (vat < 0)
        {
            throw new ArgumentException("Vat cannot be negative.", nameof(vat));
        }
        Vat = vat;
    }

    private void SetDiscount(decimal? discount)
    {
        if (discount.HasValue && discount.Value < 0)
        {
            throw new ArgumentException("Discount cannot be negative.", nameof(discount));
        }
        Discount = discount;
    }

    private void SetDiscountRate(decimal? discountRate)
    {
        if (discountRate.HasValue && discountRate.Value < 0)
        {
            throw new ArgumentException("DiscountRate cannot be negative.", nameof(discountRate));
        }
        DiscountRate = discountRate;
    }

    // Public update methods
    public virtual void UpdateCoverageParentId(Guid? coverageParentId)
    {
        SetCoverageParentId(coverageParentId);
    }

    public virtual void UpdateInsurerCoverageCode(string? insurerCoverageCode)
    {
        SetInsurerCoverageCode(insurerCoverageCode);
    }

    public virtual void UpdateTableRateLineId(Guid? tableRateLineId)
    {
        SetTableRateLineId(tableRateLineId);
    }

    public virtual void UpdateAmountLiability(decimal? amountLiability)
    {
        SetAmountLiability(amountLiability);
    }

    public virtual void UpdateQuantity(decimal quantity)
    {
        SetQuantity(quantity);
    }

    public virtual void UpdateNetRate(decimal? netRate)
    {
        SetNetRate(netRate);
    }

    public virtual void UpdateBaseRate(decimal? baseRate)
    {
        SetBaseRate(baseRate);
    }

    public virtual void UpdateFlatRate(decimal? flatRate)
    {
        SetFlatRate(flatRate);
    }

    public virtual void UpdateLoading(decimal? loading)
    {
        SetLoading(loading);
    }

    public virtual void UpdatePremiumRate(decimal premiumRate)
    {
        SetPremiumRate(premiumRate);
    }

    public virtual void UpdatePremiumTotal(decimal premiumTotal)
    {
        SetPremiumTotal(premiumTotal);
    }

    public virtual void UpdatePremium(decimal premium)
    {
        SetPremium(premium);
    }

    public virtual void UpdateVat(decimal vat)
    {
        SetVat(vat);
    }

    public virtual void UpdateDiscount(decimal? discount)
    {
        SetDiscount(discount);
    }

    public virtual void UpdateDiscountRate(decimal? discountRate)
    {
        SetDiscountRate(discountRate);
    }
}
