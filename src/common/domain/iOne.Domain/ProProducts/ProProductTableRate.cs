using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ProTableRates;

namespace iOne.ProProducts;

[Table("pro_product_table_rate")]
public class ProProductTableRate : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ProductId { get; private set; }
    
    [Required]
    public virtual Guid TableRateId { get; private set; }
    
    [Required]
    public virtual DateTime EffectDate { get; private set; }
    
    public virtual DateTime? ExpireDate { get; private set; }

    // Navigation properties
    public virtual ProProduct? Product { get; set; }
    public virtual ProTableRate? TableRate { get; set; }

    protected ProProductTableRate()
    {
        // For ORM
    }

    public ProProductTableRate(
        Guid id,
        Guid productId,
        Guid tableRateId,
        DateTime effectDate,
        DateTime? expireDate = null)
        : base(id)
    {
        SetProductId(productId);
        SetTableRateId(tableRateId);
        SetEffectDate(effectDate);
        SetExpireDate(expireDate);
    }

    private void SetProductId(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        }

        ProductId = productId;
    }

    private void SetTableRateId(Guid tableRateId)
    {
        if (tableRateId == Guid.Empty)
        {
            throw new ArgumentException("TableRateId cannot be empty.", nameof(tableRateId));
        }

        TableRateId = tableRateId;
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
    public virtual void UpdateEffectDate(DateTime effectDate)
    {
        SetEffectDate(effectDate);
    }

    public virtual void UpdateExpireDate(DateTime? expireDate)
    {
        SetExpireDate(expireDate);
    }
}
