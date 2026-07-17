using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ClaimFolders;
using iOne.ClaimSlas;
using iOne.Claims;
using iOne.HrEmployees;
using iOne.ResClaimTypes;
using iOne.ResPartners;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimFolders;

[Table("claim_folder")]
public class ClaimFolder : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string FolderNo { get; private set; } = null!;

    [MaxLength(250)]
    public virtual string? FolderName { get; private set; }

    public virtual Guid? InsurerId { get; private set; }

    [Required]
    public virtual Guid ClaimId { get; private set; }

    public virtual Guid? IncidentId { get; private set; }

    public virtual Guid? ProductId { get; private set; }

    [Required]
    public virtual Guid ClaimTypeId { get; private set; }

    [MaxLength(50)]
    public virtual string? PolicyNo { get; private set; }

    [MaxLength(50)]
    public virtual string? InsurerPolicyNo { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ClaimFolderStatus Status { get; private set; }

    /// <summary>Giai đoạn xử lý hiện tại (mã stage text).</summary>
    [Required]
    [MaxLength(50)]
    public virtual string Stage { get; private set; } = null!;

    [Required]
    public virtual DateTime OpenDate { get; private set; }

    public virtual Guid? OpenEmployeeId { get; private set; }

    public virtual DateTime? CloseDate { get; private set; }

    public virtual Guid? CloseEmployeeId { get; private set; }

    [MaxLength(500)]
    public virtual string? CloseNote { get; private set; }

    public virtual Guid? CancelReasonId { get; private set; }

    public virtual DateTime? CancelDate { get; private set; }

    public virtual Guid? CancelEmployeeId { get; private set; }

    [MaxLength(500)]
    public virtual string? CancelNote { get; private set; }

    public virtual ClaimFolderPriority? Priority { get; private set; }

    /// <summary>Y/N - có giám định hiện trường hay không.</summary>
    [Required]
    [MaxLength(1)]
    public virtual string HasAdjustLocation { get; private set; } = "N";

    // Navigation properties
    public virtual Claim? Claim { get; set; }
    public virtual ResClaimType? ClaimType { get; set; }
    public virtual ResPartner? Insurer { get; set; }
    public virtual HrEmployee? OpenEmployee { get; set; }
    public virtual HrEmployee? CloseEmployee { get; set; }

    protected ClaimFolder()
    {
        // For ORM
    }

    public ClaimFolder(
        Guid id,
        string folderNo,
        Guid claimId,
        Guid claimTypeId,
        DateTime openDate,
        ClaimFolderStatus status,
        string stage,
        Guid? insurerId = null,
        Guid? incidentId = null,
        Guid? productId = null,
        string? folderName = null,
        string? policyNo = null,
        string? insurerPolicyNo = null,
        string? description = null,
        Guid? openEmployeeId = null,
        ClaimFolderPriority? priority = null,
        string hasAdjustLocation = "N")
        : base(id)
    {
        SetFolderNo(folderNo);
        SetClaimId(claimId);
        SetClaimTypeId(claimTypeId);
        SetOpenDate(openDate);
        SetStatus(status);
        SetStage(stage);
        SetInsurerId(insurerId);
        SetIncidentId(incidentId);
        SetProductId(productId);
        SetFolderName(folderName);
        SetPolicyNo(policyNo);
        SetInsurerPolicyNo(insurerPolicyNo);
        SetDescription(description);
        SetOpenEmployeeId(openEmployeeId);
        SetPriority(priority);
        SetHasAdjustLocation(hasAdjustLocation);
    }

    private void SetFolderNo(string folderNo)
    {
        if (string.IsNullOrWhiteSpace(folderNo))
        {
            throw new ArgumentException("FolderNo cannot be null or empty.", nameof(folderNo));
        }
        if (folderNo.Length > 50)
        {
            throw new ArgumentException("FolderNo cannot exceed 50 characters.", nameof(folderNo));
        }
        FolderNo = folderNo;
    }

    private void SetFolderName(string? folderName)
    {
        if (!string.IsNullOrWhiteSpace(folderName) && folderName.Length > 250)
        {
            throw new ArgumentException("FolderName cannot exceed 250 characters.", nameof(folderName));
        }
        FolderName = folderName;
    }

    private void SetInsurerId(Guid? insurerId)
    {
        InsurerId = insurerId;
    }

    private void SetClaimId(Guid claimId)
    {
        if (claimId == Guid.Empty)
        {
            throw new ArgumentException("ClaimId cannot be empty.", nameof(claimId));
        }
        ClaimId = claimId;
    }

    private void SetIncidentId(Guid? incidentId)
    {
        IncidentId = incidentId;
    }

    private void SetProductId(Guid? productId)
    {
        ProductId = productId;
    }

    private void SetClaimTypeId(Guid claimTypeId)
    {
        if (claimTypeId == Guid.Empty)
        {
            throw new ArgumentException("ClaimTypeId cannot be empty.", nameof(claimTypeId));
        }
        ClaimTypeId = claimTypeId;
    }

    private void SetPolicyNo(string? policyNo)
    {
        if (!string.IsNullOrWhiteSpace(policyNo) && policyNo.Length > 50)
        {
            throw new ArgumentException("PolicyNo cannot exceed 50 characters.", nameof(policyNo));
        }
        PolicyNo = policyNo;
    }

    private void SetInsurerPolicyNo(string? insurerPolicyNo)
    {
        if (!string.IsNullOrWhiteSpace(insurerPolicyNo) && insurerPolicyNo.Length > 50)
        {
            throw new ArgumentException("InsurerPolicyNo cannot exceed 50 characters.", nameof(insurerPolicyNo));
        }
        InsurerPolicyNo = insurerPolicyNo;
    }

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }
        Description = description;
    }

    private void SetStatus(ClaimFolderStatus status)
    {
        Status = status;
    }

    private void SetStage(string stage)
    {
        if (string.IsNullOrWhiteSpace(stage))
        {
            throw new ArgumentException("Stage cannot be null or empty.", nameof(stage));
        }
        if (stage.Length > 50)
        {
            throw new ArgumentException("Stage cannot exceed 50 characters.", nameof(stage));
        }
        Stage = stage;
    }

    private void SetOpenDate(DateTime openDate)
    {
        OpenDate = openDate;
    }

    private void SetOpenEmployeeId(Guid? openEmployeeId)
    {
        OpenEmployeeId = openEmployeeId;
    }

    private void SetPriority(ClaimFolderPriority? priority)
    {
        Priority = priority;
    }

    private void SetHasAdjustLocation(string hasAdjustLocation)
    {
        if (string.IsNullOrWhiteSpace(hasAdjustLocation))
        {
            throw new ArgumentException("HasAdjustLocation cannot be null or empty.", nameof(hasAdjustLocation));
        }
        if (hasAdjustLocation.Length != 1 || (hasAdjustLocation != "Y" && hasAdjustLocation != "N"))
        {
            throw new ArgumentException("HasAdjustLocation must be 'Y' or 'N'.", nameof(hasAdjustLocation));
        }
        HasAdjustLocation = hasAdjustLocation;
    }

    public virtual void UpdateStatus(ClaimFolderStatus status) => SetStatus(status);
    public virtual void UpdateStage(string stage) => SetStage(stage);
    public virtual void UpdateCloseInfo(DateTime? closeDate, Guid? closeEmployeeId, string? closeNote)
    {
        CloseDate = closeDate;
        CloseEmployeeId = closeEmployeeId;
        if (!string.IsNullOrWhiteSpace(closeNote) && closeNote.Length > 500)
        {
            throw new ArgumentException("CloseNote cannot exceed 500 characters.", nameof(closeNote));
        }
        CloseNote = closeNote;
    }

    public virtual void UpdateCancelInfo(DateTime? cancelDate, Guid? cancelEmployeeId, Guid? cancelReasonId, string? cancelNote)
    {
        CancelDate = cancelDate;
        CancelEmployeeId = cancelEmployeeId;
        CancelReasonId = cancelReasonId;
        if (!string.IsNullOrWhiteSpace(cancelNote) && cancelNote.Length > 500)
        {
            throw new ArgumentException("CancelNote cannot exceed 500 characters.", nameof(cancelNote));
        }
        CancelNote = cancelNote;
    }
}

