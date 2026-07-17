using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ProProductTypes;
using iOne.ResPartners;
using iOne.ProTableRates;
using iOne.ProLineOfBusinesses;
using iOne.ProProductCategorys;
using iOne.ResCurrencies;
using iOne.ProProductPlanDefinitions;
using iOne.ProProductDistributions;
using iOne.ResDocuments;

namespace iOne.ProProducts;

[Table("pro_product")]
public class ProProduct : FullAuditedAggregateRoot<Guid>
{
    // Foreign Keys
    public virtual Guid? ProductTypeId { get; private set; }
    public virtual Guid? PartnerId { get; private set; }
    public virtual Guid? TableRateId { get; private set; }
    public virtual Guid? RootProductId { get; private set; }
    
    [Required]
    [MaxLength(1)]
    public virtual string IsRootProduct { get; private set; } = "Y";
    
    [Required]
    public virtual Guid LobId { get; private set; }
    
    public virtual Guid? ProductCategoryId { get; private set; }
    
    public virtual Guid? CurrencyId { get; private set; }
    
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;
    
    [MaxLength(50)]
    public virtual string? InsurerProductCode { get; private set; }
    
    [Required]
    [MaxLength(50)]
    public virtual string ShortName { get; private set; } = null!;
    
    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;
    
    [MaxLength(500)]
    public virtual string? Description { get; private set; }
    
    [MaxLength(250)]
    public virtual string? InternalNote { get; private set; }
    
    [Required]
    [MaxLength(15)]
    public virtual string RateType { get; private set; } = "table_rate";
    
    [Required]
    public virtual ProProductStatus Status { get; private set; }
    
    [Required]
    public virtual int SeqNumber { get; private set; } = 1;
    
    [Required]
    public virtual DateTime EffectDate { get; private set; }
    
    public virtual DateTime? ExpireDate { get; private set; }
    
    public virtual Guid? PlanDefinitionId { get; private set; }
    
    [MaxLength(1)]
    public virtual string? IsPlan { get; private set; }
    
    public virtual Guid? ImageDocumentId { get; private set; }

    public virtual Guid? CertificateTemplateDocumentId { get; private set; }

    // Navigation properties for many-to-one relationships
    public virtual ProProductType? ProductType { get; set; }
    public virtual ResPartner? Partner { get; set; }
    public virtual ProLineOfBusiness? Lob { get; set; }
    public virtual ProProductCategory? ProductCategory { get; set; }
    public virtual ResCurrency? Currency { get; set; }
    
    // Navigation property for self-joining (parent)
    public virtual ProProduct? RootProduct { get; set; }
    
    // Navigation property for image document
    public virtual ResDocument? ImageDocument { get; set; }

    // Navigation property for product certificate template (e.g. Word docx)
    public virtual ResDocument? CertificateTemplateDocument { get; set; }
    
    // Navigation property for self-joining (children)
    public virtual ICollection<ProProduct> ChildProducts { get; private set; } = new List<ProProduct>();
    
    // Navigation property for many-to-many relationship with ProTableRate
    public virtual ICollection<ProProductTableRate> ProductTableRates { get; private set; } = new List<ProProductTableRate>();
    
    // Navigation property for many-to-many relationship with ProAttribute
    public virtual ICollection<ProProductAttribute> ProductAttributes { get; private set; } = new List<ProProductAttribute>();
    
    // Navigation property for many-to-many relationship with ProCoverage
    public virtual ICollection<ProProductCoverage> ProductCoverages { get; private set; } = new List<ProProductCoverage>();
    
    // Navigation property for one-to-many relationship with ProProductPlanDefinition
    public virtual ICollection<ProProductPlanDefinition> ProductPlanDefinitions { get; private set; } = new List<ProProductPlanDefinition>();
    
    // Navigation property for one-to-many relationship with ProProductDistribution
    public virtual ICollection<ProProductDistribution> ProductDistributions { get; private set; } = new List<ProProductDistribution>();

    protected ProProduct()
    {
        // For ORM
    }

