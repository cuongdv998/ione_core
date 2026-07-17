using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ResClaimStages;
using iOne.ResClaimTypes;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResClaimStages;

[Table("res_claim_stage")]
public class ResClaimStage : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimTypeId { get; private set; }

    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResClaimStageStatus Status { get; private set; }

    public virtual Guid? SurveyPlanId { get; private set; }

    // Navigation
    public virtual ResClaimType? ClaimType { get; set; }

    protected ResClaimStage()
    {
    }

    public ResClaimStage(
        Guid id,
        Guid claimTypeId,
        string code,
        string name,
        ResClaimStageStatus status,
        Guid? surveyPlanId = null)
        : base(id)
    {
        SetClaimTypeId(claimTypeId);
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SurveyPlanId = surveyPlanId;
    }

    private void SetClaimTypeId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ClaimTypeId cannot be empty.", nameof(id));
        }
        ClaimTypeId = id;
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }
        if (code.Length > 25)
        {
            throw new ArgumentException("Code cannot exceed 25 characters.", nameof(code));
        }
        Code = code.ToUpperInvariant();
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

    private void SetStatus(ResClaimStageStatus status)
    {
        Status = status;
    }
}

