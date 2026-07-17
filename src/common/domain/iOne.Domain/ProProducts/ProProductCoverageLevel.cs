using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ProProducts;

[Table("pro_product_coverage_level")]
public class ProProductCoverageLevel : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? ProductCoverageId { get; private set; }
    
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;
    
    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;
    
    [Required]
    public virtual DateTime EffectDate { get; private set; }
    
    public virtual DateTime? ExpireDate { get; private set; }

    public virtual string? ConditionalScript { get; private set; }

    // Navigation properties
    public virtual ProProductCoverage? ProductCoverage { get; set; }
    
    // Navigation property for one-to-many relationship with ProProductCoverageLevelTerm
    public virtual ICollection<ProProductCoverageLevelTerm> Terms { get; private set; } = new List<ProProductCoverageLevelTerm>();

    protected ProProductCoverageLevel()
    {
        // For ORM
    }

    public ProProductCoverageLevel(
        Guid id,
        string code,
        string name,
        DateTime effectDate,
        Guid? productCoverageId = null,
        DateTime? expireDate = null,
        string? conditionalScript = null)
        : base(id)
    {
        SetProductCoverageId(productCoverageId);
        SetCode(code);
        SetName(name);
        SetEffectDate(effectDate);
        SetExpireDate(expireDate);
        SetConditionalScript(conditionalScript);
    }

    private void SetProductCoverageId(Guid? productCoverageId)
    {
        if (productCoverageId.HasValue && productCoverageId.Value == Guid.Empty)
        {
            throw new ArgumentException("ProductCoverageId cannot be empty if provided.", nameof(productCoverageId));
        }

        ProductCoverageId = productCoverageId;
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

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime? expireDate)
    {
        ExpireDate = expireDate;
    }

    private void SetConditionalScript(string? value)
    {
        ConditionalScript = value;
    }

    // Update methods
    public virtual void UpdateProductCoverageId(Guid? productCoverageId)
    {
        SetProductCoverageId(productCoverageId);
    }

    public virtual void UpdateCode(string code)
    {
        SetCode(code);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateEffectDate(DateTime effectDate)
    {
        SetEffectDate(effectDate);
    }

    public virtual void UpdateExpireDate(DateTime? expireDate)
    {
        SetExpireDate(expireDate);
    }

    public virtual void UpdateConditionalScript(string? value)
    {
        SetConditionalScript(value);
    }
}
