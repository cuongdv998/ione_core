using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimStages;
using iOne.Claims;
using iOne.ClaimFolders;
using iOne.ResClaimStages;
using iOne.ResPartners;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimStages;

[Table("claim_stage")]
public class ClaimStage : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimId { get; private set; }

    public virtual Guid? ClaimFolderId { get; private set; }

    public virtual Guid? PartnerId { get; private set; }

    [Required]
    public virtual Guid StageId { get; private set; }

    [Required]
    public virtual ClaimStageStatus Status { get; private set; }

    public virtual DateTime? StartDate { get; private set; }

    [Required]
    public virtual DateTime DueDate { get; private set; }

    public virtual DateTime? EndDate { get; private set; }

    // Navigation
    public virtual Claim? Claim { get; set; }
    public virtual ClaimFolder? ClaimFolder { get; set; }
    public virtual ResClaimStage? Stage { get; set; }
    public virtual ResPartner? Partner { get; set; }

    protected ClaimStage()
    {
    }

    public ClaimStage(
        Guid id,
        Guid claimId,
        Guid stageId,
        DateTime dueDate,
        ClaimStageStatus status,
        Guid? claimFolderId = null,
        Guid? partnerId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
        : base(id)
    {
        SetClaimId(claimId);
        SetStageId(stageId);
        ClaimFolderId = claimFolderId;
        PartnerId = partnerId;
        StartDate = startDate;
        DueDate = dueDate;
        EndDate = endDate;
        SetStatus(status);
    }

    private void SetClaimId(Guid claimId)
    {
        if (claimId == Guid.Empty)
        {
            throw new ArgumentException("ClaimId cannot be empty.", nameof(claimId));
        }
        ClaimId = claimId;
    }

    private void SetStageId(Guid stageId)
    {
        if (stageId == Guid.Empty)
        {
            throw new ArgumentException("StageId cannot be empty.", nameof(stageId));
        }
        StageId = stageId;
    }

    private void SetStatus(ClaimStageStatus status)
    {
        Status = status;
    }

    public virtual void UpdateStatus(ClaimStageStatus status) => SetStatus(status);
}

