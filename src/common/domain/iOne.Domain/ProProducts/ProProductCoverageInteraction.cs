using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ProProducts;

[Table("pro_product_coverage_interaction")]
public class ProProductCoverageInteraction : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ProductCoverageId { get; private set; }
    
    [Required]
    [MaxLength(15)]
    public virtual ProProductCoverageInteractionType InteractionType { get; private set; }
    
    [Required]
    public virtual Guid InteractionCoverageId { get; private set; }
    
    [Required]
    public virtual DateTime EffectDate { get; private set; }
    
    public virtual DateTime? ExpireDate { get; private set; }

    // Navigation properties
    public virtual ProProductCoverage? ProductCoverage { get; set; }
    public virtual ProProductCoverage? InteractionCoverage { get; set; }

    protected ProProductCoverageInteraction()
    {
        // For ORM
    }

    public ProProductCoverageInteraction(
        Guid id,
        Guid productCoverageId,
        ProProductCoverageInteractionType interactionType,
        Guid interactionCoverageId,
        DateTime effectDate,
        DateTime? expireDate = null)
        : base(id)
    {
        SetProductCoverageId(productCoverageId);
        SetInteractionType(interactionType);
        SetInteractionCoverageId(interactionCoverageId);
        SetEffectDate(effectDate);
        SetExpireDate(expireDate);
    }

    private void SetProductCoverageId(Guid productCoverageId)
    {
        if (productCoverageId == Guid.Empty)
        {
            throw new ArgumentException("ProductCoverageId cannot be empty.", nameof(productCoverageId));
        }

        ProductCoverageId = productCoverageId;
    }

    private void SetInteractionType(ProProductCoverageInteractionType interactionType)
    {
        InteractionType = interactionType;
    }

    private void SetInteractionCoverageId(Guid interactionCoverageId)
    {
        if (interactionCoverageId == Guid.Empty)
        {
            throw new ArgumentException("InteractionCoverageId cannot be empty.", nameof(interactionCoverageId));
        }

        InteractionCoverageId = interactionCoverageId;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime? expireDate)
    {
        ExpireDate = expireDate;
    }

    // Update methods
    public virtual void UpdateInteractionType(ProProductCoverageInteractionType interactionType)
    {
        SetInteractionType(interactionType);
    }

    public virtual void UpdateInteractionCoverageId(Guid interactionCoverageId)
    {
        SetInteractionCoverageId(interactionCoverageId);
    }

    public virtual void UpdateEffectDate(DateTime effectDate)
    {
        SetEffectDate(effectDate);
    }

    public virtual void UpdateExpireDate(DateTime? expireDate)
    {
        SetExpireDate(expireDate);
    }
}
