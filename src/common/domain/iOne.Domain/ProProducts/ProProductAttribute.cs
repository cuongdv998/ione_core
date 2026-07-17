using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ProAttributes;

namespace iOne.ProProducts;

[Table("pro_product_attribute")]
public class ProProductAttribute : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ProductId { get; private set; }
    
    [Required]
    public virtual Guid AttributeId { get; private set; }
    
    [Required]
    [MaxLength(1)]
    public virtual string IsRequired { get; private set; } = "N";
    
    [MaxLength(15)]
    public virtual string? Status { get; private set; }

    // Navigation properties
    public virtual ProProduct? Product { get; set; }
    public virtual ProAttribute? Attribute { get; set; }

    protected ProProductAttribute()
    {
        // For ORM
    }

    public ProProductAttribute(
        Guid id,
        Guid productId,
        Guid attributeId,
        string isRequired = "N",
        string? status = null)
        : base(id)
    {
        SetProductId(productId);
        SetAttributeId(attributeId);
        SetIsRequired(isRequired);
        SetStatus(status);
    }

    private void SetProductId(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        }

        ProductId = productId;
    }

    private void SetAttributeId(Guid attributeId)
    {
        if (attributeId == Guid.Empty)
        {
            throw new ArgumentException("AttributeId cannot be empty.", nameof(attributeId));
        }

        AttributeId = attributeId;
    }

    private void SetIsRequired(string isRequired)
    {
        if (string.IsNullOrWhiteSpace(isRequired))
        {
            throw new ArgumentException("IsRequired cannot be null or empty.", nameof(isRequired));
        }

        if (isRequired.Length > 1)
        {
            throw new ArgumentException("IsRequired cannot exceed 1 character.", nameof(isRequired));
        }

        IsRequired = isRequired;
    }

    private void SetStatus(string? status)
    {
        if (status != null && status.Length > 15)
        {
            throw new ArgumentException("Status cannot exceed 15 characters.", nameof(status));
        }

        Status = status;
    }

    // Update methods
    public virtual void UpdateIsRequired(string isRequired)
    {
        SetIsRequired(isRequired);
    }

    public virtual void UpdateStatus(string? status)
    {
        SetStatus(status);
    }
}
