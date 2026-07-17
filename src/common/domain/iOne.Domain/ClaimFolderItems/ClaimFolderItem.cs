using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimFolders;
using iOne.ClaimFolderIncidentObjects;
using iOne.ClaimFolderExposures;
using iOne.ResDamageLevels;
using iOne.ResObjectTypeItems;
using iOne.ResRisks;
using iOne.ResUoms;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimFolderItems;

[Table("claim_folder_item")]
public class ClaimFolderItem : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimFolderId { get; private set; }

    [Required]
    public virtual Guid ClaimFolderIncidentObjectId { get; private set; }

    [Required]
    public virtual Guid ClaimFolderExposureId { get; private set; }

    public virtual Guid? ItemId { get; private set; }

    public virtual Guid? RiskId { get; private set; }

    public virtual Guid? ClaimPlanId { get; private set; }

    public virtual Guid? DemageLevelId { get; private set; }

    [Required]
    public virtual decimal Quantity { get; private set; }

    public virtual Guid? UomId { get; private set; }

    public virtual DateTime? IssueDate { get; private set; }

    public virtual decimal? LossValue { get; private set; }

    /// <summary>Y/N - có thu hồi hay không.</summary>
    [Required]
    [MaxLength(1)]
    public virtual string IsRecovery { get; private set; } = "N";

    /// <summary>Y/N - có được bảo hiểm hay không.</summary>
    [MaxLength(1)]
    public virtual string? IsCover { get; private set; }

    public virtual double? DepreciationPercent { get; private set; }

    /// <summary>Y/N - thay thế chính hãng hay không.</summary>
    [MaxLength(1)]
    public virtual string? IsGenuien { get; private set; }

    [MaxLength(15)]
    public virtual string? Position { get; private set; }

    public virtual double? CoveragePercent { get; private set; }

    [MaxLength(250)]
    public virtual string? Description { get; private set; }

    // Navigation
    public virtual ClaimFolder? ClaimFolder { get; set; }
    public virtual ClaimFolderIncidentObject? ClaimFolderIncidentObject { get; set; }
    public virtual ClaimFolderExposure? ClaimFolderExposure { get; set; }
    public virtual ResObjectTypeItem? Item { get; set; }
    public virtual ResRisk? Risk { get; set; }
    public virtual ResDamageLevel? DemageLevel { get; set; }
    public virtual ResUom? Uom { get; set; }

    protected ClaimFolderItem()
    {
    }

    public ClaimFolderItem(
        Guid id,
        Guid claimFolderId,
        Guid claimFolderIncidentObjectId,
        Guid claimFolderExposureId,
        Guid? itemId,
        decimal quantity)
        : base(id)
    {
        SetClaimFolderId(claimFolderId);
        SetClaimFolderIncidentObjectId(claimFolderIncidentObjectId);
        SetClaimFolderExposureId(claimFolderExposureId);
        SetItemId(itemId);
        SetQuantity(quantity);
        SetIsRecovery("N");
    }

    private void SetClaimFolderId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ClaimFolderId cannot be empty.", nameof(id));
        }
        ClaimFolderId = id;
    }

    private void SetClaimFolderIncidentObjectId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ClaimFolderIncidentObjectId cannot be empty.", nameof(id));
        }
        ClaimFolderIncidentObjectId = id;
    }

    private void SetClaimFolderExposureId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ClaimFolderExposureId cannot be empty.", nameof(id));
        }
        ClaimFolderExposureId = id;
    }

    private void SetItemId(Guid? id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ItemId cannot be empty.", nameof(id));
        }
        ItemId = id;
    }

    private void SetQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        }
        Quantity = quantity;
    }

    private void SetRiskId(Guid? value)
    {
        RiskId = value;
    }

    private void SetClaimPlanId(Guid? value)
    {
        ClaimPlanId = value;
    }

    private void SetIssueDate(DateTime? value)
    {
        IssueDate = value;
    }

    private void SetIsRecovery(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 1 || (value != "Y" && value != "N"))
        {
            throw new ArgumentException("IsRecovery must be 'Y' or 'N'.", nameof(value));
        }
        IsRecovery = value;
    }

    private void SetIsGenuien(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            IsGenuien = null;
            return;
        }

        if (value.Length != 1 || (value != "Y" && value != "N"))
        {
            throw new ArgumentException("IsGenuien must be 'Y' or 'N'.", nameof(value));
        }

        IsGenuien = value;
    }

    private void SetDescription(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 250)
        {
            throw new ArgumentException("Description cannot exceed 250 characters.", nameof(value));
        }

        Description = value;
    }

    private void SetCoveragePercent(double? value)
    {
        CoveragePercent = value;
    }

    public virtual void UpdateRiskId(Guid? value) => SetRiskId(value);

    public virtual void UpdateClaimPlanId(Guid? value) => SetClaimPlanId(value);

    public virtual void UpdateClaimFolderIncidentObjectId(Guid value) => SetClaimFolderIncidentObjectId(value);

    public virtual void UpdateClaimFolderExposureId(Guid value) => SetClaimFolderExposureId(value);

    public virtual void UpdateItemId(Guid? value) => SetItemId(value);

    public virtual void UpdateQuantity(decimal value) => SetQuantity(value);

    public virtual void UpdateIssueDate(DateTime? value) => SetIssueDate(value);

    public virtual void UpdateIsRecovery(string value) => SetIsRecovery(value);

    public virtual void UpdateIsGenuien(string? value) => SetIsGenuien(value);

    public virtual void UpdateDescription(string? value) => SetDescription(value);

    public virtual void UpdateCoveragePercent(double? value) => SetCoveragePercent(value);
}
