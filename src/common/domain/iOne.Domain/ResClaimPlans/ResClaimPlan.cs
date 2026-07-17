using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResClaimPlans;

[Table("res_claim_plan")]
public class ResClaimPlan : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    /// <summary>in = thu tiền về, out = thanh toán ra.</summary>
    [MaxLength(15)]
    public virtual string? PaymentType { get; private set; }

    /// <summary>Y/N - có phát sinh chi phí hay không.</summary>
    [MaxLength(1)]
    public virtual string? IsExpenses { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ResClaimPlanStatus Status { get; private set; }

    protected ResClaimPlan()
    {
    }

    public ResClaimPlan(
        Guid id,
        string code,
        string name,
        ResClaimPlanStatus status,
        string? paymentType = null,
        string? isExpenses = null,
        string? description = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetPaymentType(paymentType);
        SetIsExpenses(isExpenses);
        SetDescription(description);
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

    private void SetPaymentType(string? paymentType)
    {
        if (!string.IsNullOrWhiteSpace(paymentType) && paymentType.Length > 15)
        {
            throw new ArgumentException("PaymentType cannot exceed 15 characters.", nameof(paymentType));
        }
        PaymentType = paymentType;
    }

    private void SetIsExpenses(string? isExpenses)
    {
        if (string.IsNullOrWhiteSpace(isExpenses))
        {
            IsExpenses = null;
            return;
        }
        if (isExpenses.Length != 1 || (isExpenses != "Y" && isExpenses != "N"))
        {
            throw new ArgumentException("IsExpenses must be 'Y' or 'N'.", nameof(isExpenses));
        }
        IsExpenses = isExpenses;
    }

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }
        Description = description;
    }

    private void SetStatus(ResClaimPlanStatus status)
    {
        Status = status;
    }

    public virtual void UpdateName(string name) => SetName(name);
    public virtual void UpdateDescription(string? description) => SetDescription(description);
    public virtual void UpdateStatus(ResClaimPlanStatus status) => SetStatus(status);
}

