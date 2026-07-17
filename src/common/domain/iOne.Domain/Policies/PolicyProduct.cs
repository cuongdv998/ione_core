using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ProProducts;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Policies;

[Table("policy_product")]
public class PolicyProduct : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    [Required]
    public virtual Guid PolicyVersionId { get; private set; }

    [Required]
    public virtual Guid ProductId { get; private set; }

    [MaxLength(50)]
    public virtual string? InsurerProductCode { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? AmountLiability { get; private set; }

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

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? Markup { get; private set; }

    // Navigation Properties
    public virtual PolicyVersion PolicyVersion { get; set; } = null!;
    public virtual ProProduct? Product { get; set; }
    public virtual ICollection<PolicyCoverage> PolicyCoverages { get; set; } = new List<PolicyCoverage>();

    protected PolicyProduct()
    {
        // For ORM
    }

    public PolicyProduct(
        Guid id,
        Guid policyVersionId,
        Guid productId,
        decimal premiumTotal,
        decimal premium,
        decimal vat,
        string? insurerProductCode = null,
        decimal? amountLiability = null,
        decimal? discount = null,
        decimal? discountRate = null,
        decimal? markup = null)
        : base(id)
    {
        SetPolicyVersionId(policyVersionId);
        SetProductId(productId);
        SetInsurerProductCode(insurerProductCode);
        SetAmountLiability(amountLiability);
        SetPremiumTotal(premiumTotal);
        SetPremium(premium);
        SetVat(vat);
        SetDiscount(discount);
        SetDiscountRate(discountRate);
        SetMarkup(markup);
    }

    // Private setters with validation
    private void SetPolicyVersionId(Guid policyVersionId)
    {
        if (policyVersionId == Guid.Empty)
        {
            throw new ArgumentException("PolicyVersionId cannot be empty.", nameof(policyVersionId));
        }
        PolicyVersionId = policyVersionId;
    }

    private void SetProductId(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        }
        ProductId = productId;
    }

    private void SetInsurerProductCode(string? insurerProductCode)
    {
        if (insurerProductCode != null && insurerProductCode.Length > 50)
        {
            throw new ArgumentException("InsurerProductCode cannot exceed 50 characters.", nameof(insurerProductCode));
        }
        InsurerProductCode = insurerProductCode;
    }

    private void SetAmountLiability(decimal? amountLiability)
    {
        if (amountLiability.HasValue && amountLiability.Value < 0)
        {
            throw new ArgumentException("AmountLiability cannot be negative.", nameof(amountLiability));
        }
        AmountLiability = amountLiability;
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

    private void SetMarkup(decimal? markup)
    {
        if (markup.HasValue && markup.Value < 0)
        {
            throw new ArgumentException("Markup cannot be negative.", nameof(markup));
        }
        Markup = markup;
    }

    // Public update methods
    public virtual void UpdateInsurerProductCode(string? insurerProductCode)
    {
        SetInsurerProductCode(insurerProductCode);
    }

    public virtual void UpdateAmountLiability(decimal? amountLiability)
    {
        SetAmountLiability(amountLiability);
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

    public virtual void UpdateMarkup(decimal? markup)
    {
        SetMarkup(markup);
    }
}