    public ProProduct(
        Guid id,
        Guid lobId,
        string code,
        string shortName,
        string name,
        ProProductStatus status,
        DateTime effectDate,
        Guid? productTypeId = null,
        Guid? partnerId = null,
        Guid? tableRateId = null,
        Guid? rootProductId = null,
        string isRootProduct = "Y",
        Guid? productCategoryId = null,
        Guid? currencyId = null,
        string? insurerProductCode = null,
        string? description = null,
        string? internalNote = null,
        string rateType = "table_rate",
        int seqNumber = 1,
        DateTime? expireDate = null,
        Guid? planDefinitionId = null,
        string? isPlan = null,
        Guid? imageDocumentId = null,
        Guid? certificateTemplateDocumentId = null)
        : base(id)
    {
        SetLobId(lobId);
        SetCode(code);
        SetShortName(shortName);
        SetName(name);
        SetStatus(status);
        SetEffectDate(effectDate);
        SetProductTypeId(productTypeId);
        SetPartnerId(partnerId);
        SetTableRateId(tableRateId);
        SetRootProductId(rootProductId);
        SetIsRootProduct(isRootProduct);
        SetProductCategoryId(productCategoryId);
        SetCurrencyId(currencyId);
        SetInsurerProductCode(insurerProductCode);
        SetDescription(description);
        SetInternalNote(internalNote);
        SetRateType(rateType);
        SetSeqNumber(seqNumber);
        SetExpireDate(expireDate);
        SetPlanDefinitionId(planDefinitionId);
        SetIsPlan(isPlan);
        SetImageDocumentId(imageDocumentId);
        SetCertificateTemplateDocumentId(certificateTemplateDocumentId);
    }

