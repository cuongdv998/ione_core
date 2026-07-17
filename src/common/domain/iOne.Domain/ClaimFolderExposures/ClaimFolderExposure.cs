using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimFolders;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimFolderExposures;

[Table("claim_folder_exposure")]
public class ClaimFolderExposure : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimFolderId { get; private set; }

    [Required]
    public virtual Guid ClaimFolderIncidentObjectId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string CoverageCode { get; private set; } = null!;

    [MaxLength(50)]
    public virtual string? CoverageParentCode { get; private set; }

    [MaxLength(50)]
    public virtual string? InsurerCoverageCode { get; private set; }

    public virtual decimal? EstimateAmount { get; private set; }
    public virtual decimal? RepairDiscount { get; private set; }
    public virtual decimal? RepairDiscountPercent { get; private set; }
    public virtual Guid? MechanismIndemnifyReasonId { get; private set; }
    public virtual decimal? MechanismIndemnifyAmount { get; private set; }
    public virtual decimal? MechanismIndemnifyPercent { get; private set; }
    public virtual decimal? ClaimAmount { get; private set; }
    public virtual decimal? DeductibleAmount { get; private set; }
    public virtual decimal? DeductibleTaxId { get; private set; }
    public virtual decimal? LiabilityAmount { get; private set; }
    public virtual decimal? LimitLiabilityAmount { get; private set; }
    public virtual decimal? ExpenseAmount { get; private set; }
    public virtual decimal? DepreciationAmount { get; private set; }
    public virtual decimal? LossLimitAmount { get; private set; }

    // Navigation
    public virtual ClaimFolder? ClaimFolder { get; set; }

    protected ClaimFolderExposure()
    {
    }

    public ClaimFolderExposure(
        Guid id,
        Guid claimFolderId,
        Guid claimFolderIncidentObjectId,
        string coverageCode)
        : base(id)
    {
        SetClaimFolderId(claimFolderId);
        SetClaimFolderIncidentObjectId(claimFolderIncidentObjectId);
        SetCoverageCode(coverageCode);
    }

    private void SetClaimFolderId(Guid claimFolderId)
    {
        if (claimFolderId == Guid.Empty)
        {
            throw new ArgumentException("ClaimFolderId cannot be empty.", nameof(claimFolderId));
        }
        ClaimFolderId = claimFolderId;
    }

    private void SetClaimFolderIncidentObjectId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ClaimFolderIncidentObjectId cannot be empty.", nameof(id));
        }
        ClaimFolderIncidentObjectId = id;
    }

    private void SetCoverageCode(string coverageCode)
    {
        if (string.IsNullOrWhiteSpace(coverageCode))
        {
            throw new ArgumentException("CoverageCode cannot be null or empty.", nameof(coverageCode));
        }
        if (coverageCode.Length > 50)
        {
            throw new ArgumentException("CoverageCode cannot exceed 50 characters.", nameof(coverageCode));
        }
        CoverageCode = coverageCode;
    }

    private void SetCoverageParentCode(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 50)
        {
            throw new ArgumentException("CoverageParentCode cannot exceed 50 characters.", nameof(value));
        }

        CoverageParentCode = value;
    }

    private void SetInsurerCoverageCode(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 50)
        {
            throw new ArgumentException("InsurerCoverageCode cannot exceed 50 characters.", nameof(value));
        }

        InsurerCoverageCode = value;
    }

    public virtual void UpdateCoverageParentCode(string? value) => SetCoverageParentCode(value);

    public virtual void UpdateInsurerCoverageCode(string? value) => SetInsurerCoverageCode(value);

    public virtual void UpdateClaimFolderIncidentObjectId(Guid value) => SetClaimFolderIncidentObjectId(value);

    public virtual void UpdateCoverageCode(string value) => SetCoverageCode(value);
}
