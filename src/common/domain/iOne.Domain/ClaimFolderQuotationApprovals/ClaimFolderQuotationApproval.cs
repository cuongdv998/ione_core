using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimFolders;
using iOne.Claims;
using iOne.ResPartners;
using iOne.ResReasons;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimFolderQuotationApprovals;

[Table("claim_folder_quotation_approval")]
public class ClaimFolderQuotationApproval : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimId { get; private set; }

    public virtual Guid? ClaimFolderId { get; private set; }

    public virtual Guid? PartnerId { get; private set; }

    [Required]
    public virtual ClaimFolderQuotationApprovalStatus Status { get; private set; }

    [Required]
    public virtual DateTime SubmittedDate { get; private set; }

    [Required]
    public virtual Guid SubmitterId { get; private set; }

    public virtual DateTime? ApprovedDate { get; private set; }

    public virtual Guid? ApproverId { get; private set; }

    public virtual decimal? ClaimAmount { get; private set; }

    public virtual decimal? DiscountAmount { get; private set; }

    public virtual decimal? DepreciationAmount { get; private set; }

    public virtual decimal? ExpenseAmount { get; private set; }

    public virtual decimal? AssessmentAmount { get; private set; }

    public virtual decimal? LossPreventionAmount { get; private set; }

    public virtual decimal? RescueAmount { get; private set; }

    public virtual decimal? OtherAmount { get; private set; }

    public virtual string? Data { get; private set; }

    [MaxLength(250)]
    public virtual string? Description { get; private set; }

    public virtual Guid? ReasonId { get; private set; }

    [MaxLength(250)]
    public virtual string? ReasonDescription { get; private set; }

    public virtual Claim? Claim { get; set; }

    public virtual ClaimFolder? ClaimFolder { get; set; }

    public virtual ResPartner? Partner { get; set; }

    public virtual ResReason? Reason { get; set; }

    protected ClaimFolderQuotationApproval()
    {
    }

    public ClaimFolderQuotationApproval(
        Guid id,
        Guid claimId,
        Guid submitterId,
        DateTime submittedDate,
        ClaimFolderQuotationApprovalStatus status,
        Guid? claimFolderId = null,
        Guid? partnerId = null,
        decimal? claimAmount = null,
        decimal? discountAmount = null,
        decimal? depreciationAmount = null,
        decimal? expenseAmount = null,
        decimal? assessmentAmount = null,
        decimal? lossPreventionAmount = null,
        decimal? rescueAmount = null,
        decimal? otherAmount = null,
        string? data = null,
        string? description = null)
        : base(id)
    {
        SetClaimId(claimId);
        SetSubmitterId(submitterId);
        SetSubmittedDate(submittedDate);
        SetStatus(status);
        ClaimFolderId = claimFolderId;
        PartnerId = partnerId;
        SetOptionalAmount(nameof(ClaimAmount), claimAmount, v => ClaimAmount = v);
        SetOptionalAmount(nameof(DiscountAmount), discountAmount, v => DiscountAmount = v);
        SetOptionalAmount(nameof(DepreciationAmount), depreciationAmount, v => DepreciationAmount = v);
        SetOptionalAmount(nameof(ExpenseAmount), expenseAmount, v => ExpenseAmount = v);
        SetOptionalAmount(nameof(AssessmentAmount), assessmentAmount, v => AssessmentAmount = v);
        SetOptionalAmount(nameof(LossPreventionAmount), lossPreventionAmount, v => LossPreventionAmount = v);
        SetOptionalAmount(nameof(RescueAmount), rescueAmount, v => RescueAmount = v);
        SetOptionalAmount(nameof(OtherAmount), otherAmount, v => OtherAmount = v);
        SetData(data);
        SetDescription(description);
    }

    public virtual void UpdateClaimFolderId(Guid? claimFolderId)
    {
        ClaimFolderId = claimFolderId;
    }

    public virtual void UpdatePartnerId(Guid? partnerId)
    {
        PartnerId = partnerId;
    }

    public virtual void UpdateApproverId(Guid? approverId)
    {
        ApproverId = approverId;
    }

    public virtual void UpdateSubmission(Guid submitterId, DateTime submittedDate)
    {
        SetSubmitterId(submitterId);
        SetSubmittedDate(submittedDate);
    }

    public virtual void UpdateStatus(ClaimFolderQuotationApprovalStatus status)
    {
        SetStatus(status);
    }

    public virtual void SetApproval(Guid approverId, DateTime approvedDate)
    {
        if (approverId == Guid.Empty)
        {
            throw new ArgumentException("ApproverId cannot be empty.", nameof(approverId));
        }

        ApproverId = approverId;
        ApprovedDate = approvedDate;
        SetStatus(ClaimFolderQuotationApprovalStatus.Approved);
    }

    public virtual void SetRejection(Guid reasonId, string? reasonDescription)
    {
        if (reasonId == Guid.Empty)
        {
            throw new ArgumentException("ReasonId cannot be empty when rejecting.", nameof(reasonId));
        }

        ReasonId = reasonId;
        SetReasonDescription(reasonDescription);
        SetStatus(ClaimFolderQuotationApprovalStatus.Rejected);
    }

    public virtual void UpdateReason(Guid? reasonId, string? reasonDescription)
    {
        ReasonId = reasonId;
        SetReasonDescription(reasonDescription);
    }

    public virtual void UpdateData(string? data)
    {
        SetData(data);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateAmounts(
        decimal? claimAmount = null,
        decimal? discountAmount = null,
        decimal? depreciationAmount = null,
        decimal? expenseAmount = null,
        decimal? assessmentAmount = null,
        decimal? lossPreventionAmount = null,
        decimal? rescueAmount = null,
        decimal? otherAmount = null)
    {
        if (claimAmount.HasValue)
        {
            SetOptionalAmount(nameof(ClaimAmount), claimAmount, v => ClaimAmount = v);
        }

        if (discountAmount.HasValue)
        {
            SetOptionalAmount(nameof(DiscountAmount), discountAmount, v => DiscountAmount = v);
        }

        if (depreciationAmount.HasValue)
        {
            SetOptionalAmount(nameof(DepreciationAmount), depreciationAmount, v => DepreciationAmount = v);
        }

        if (expenseAmount.HasValue)
        {
            SetOptionalAmount(nameof(ExpenseAmount), expenseAmount, v => ExpenseAmount = v);
        }

        if (assessmentAmount.HasValue)
        {
            SetOptionalAmount(nameof(AssessmentAmount), assessmentAmount, v => AssessmentAmount = v);
        }

        if (lossPreventionAmount.HasValue)
        {
            SetOptionalAmount(nameof(LossPreventionAmount), lossPreventionAmount, v => LossPreventionAmount = v);
        }

        if (rescueAmount.HasValue)
        {
            SetOptionalAmount(nameof(RescueAmount), rescueAmount, v => RescueAmount = v);
        }

        if (otherAmount.HasValue)
        {
            SetOptionalAmount(nameof(OtherAmount), otherAmount, v => OtherAmount = v);
        }
    }

    private void SetClaimId(Guid claimId)
    {
        if (claimId == Guid.Empty)
        {
            throw new ArgumentException("ClaimId cannot be empty.", nameof(claimId));
        }

        ClaimId = claimId;
    }

    private void SetSubmitterId(Guid submitterId)
    {
        if (submitterId == Guid.Empty)
        {
            throw new ArgumentException("SubmitterId cannot be empty.", nameof(submitterId));
        }

        SubmitterId = submitterId;
    }

    private void SetSubmittedDate(DateTime submittedDate)
    {
        SubmittedDate = submittedDate;
    }

    private void SetStatus(ClaimFolderQuotationApprovalStatus status)
    {
        Status = status;
    }

    private static void SetOptionalAmount(string name, decimal? value, Action<decimal?> setter)
    {
        if (value.HasValue && value.Value < 0)
        {
            throw new ArgumentException($"{name} cannot be negative.", name);
        }

        setter(value);
    }

    private void SetData(string? data)
    {
        Data = data;
    }

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 250)
        {
            throw new ArgumentException("Description cannot exceed 250 characters.", nameof(description));
        }

        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    private void SetReasonDescription(string? reasonDescription)
    {
        if (!string.IsNullOrWhiteSpace(reasonDescription) && reasonDescription.Length > 250)
        {
            throw new ArgumentException("ReasonDescription cannot exceed 250 characters.", nameof(reasonDescription));
        }

        ReasonDescription = string.IsNullOrWhiteSpace(reasonDescription) ? null : reasonDescription.Trim();
    }
}
