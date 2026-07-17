using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ProProducts;

namespace iOne.ProProductPlanDefinitions;

[Table("PROPRODUCTPLANDEFINITION")]
public class ProProductPlanDefinition : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ProductId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string PlanCode { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string PlanName { get; private set; } = null!;

    [Required]
    public virtual ProProductPlanDefinitionStatus Status { get; private set; }

    // Navigation property for many-to-one relationship with ProProduct
    public virtual ProProduct? Product { get; set; }

    protected ProProductPlanDefinition()
    {
        // For ORM
    }

    public ProProductPlanDefinition(
        Guid id,
        Guid productId,
        string planCode,
        string planName,
        ProProductPlanDefinitionStatus status)
        : base(id)
    {
        SetProductId(productId);
        SetPlanCode(planCode);
        SetPlanName(planName);
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

    private void SetPlanCode(string planCode)
    {
        if (string.IsNullOrWhiteSpace(planCode))
        {
            throw new ArgumentException("PlanCode cannot be null or empty.", nameof(planCode));
        }

        if (planCode.Length > 50)
        {
            throw new ArgumentException("PlanCode cannot exceed 50 characters.", nameof(planCode));
        }

        // Convert to uppercase and validate format: only A-Z, 0-9, and underscore
        var upperCode = planCode.ToUpperInvariant();
        if (!Regex.IsMatch(upperCode, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("PlanCode can only contain letters (A-Z), numbers (0-9), and underscore (_).", nameof(planCode));
        }

        PlanCode = upperCode;
    }

    private void SetPlanName(string planName)
    {
        if (string.IsNullOrWhiteSpace(planName))
        {
            throw new ArgumentException("PlanName cannot be null or empty.", nameof(planName));
        }

        if (planName.Length > 250)
        {
            throw new ArgumentException("PlanName cannot exceed 250 characters.", nameof(planName));
        }

        PlanName = planName;
    }

    private void SetStatus(ProProductPlanDefinitionStatus status)
    {
        Status = status;
    }

    // Update methods
    // ⚠️ QUAN TRỌNG: Không có method UpdatePlanCode() - PlanCode không được phép sửa
    public virtual void UpdatePlanName(string planName)
    {
        SetPlanName(planName);
    }

    public virtual void UpdateStatus(ProProductPlanDefinitionStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateProductId(Guid productId)
    {
        SetProductId(productId);
    }
}
