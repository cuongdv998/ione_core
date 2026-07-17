using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimFolderEvaluates;

[Table("claim_folde_revaluate")]
public class ClaimFolderEvaluate : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ClaimFolderId { get; private set; }

    [Required]
    public virtual Guid EmployeeId { get; private set; }

    [Required]
    [MaxLength(1)]
    public virtual string Result { get; private set; } = "Y";

    [MaxLength(250)]
    public virtual string? Description { get; private set; }

    protected ClaimFolderEvaluate()
    {
    }

    public ClaimFolderEvaluate(Guid id, Guid claimFolderId, Guid employeeId, string result, string? description = null)
        : base(id)
    {
        UpdateClaimFolderId(claimFolderId);
        UpdateEmployeeId(employeeId);
        UpdateResult(result);
        UpdateDescription(description);
    }

    public virtual void UpdateClaimFolderId(Guid claimFolderId)
    {
        if (claimFolderId == Guid.Empty)
        {
            throw new ArgumentException("ClaimFolderId cannot be empty.", nameof(claimFolderId));
        }

        ClaimFolderId = claimFolderId;
    }

    public virtual void UpdateEmployeeId(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
        {
            throw new ArgumentException("EmployeeId cannot be empty.", nameof(employeeId));
        }

        EmployeeId = employeeId;
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
