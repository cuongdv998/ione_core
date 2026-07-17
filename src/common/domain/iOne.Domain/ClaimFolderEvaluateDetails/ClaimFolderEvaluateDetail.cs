using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimFolderEvaluateDetails;

[Table("claim_folde_revaluate_detail")]
public class ClaimFolderEvaluateDetail : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimFolderEvaluateId { get; private set; }

    [Required]
    public virtual Guid EvaluateItemId { get; private set; }

    [Required]
    [MaxLength(1)]
    public virtual string Result { get; private set; } = "Y";

    [MaxLength(250)]
    public virtual string? Description { get; private set; }

    protected ClaimFolderEvaluateDetail()
    {
    }

    public ClaimFolderEvaluateDetail(Guid id, Guid claimFolderEvaluateId, Guid evaluateItemId, string result, string? description = null)
        : base(id)
    {
        UpdateClaimFolderEvaluateId(claimFolderEvaluateId);
        UpdateEvaluateItemId(evaluateItemId);
        UpdateResult(result);
        UpdateDescription(description);
    }

    public virtual void UpdateClaimFolderEvaluateId(Guid claimFolderEvaluateId)
    {
        if (claimFolderEvaluateId == Guid.Empty)
        {
            throw new ArgumentException("ClaimFolderEvaluateId cannot be empty.", nameof(claimFolderEvaluateId));
        }

        ClaimFolderEvaluateId = claimFolderEvaluateId;
    }

    public virtual void UpdateEvaluateItemId(Guid evaluateItemId)
    {
        if (evaluateItemId == Guid.Empty)
        {
            throw new ArgumentException("EvaluateItemId cannot be empty.", nameof(evaluateItemId));
        }

        EvaluateItemId = evaluateItemId;
    }

    public virtual void UpdateResult(string result)
    {
        var normalized = (result ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized != "Y" && normalized != "N")
        {
            throw new ArgumentException("Result must be Y or N.", nameof(result));
        }

        Result = normalized;
    }

    public virtual void UpdateDescription(string? description)
    {
        var normalized = description?.Trim();
        if (!string.IsNullOrEmpty(normalized) && normalized.Length > 250)
        {
            throw new ArgumentException("Description cannot exceed 250 characters.", nameof(description));
        }

        Description = string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
