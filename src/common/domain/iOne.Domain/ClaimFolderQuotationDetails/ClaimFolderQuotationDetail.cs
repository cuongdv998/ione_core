using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimFolderQuotationDetails;

[Table("claim_folder_quotation_detail")]
public class ClaimFolderQuotationDetail : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? QuotationId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string ItemName { get; private set; } = null!;

    [Required]
    public virtual decimal Quantity { get; private set; }

    [MaxLength(50)]
    public virtual string? Plan { get; private set; }

    [Required]
    public virtual decimal Price { get; private set; }

    [Required]
    public virtual decimal AmountTotal { get; private set; }

    protected ClaimFolderQuotationDetail()
    {
    }

    public ClaimFolderQuotationDetail(
        Guid id,
        string itemName,
        decimal quantity,
        decimal price,
        decimal amountTotal,
        Guid? quotationId = null,
        string? plan = null)
        : base(id)
    {
        SetItemName(itemName);
        SetQuantity(quantity);
        SetPrice(price);
        SetAmountTotal(amountTotal);
        QuotationId = quotationId;
        SetPlan(plan);
    }

    private void SetItemName(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            throw new ArgumentException("ItemName cannot be null or empty.", nameof(itemName));
        }
        if (itemName.Length > 50)
        {
            throw new ArgumentException("ItemName cannot exceed 50 characters.", nameof(itemName));
        }
        ItemName = itemName;
    }

    private void SetQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        }
        Quantity = quantity;
    }

    private void SetPlan(string? plan)
    {
        if (!string.IsNullOrWhiteSpace(plan) && plan.Length > 50)
        {
            throw new ArgumentException("Plan cannot exceed 50 characters.", nameof(plan));
        }
        Plan = plan;
    }

    private void SetPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException("Price cannot be negative.", nameof(price));
        }
        Price = price;
    }

    private void SetAmountTotal(decimal amountTotal)
    {
        if (amountTotal < 0)
        {
            throw new ArgumentException("AmountTotal cannot be negative.", nameof(amountTotal));
        }
        AmountTotal = amountTotal;
    }
}

