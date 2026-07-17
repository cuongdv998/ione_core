using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Policies;

[Table("policy_version")]
public class PolicyVersion : FullAuditedAggregateRoot<Guid>, IEntity<Guid>
{
    [Required]
    [Column(TypeName = "NUMERIC(2)")]
    public virtual decimal Version { get; private set; }

    [Required]
    public virtual Guid PolicyId { get; private set; }

    [Required]
    [MaxLength(15)]
    public virtual string Type { get; private set; } = null!;

    [Required]
    [MaxLength(15)]
    public virtual string Status { get; private set; } = null!;

    [Required]
    public virtual DateTime EffectDate { get; private set; }

    [Required]
    public virtual DateTime ExpireDate { get; private set; }

    [Required]
    public virtual DateTime OrgEffectDate { get; private set; }

    [Required]
    public virtual DateTime OrgExpireDate { get; private set; }

    [MaxLength(500)]
    public virtual string? InternalNote { get; private set; }

    [MaxLength(500)]
    public virtual string? CustomerNote { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal PremiumTotal { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal Premium { get; private set; }

    [Required]
    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal Vat { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? Discount { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? DiscountRate { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? Markup { get; private set; }

    public virtual DateTime? ApprovalDate { get; private set; }

    public virtual Guid? ApproverId { get; private set; }

    [MaxLength(15)]
    public virtual string? ApprovalStatus { get; private set; }

    [MaxLength(15)]
    public virtual string? InsurerIntegrationStatus { get; private set; }

    public virtual string? InsurerIntegrationDescription { get; private set; }

    public virtual Guid? EndorsementType { get; private set; }

    public virtual Guid? EndorsementReasonId { get; private set; }

    [MaxLength(300)]
    public virtual string? EndorsementDescription { get; private set; }

    public virtual PolicyTerminationStatus? TerminationStatus { get; private set; }

    [Column(TypeName = "NUMERIC(15,3)")]
    public virtual decimal? RefundAmount { get; private set; }

    // Navigation Properties
    public virtual Policy Policy { get; set; } = null!;
    public virtual ICollection<PolicyCertificate> PolicyCertificates { get; set; } = new List<PolicyCertificate>();
    public virtual ICollection<PolicyRiskObject> PolicyRiskObjects { get; set; } = new List<PolicyRiskObject>();
    public virtual ICollection<PolicyProduct> PolicyProducts { get; set; } = new List<PolicyProduct>();

    protected PolicyVersion()
    {
        // For ORM
    }

    public PolicyVersion(
        Guid id,
        decimal version,
        Guid policyId,
        string type,
        string status,
        DateTime effectDate,
        DateTime expireDate,
        DateTime orgEffectDate,
        DateTime orgExpireDate,
        decimal premiumTotal,
        decimal premium,
        decimal vat,
        string? internalNote = null,
        string? customerNote = null,
        decimal? discount = null,
        decimal? discountRate = null,
        decimal? markup = null,
        DateTime? approvalDate = null,
        Guid? approverId = null,
        string? approvalStatus = null,
        string? insurerIntegrationStatus = null,
        string? insurerIntegrationDescription = null,
        Guid? endorsementType = null,
        Guid? endorsementReasonId = null,
        string? endorsementDescription = null,
        PolicyTerminationStatus? terminationStatus = null,
        decimal? refundAmount = null)
        : base(id)
    {
        SetVersion(version);
        SetPolicyId(policyId);
        SetType(type);
        SetStatus(status);
        SetEffectDate(effectDate);
        SetExpireDate(expireDate);
        SetOrgEffectDate(orgEffectDate);
        SetOrgExpireDate(orgExpireDate);
        SetPremiumTotal(premiumTotal);
        SetPremium(premium);
        SetVat(vat);
        SetInternalNote(internalNote);
        SetCustomerNote(customerNote);
        SetDiscount(discount);
        SetDiscountRate(discountRate);
        SetMarkup(markup);
        SetApprovalDate(approvalDate);
        SetApproverId(approverId);
        SetApprovalStatus(approvalStatus);
        SetInsurerIntegrationStatus(insurerIntegrationStatus);
        SetInsurerIntegrationDescription(insurerIntegrationDescription);
        SetEndorsementType(endorsementType);
        SetEndorsementReasonId(endorsementReasonId);
        SetEndorsementDescription(endorsementDescription);
        SetTerminationStatus(terminationStatus);
        SetRefundAmount(refundAmount);
    }

    // Private setters with validation
    private void SetVersion(decimal version)
    {
        if (version < 0)
        {
            throw new ArgumentException("Version cannot be negative.", nameof(version));
        }
        Version = version;
    }

    private void SetPolicyId(Guid policyId)
    {
        if (policyId == Guid.Empty)
        {
            throw new ArgumentException("PolicyId cannot be empty.", nameof(policyId));
        }
        PolicyId = policyId;
    }

    private void SetType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Type cannot be null or empty.", nameof(type));
        }
        if (type.Length > 15)
        {
            throw new ArgumentException("Type cannot exceed 15 characters.", nameof(type));
        }
        Type = type;
    }

    private void SetStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status cannot be null or empty.", nameof(status));
        }
        if (status.Length > 15)
        {
            throw new ArgumentException("Status cannot exceed 15 characters.", nameof(status));
        }
        Status = status;
    }

    private void SetEffectDate(DateTime effectDate)
    {
        EffectDate = effectDate;
    }

    private void SetExpireDate(DateTime expireDate)
    {
        if (expireDate < EffectDate)
        {
            throw new ArgumentException("ExpireDate must be after EffectDate.", nameof(expireDate));
        }
        ExpireDate = expireDate;
    }

    private void SetOrgEffectDate(DateTime orgEffectDate)
    {
        OrgEffectDate = orgEffectDate;
    }

    private void SetOrgExpireDate(DateTime orgExpireDate)
    {
        if (orgExpireDate < OrgEffectDate)
        {
            throw new ArgumentException("OrgExpireDate must be after OrgEffectDate.", nameof(orgExpireDate));
        }
        OrgExpireDate = orgExpireDate;
    }

    private void SetInternalNote(string? internalNote)
    {
        if (internalNote != null && internalNote.Length > 500)
        {
            throw new ArgumentException("InternalNote cannot exceed 500 characters.", nameof(internalNote));
        }
        InternalNote = internalNote;
    }

    private void SetCustomerNote(string? customerNote)
    {
        if (customerNote != null && customerNote.Length > 500)
        {
            throw new ArgumentException("CustomerNote cannot exceed 500 characters.", nameof(customerNote));
        }
        CustomerNote = customerNote;
    }

    private void SetPremiumTotal(decimal premiumTotal)
    {
        if (premiumTotal < 0)
        {
            throw new ArgumentException("PremiumTotal cannot be negative.", nameof(premiumTotal));
        }
        PremiumTotal = premiumTotal;
    }

    private void SetPremium(decimal premium)
    {
        if (premium < 0)
        {
            throw new ArgumentException("Premium cannot be negative.", nameof(premium));
        }
        Premium = premium;
    }

    private void SetVat(decimal vat)
    {
        if (vat < 0)
        {
            throw new ArgumentException("Vat cannot be negative.", nameof(vat));
        }
        Vat = vat;
    }

    private void SetDiscount(decimal? discount)
    {
        if (discount.HasValue && discount.Value < 0)
        {
            throw new ArgumentException("Discount cannot be negative.", nameof(discount));
        }
        Discount = discount;
    }

    private void SetDiscountRate(decimal? discountRate)
    {
        if (discountRate.HasValue && discountRate.Value < 0)
        {
            throw new ArgumentException("DiscountRate cannot be negative.", nameof(discountRate));
        }
        DiscountRate = discountRate;
    }

    private void SetMarkup(decimal? markup)
    {
        if (markup.HasValue && markup.Value < 0)
        {
            throw new ArgumentException("Markup cannot be negative.", nameof(markup));
        }
        Markup = markup;
    }

    private void SetApprovalDate(DateTime? approvalDate)
    {
        ApprovalDate = approvalDate;
    }

    private void SetApproverId(Guid? approverId)
    {
        ApproverId = approverId;
    }

    private void SetApprovalStatus(string? approvalStatus)
    {
        if (approvalStatus != null && approvalStatus.Length > 15)
        {
            throw new ArgumentException("ApprovalStatus cannot exceed 15 characters.", nameof(approvalStatus));
        }
        ApprovalStatus = approvalStatus;
    }

    private void SetInsurerIntegrationStatus(string? insurerIntegrationStatus)
    {
        if (insurerIntegrationStatus != null && insurerIntegrationStatus.Length > 15)
        {
            throw new ArgumentException("InsurerIntegrationStatus cannot exceed 15 characters.", nameof(insurerIntegrationStatus));
        }
        InsurerIntegrationStatus = insurerIntegrationStatus;
    }

    private void SetInsurerIntegrationDescription(string? insurerIntegrationDescription)
    {
        InsurerIntegrationDescription = insurerIntegrationDescription;
    }

    private void SetEndorsementType(Guid? endorsementType)
    {
        EndorsementType = endorsementType;
    }

    private void SetEndorsementReasonId(Guid? endorsementReasonId)
    {
        EndorsementReasonId = endorsementReasonId;
    }

    private void SetEndorsementDescription(string? endorsementDescription)
    {
        if (endorsementDescription != null && endorsementDescription.Length > 300)
        {
            throw new ArgumentException("EndorsementDescription cannot exceed 300 characters.", nameof(endorsementDescription));
        }
        EndorsementDescription = endorsementDescription;
    }

    private void SetTerminationStatus(PolicyTerminationStatus? terminationStatus)
    {
        TerminationStatus = terminationStatus;
    }

    private void SetRefundAmount(decimal? refundAmount)
    {
        if (refundAmount.HasValue && refundAmount.Value > 0)
        {
            throw new ArgumentException("RefundAmount must be negative or zero when set.", nameof(refundAmount));
        }
        RefundAmount = refundAmount;
    }

    // Public update methods
    public virtual void UpdateVersion(decimal version)
    {
        SetVersion(version);
    }

    public virtual void UpdateStatus(string status)
    {
        SetStatus(status);
    }

    public virtual void UpdateEffectDate(DateTime effectDate)
    {
        SetEffectDate(effectDate);
    }

    public virtual void UpdateExpireDate(DateTime expireDate)
    {
        SetExpireDate(expireDate);
    }

    public virtual void UpdateOrgEffectDate(DateTime orgEffectDate)
    {
        SetOrgEffectDate(orgEffectDate);
    }

    public virtual void UpdateOrgExpireDate(DateTime orgExpireDate)
    {
        SetOrgExpireDate(orgExpireDate);
    }

    public virtual void UpdateInternalNote(string? internalNote)
    {
        SetInternalNote(internalNote);
    }

    public virtual void UpdateCustomerNote(string? customerNote)
    {
        SetCustomerNote(customerNote);
    }

    public virtual void UpdatePremiumTotal(decimal premiumTotal)
    {
        SetPremiumTotal(premiumTotal);
    }

    public virtual void UpdatePremium(decimal premium)
    {
        SetPremium(premium);
    }

    public virtual void UpdateVat(decimal vat)
    {
        SetVat(vat);
    }

    public virtual void UpdateDiscount(decimal? discount)
    {
        SetDiscount(discount);
    }

    public virtual void UpdateDiscountRate(decimal? discountRate)
    {
        SetDiscountRate(discountRate);
    }

    public virtual void UpdateMarkup(decimal? markup)
    {
        SetMarkup(markup);
    }

    public virtual void UpdateApprovalDate(DateTime? approvalDate)
    {
        SetApprovalDate(approvalDate);
    }

    public virtual void UpdateApproverId(Guid? approverId)
    {
        SetApproverId(approverId);
    }

    public virtual void UpdateApprovalStatus(string? approvalStatus)
    {
        SetApprovalStatus(approvalStatus);
    }

    public virtual void UpdateInsurerIntegrationStatus(string? insurerIntegrationStatus)
    {
        SetInsurerIntegrationStatus(insurerIntegrationStatus);
    }

    public virtual void UpdateInsurerIntegrationDescription(string? insurerIntegrationDescription)
    {
        SetInsurerIntegrationDescription(insurerIntegrationDescription);
    }

    public virtual void UpdateEndorsementType(Guid? endorsementType)
    {
        SetEndorsementType(endorsementType);
    }

    public virtual void UpdateEndorsementReasonId(Guid? endorsementReasonId)
    {
        SetEndorsementReasonId(endorsementReasonId);
    }

    public virtual void UpdateEndorsementDescription(string? endorsementDescription)
    {
        SetEndorsementDescription(endorsementDescription);
    }

    public virtual void UpdateTerminationStatus(PolicyTerminationStatus? terminationStatus)
    {
        SetTerminationStatus(terminationStatus);
    }

    public virtual void UpdateRefundAmount(decimal? refundAmount)
    {
        SetRefundAmount(refundAmount);
    }
}
