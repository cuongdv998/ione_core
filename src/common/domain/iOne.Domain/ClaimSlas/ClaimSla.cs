using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimSlas;
using iOne.ResClaimStages;
using iOne.ResPartners;
using iOne.ResTaskCategories;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimSlas;

[Table("claim_sla")]
public class ClaimSla : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid InsurerId { get; private set; }

    [Required]
    public virtual Guid ClaimStageId { get; private set; }

    public virtual Guid? TaskId { get; private set; }

    [Required]
    public virtual int SlaTime { get; private set; } // minutes

    [Required]
    public virtual DateTime EffectDate { get; private set; }

    public virtual DateTime? ExpireDate { get; private set; }

    [Required]
    public virtual ClaimSlaStatus Status { get; private set; }

    // Navigation
    public virtual ResPartner? Insurer { get; set; }
    public virtual ResClaimStage? ClaimStage { get; set; }
    public virtual ResTaskCategory? Task { get; set; }

    protected ClaimSla()
    {
    }

    public ClaimSla(
        Guid id,
        Guid insurerId,
        Guid claimStageId,
        int slaTime,
        DateTime effectDate,
        ClaimSlaStatus status,
        Guid? taskId = null,
        DateTime? expireDate = null)
        : base(id)
    {
        SetInsurerId(insurerId);
        SetClaimStageId(claimStageId);
        TaskId = taskId;
        SetSlaTime(slaTime);
        EffectDate = effectDate;
        ExpireDate = expireDate;
        SetStatus(status);
    }

    private void SetInsurerId(Guid insurerId)
    {
        if (insurerId == Guid.Empty)
        {
            throw new ArgumentException("InsurerId cannot be empty.", nameof(insurerId));
        }
        InsurerId = insurerId;
    }

    private void SetClaimStageId(Guid claimStageId)
    {
        if (claimStageId == Guid.Empty)
        {
            throw new ArgumentException("ClaimStageId cannot be empty.", nameof(claimStageId));
        }
        ClaimStageId = claimStageId;
    }

    private void SetSlaTime(int slaTime)
    {
        if (slaTime <= 0)
        {
            throw new ArgumentException("SlaTime must be positive.", nameof(slaTime));
        }
        SlaTime = slaTime;
    }

    private void SetStatus(ClaimSlaStatus status)
    {
        Status = status;
    }
}

