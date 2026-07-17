using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimFolderExposureEstimates;
using iOne.ClaimFolders;
using iOne.ResFeeItems;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimFolderExposureEstimates;

[Table("claim_folder_exposure_estimate")]
public class ClaimFolderExposureEstimate : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? ClaimFolderId { get; private set; }

    [Required]
    public virtual Guid FeeItemId { get; private set; }

    [Required]
    public virtual decimal Amount { get; private set; }

    [Required]
    public virtual ClaimFolderExposureEstimateStatus Status { get; private set; }

    // Navigation
    public virtual ClaimFolder? ClaimFolder { get; set; }
    public virtual ResFeeItem? FeeItem { get; set; }

    protected ClaimFolderExposureEstimate()
    {
    }

    public ClaimFolderExposureEstimate(
        Guid id,
        Guid feeItemId,
        decimal amount,
        ClaimFolderExposureEstimateStatus status,
        Guid? claimFolderId = null)
        : base(id)
    {
        ClaimFolderId = claimFolderId;
        SetFeeItemId(feeItemId);
        SetAmount(amount);
        SetStatus(status);
    }

    private void SetFeeItemId(Guid feeItemId)
    {
        if (feeItemId == Guid.Empty)
        {
            throw new ArgumentException("FeeItemId cannot be empty.", nameof(feeItemId));
        }
        FeeItemId = feeItemId;
    }

    private void SetAmount(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }
        Amount = amount;
    }

    private void SetStatus(ClaimFolderExposureEstimateStatus status)
    {
        Status = status;
    }
}

