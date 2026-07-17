using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ProCoverages;
using iOne.ResUoms;
using iOne.ResTaxes;

namespace iOne.ProProducts;

[Table("pro_product_coverage")]
public class ProProductCoverage : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ProductId { get; private set; }
    
    [Required]
    public virtual Guid CoverageId { get; private set; }
    
    public virtual Guid? ParentId { get; private set; }
    
    [MaxLength(50)]
    public virtual string? InsurerCoverageCode { get; private set; }
    
    public virtual Guid? UomId { get; private set; }
    
    [Required]
    public virtual ProProductCoverageAvailabilityType AvailabilityType { get; private set; }
    
    [Required]
    public virtual DateTime EffectDate { get; private set; }
    
    public virtual DateTime? ExpireDate { get; private set; }
    
    [Required]
    public virtual int SeqNumber { get; private set; } = 1;
    
    [Required]
    public virtual Guid TaxId { get; private set; }
    
    [MaxLength(1)]
    public virtual string? EnableQuantity { get; private set; }

    // Navigation properties
    public virtual ProProduct? Product { get; set; }
    public virtual ProCoverage? Coverage { get; set; }
    public virtual ProProductCoverage? Parent { get; set; }
    public virtual ICollection<ProProductCoverage> Children { get; private set; } = new List<ProProductCoverage>();
    public virtual ResUom? Uom { get; set; }
    public virtual ResTax? Tax { get; set; }
    
    // Navigation property for one-to-many relationship with ProProductCoverageInteraction
    public virtual ICollection<ProProductCoverageInteraction> Interactions { get; private set; } = new List<ProProductCoverageInteraction>();
    
    // Navigation property for one-to-many relationship with ProProductCoverageLevel
    public virtual ICollection<ProProductCoverageLevel> CoverageLevels { get; private set; } = new List<ProProductCoverageLevel>();

    protected ProProductCoverage()
    {
        // For ORM
    }

    public ProProductCoverage(
        Guid id,
        Guid productId,
        Guid coverageId,
        ProProductCoverageAvailabilityType availabilityType,
        DateTime effectDate,
        Guid taxId,
        int seqNumber = 1,
        Guid? parentId = null,
        string? insurerCoverageCode = null,
        Guid? uomId = null,
        DateTime? expireDate = null,
        string? enableQuantity = null)
        : base(id)
    {
        SetProductId(productId);
        SetCoverageId(coverageId);
        SetAvailabilityType(availabilityType);
        SetEffectDate(effectDate);
        SetTaxId(taxId);
        SetSeqNumber(seqNumber);
        SetParentId(parentId);
        SetInsurerCoverageCode(insurerCoverageCode);
        SetUomId(uomId);
        SetExpireDate(expireDate);
        SetEnableQuantity(enableQuantity);
    }

    private void SetProductId(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        }

        ProductId = productId;
    }

    private void SetCoverageId(Guid coverageId)
    {
        if (coverageId == Guid.Empty)
        {
            throw new ArgumentException("CoverageId cannot be empty.", nameof(coverageId));
        }

        CoverageId = coverageId;
    }

    private void SetParentId(Guid? parentId)
    {
        if (parentId.HasValue && parentId.Value == Guid.Empty)
        {
            throw new ArgumentException("ParentId cannot be empty if provided.", nameof(parentId));
        }

        ParentId = parentId;
    }

    private void SetInsurerCoverageCode(string? insurerCoverageCode)
    {
        if (insurerCoverageCode != null && insurerCoverageCode.Length > 50)
        {
            throw new ArgumentException("InsurerCoverageCode cannot exceed 50 characters.", nameof(insurerCoverageCode));
        }

        InsurerCoverageCode = insurerCoverageCode;
    }

    private void SetUomId(Guid? uomId)
    {
        if (uomId.HasValue && uomId.Value == Guid.Empty)
        {
            throw new ArgumentException("UomId cannot be empty if provided.", nameof(uomId));
        }

        UomId = uomId;
    }

    private void SetAvailabilityType(ProProductCoverageAvailabilityType availabilityType)
    {
        AvailabilityType = availabilityType;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime? expireDate)
    {
        ExpireDate = expireDate;
    }

    private void SetSeqNumber(int seqNumber)
    {
        if (seqNumber < 0)
        {
            throw new ArgumentException("SeqNumber cannot be negative.", nameof(seqNumber));
        }

        SeqNumber = seqNumber;
    }

    private void SetTaxId(Guid taxId)
    {
        if (taxId == Guid.Empty)
        {
            throw new ArgumentException("TaxId cannot be empty.", nameof(taxId));
        }

        TaxId = taxId;
    }

    private void SetEnableQuantity(string? enableQuantity)
    {
        if (enableQuantity != null && enableQuantity.Length > 1)
        {
            throw new ArgumentException("EnableQuantity cannot exceed 1 character.", nameof(enableQuantity));
        }

        EnableQuantity = enableQuantity;
    }

    // Update methods
    public virtual void UpdateCoverageId(Guid coverageId)
    {
        SetCoverageId(coverageId);
    }

    public virtual void UpdateParentId(Guid? parentId)
    {
        SetParentId(parentId);
    }

    public virtual void UpdateInsurerCoverageCode(string? insurerCoverageCode)
    {
        SetInsurerCoverageCode(insurerCoverageCode);
    }

    public virtual void UpdateUomId(Guid? uomId)
    {
        SetUomId(uomId);
    }

    public virtual void UpdateAvailabilityType(ProProductCoverageAvailabilityType availabilityType)
    {
        SetAvailabilityType(availabilityType);
    }

    public virtual void UpdateEffectDate(DateTime effectDate)
    {
        SetEffectDate(effectDate);
    }

    public virtual void UpdateExpireDate(DateTime? expireDate)
    {
        SetExpireDate(expireDate);
    }

    public virtual void UpdateSeqNumber(int seqNumber)
    {
        SetSeqNumber(seqNumber);
    }

    public virtual void UpdateTaxId(Guid taxId)
    {
        SetTaxId(taxId);
    }

    public virtual void UpdateEnableQuantity(string? enableQuantity)
    {
        SetEnableQuantity(enableQuantity);
    }
}