    private void SetLobId(Guid lobId)
    {
        if (lobId == Guid.Empty)
        {
            throw new ArgumentException("LobId cannot be empty.", nameof(lobId));
        }

        LobId = lobId;
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        if (code.Length > 50)
        {
            throw new ArgumentException("Code cannot exceed 50 characters.", nameof(code));
        }

        // Convert to uppercase and validate format: only A-Z, 0-9, and underscore
        var upperCode = code.ToUpperInvariant();
        if (!Regex.IsMatch(upperCode, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("Code can only contain letters (A-Z), numbers (0-9), and underscore (_).", nameof(code));
        }

        Code = upperCode;
    }

    private void SetShortName(string shortName)
    {
        if (string.IsNullOrWhiteSpace(shortName))
        {
            throw new ArgumentException("ShortName cannot be null or empty.", nameof(shortName));
        }

        if (shortName.Length > 50)
        {
            throw new ArgumentException("ShortName cannot exceed 50 characters.", nameof(shortName));
        }

        ShortName = shortName;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (name.Length > 250)
        {
            throw new ArgumentException("Name cannot exceed 250 characters.", nameof(name));
        }

        Name = name;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetInternalNote(string? internalNote)
    {
        if (internalNote != null && internalNote.Length > 250)
        {
            throw new ArgumentException("InternalNote cannot exceed 250 characters.", nameof(internalNote));
        }

        InternalNote = internalNote;
    }

    private void SetInsurerProductCode(string? insurerProductCode)
    {
        if (insurerProductCode != null && insurerProductCode.Length > 50)
        {
            throw new ArgumentException("InsurerProductCode cannot exceed 50 characters.", nameof(insurerProductCode));
        }

        InsurerProductCode = insurerProductCode;
    }

    private void SetRateType(string rateType)
    {
        if (string.IsNullOrWhiteSpace(rateType))
        {
            throw new ArgumentException("RateType cannot be null or empty.", nameof(rateType));
        }

        if (rateType.Length > 15)
        {
            throw new ArgumentException("RateType cannot exceed 15 characters.", nameof(rateType));
        }

        RateType = rateType;
    }

    private void SetStatus(ProProductStatus status)
    {
        Status = status;
    }

    private void SetSeqNumber(int seqNumber)
    {
        if (seqNumber < 0)
        {
            throw new ArgumentException("SeqNumber cannot be negative.", nameof(seqNumber));
        }

        SeqNumber = seqNumber;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime? expireDate)
    {
        ExpireDate = expireDate;
    }

    private void SetProductTypeId(Guid? productTypeId)
    {
        ProductTypeId = productTypeId;
    }

    private void SetPartnerId(Guid? partnerId)
    {
        PartnerId = partnerId;
    }

    private void SetTableRateId(Guid? tableRateId)
    {
        TableRateId = tableRateId;
    }

    private void SetRootProductId(Guid? rootProductId)
    {
        RootProductId = rootProductId;
    }

    private void SetIsRootProduct(string isRootProduct)
    {
        if (string.IsNullOrWhiteSpace(isRootProduct))
        {
            throw new ArgumentException("IsRootProduct cannot be null or empty.", nameof(isRootProduct));
        }

        if (isRootProduct.Length > 1)
        {
            throw new ArgumentException("IsRootProduct cannot exceed 1 character.", nameof(isRootProduct));
        }

        IsRootProduct = isRootProduct;
    }

    private void SetProductCategoryId(Guid? productCategoryId)
    {
        ProductCategoryId = productCategoryId;
    }

    private void SetCurrencyId(Guid? currencyId)
    {
        CurrencyId = currencyId;
    }

    private void SetPlanDefinitionId(Guid? planDefinitionId)
    {
        PlanDefinitionId = planDefinitionId;
    }

    private void SetIsPlan(string? isPlan)
    {
        if (isPlan != null && isPlan.Length > 1)
        {
            throw new ArgumentException("IsPlan cannot exceed 1 character.", nameof(isPlan));
        }

        IsPlan = isPlan;
    }

    private void SetImageDocumentId(Guid? imageDocumentId)
    {
        ImageDocumentId = imageDocumentId;
    }

    private void SetCertificateTemplateDocumentId(Guid? certificateTemplateDocumentId)
    {
        CertificateTemplateDocumentId = certificateTemplateDocumentId;
    }

    // Update methods
    public virtual void UpdateProductTypeId(Guid? productTypeId)
    {
        SetProductTypeId(productTypeId);
    }

    public virtual void UpdatePartnerId(Guid? partnerId)
    {
        SetPartnerId(partnerId);
    }

    public virtual void UpdateTableRateId(Guid? tableRateId)
    {
        SetTableRateId(tableRateId);
    }

    public virtual void UpdateRootProductId(Guid? rootProductId)
    {
        SetRootProductId(rootProductId);
    }

    public virtual void UpdateIsRootProduct(string isRootProduct)
    {
        SetIsRootProduct(isRootProduct);
    }

    public virtual void UpdateLobId(Guid lobId)
    {
        SetLobId(lobId);
    }

    public virtual void UpdateProductCategoryId(Guid? productCategoryId)
    {
        SetProductCategoryId(productCategoryId);
    }

    public virtual void UpdateCurrencyId(Guid? currencyId)
    {
        SetCurrencyId(currencyId);
    }

    public virtual void UpdateShortName(string shortName)
    {
        SetShortName(shortName);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateInternalNote(string? internalNote)
    {
        SetInternalNote(internalNote);
    }

    public virtual void UpdateInsurerProductCode(string? insurerProductCode)
    {
        SetInsurerProductCode(insurerProductCode);
    }

    public virtual void UpdateRateType(string rateType)
    {
        SetRateType(rateType);
    }

    public virtual void UpdateStatus(ProProductStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateSeqNumber(int seqNumber)
    {
        SetSeqNumber(seqNumber);
    }

    public virtual void UpdateEffectDate(DateTime effectDate)
    {
        SetEffectDate(effectDate);
    }

    public virtual void UpdateExpireDate(DateTime? expireDate)
    {
        SetExpireDate(expireDate);
    }

    public virtual void UpdatePlanDefinitionId(Guid? planDefinitionId)
    {
        SetPlanDefinitionId(planDefinitionId);
    }

    public virtual void UpdateIsPlan(string? isPlan)
    {
        SetIsPlan(isPlan);
    }

    public virtual void UpdateImageDocumentId(Guid? imageDocumentId)
    {
        SetImageDocumentId(imageDocumentId);
    }

    public virtual void UpdateCertificateTemplateDocumentId(Guid? certificateTemplateDocumentId)
    {
        SetCertificateTemplateDocumentId(certificateTemplateDocumentId);
    }
}
