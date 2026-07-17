using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ResFeeItems;
using iOne.ResPaymentMethods;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Policies;

[Table("policy_amount")]
public class PolicyAmount : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    protected PolicyAmount()
    {
        // For ORM
    }

    public PolicyAmount(
        Guid id,
        Guid policyId,
        Guid policyVersionId,
        Guid feeItemId,
        DateTime issueDate,
        decimal amountTotal,
        decimal amount,
        decimal vat,
        string paymentStatus = "new",
        Guid? paymentMethodId = null,
        DateTime? paymentDate = null)
        : base(id)
    {
        SetPolicyId(policyId);
        SetPolicyVersionId(policyVersionId);
        SetFeeItemId(feeItemId);
        SetIssueDate(issueDate);
        SetAmountTotal(amountTotal);
        SetAmount(amount);
        SetVat(vat);
        SetPaymentStatus(paymentStatus);
        SetPaymentMethodId(paymentMethodId);
        SetPaymentDate(paymentDate);
    }

    [Required] public virtual Guid PolicyId { get; private set; }

    [Required] public virtual Guid PolicyVersionId { get; private set; }

    [Required] public virtual Guid FeeItemId { get; private set; }

    [Required] [Column(TypeName = "DATE")] public virtual DateTime IssueDate { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal AmountTotal { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal Amount { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal Vat { get; private set; }

    [Required] [MaxLength(15)] public virtual string PaymentStatus { get; private set; } = "new";

    public virtual Guid? PaymentMethodId { get; private set; }

    [Column(TypeName = "DATE")] public virtual DateTime? PaymentDate { get; private set; }

    // Navigation Properties
    public virtual Policy? Policy { get; set; }
    public virtual PolicyVersion? PolicyVersion { get; set; }
    public virtual ResFeeItem? FeeItem { get; set; }
    public virtual ResPaymentMethod? PaymentMethod { get; set; }

    private void SetPolicyId(Guid policyId)
    {
        if (policyId == Guid.Empty)
        {
            throw new ArgumentException("PolicyId cannot be empty.", nameof(policyId));
        }

        PolicyId = policyId;
    }

    private void SetPolicyVersionId(Guid policyVersionId)
    {
        if (policyVersionId == Guid.Empty)
        {
            throw new ArgumentException("PolicyVersionId cannot be empty.", nameof(policyVersionId));
        }

        PolicyVersionId = policyVersionId;
    }

    private void SetFeeItemId(Guid feeItemId)
    {
        if (feeItemId == Guid.Empty)
        {
            throw new ArgumentException("FeeItemId cannot be empty.", nameof(feeItemId));
        }

        FeeItemId = feeItemId;
    }

    private void SetIssueDate(DateTime issueDate)
    {
        IssueDate = issueDate;
    }

    private void SetAmountTotal(decimal amountTotal)
    {
        // Cho phép âm (ví dụ dòng ENDORSEMENT_ADJUSTMENT_AMOUNT — giảm phí). Dòng POLICY_AMOUNT cấp đơn vẫn thường không âm.
        AmountTotal = amountTotal;
    }

    private void SetAmount(decimal amount)
    {
        // Amount can be negative for endorsement adjustments (giảm phí) or positive (tăng phí)
        Amount = amount;
    }

    private void SetVat(decimal vat)
    {
        if (vat < 0)
        {
            throw new ArgumentException("Vat cannot be negative.", nameof(vat));
        }

        Vat = vat;
    }

    private void SetPaymentStatus(string paymentStatus)
    {
        if (string.IsNullOrWhiteSpace(paymentStatus))
        {
            throw new ArgumentException("PaymentStatus cannot be null or empty.", nameof(paymentStatus));
        }

        if (paymentStatus.Length > 15)
        {
            throw new ArgumentException("PaymentStatus cannot exceed 15 characters.", nameof(paymentStatus));
        }

        PaymentStatus = paymentStatus.ToLowerInvariant();
    }

    private void SetPaymentMethodId(Guid? paymentMethodId)
    {
        if (paymentMethodId.HasValue && paymentMethodId.Value == Guid.Empty)
        {
            throw new ArgumentException("PaymentMethodId cannot be empty.", nameof(paymentMethodId));
        }

        PaymentMethodId = paymentMethodId;
    }

    private void SetPaymentDate(DateTime? paymentDate)
    {
        PaymentDate = paymentDate;
    }

    // Update methods
    public virtual void UpdateIssueDate(DateTime issueDate)
    {
        SetIssueDate(issueDate);
    }

    public virtual void UpdateAmountTotal(decimal amountTotal)
    {
        SetAmountTotal(amountTotal);
    }

    public virtual void UpdateAmount(decimal amount)
    {
        SetAmount(amount);
    }

    public virtual void UpdateVat(decimal vat)
    {
        SetVat(vat);
    }

    public virtual void UpdatePaymentStatus(string paymentStatus)
    {
        SetPaymentStatus(paymentStatus);
    }

    public virtual void UpdatePaymentMethodId(Guid? paymentMethodId)
    {
        SetPaymentMethodId(paymentMethodId);
    }

    public virtual void UpdatePaymentDate(DateTime? paymentDate)
    {
        SetPaymentDate(paymentDate);
    }
}