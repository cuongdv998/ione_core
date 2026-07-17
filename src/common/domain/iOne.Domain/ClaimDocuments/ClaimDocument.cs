using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.Claims;
using iOne.ClaimFolders;
using iOne.ClaimAdjustAtLocations;
using iOne.ResDocuments;
using iOne.ResDocumentTypes;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ClaimDocuments;

[Table("claim_document")]
public class ClaimDocument : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? ClaimId { get; private set; }

    public virtual Guid? DocumentId { get; private set; }

    public virtual Guid? DocumentTypeId { get; private set; }

    public virtual Guid? ClaimFolderId { get; private set; }

    public virtual Guid? ClaimFolderObjectId { get; private set; }

    public virtual Guid? AdjustAtLocationId { get; private set; }

    public virtual Guid? ClaimFolderItemId { get; private set; }

    public virtual Guid? QuotationId { get; private set; }

    [MaxLength(250)]
    public virtual string? Note { get; private set; }

    /// <summary>Y/N - hồ sơ đã đầy đủ.</summary>
    [MaxLength(1)]
    public virtual string? Complete { get; private set; }

    /// <summary>Y/N - là bản sao.</summary>
    [MaxLength(1)]
    public virtual string? IsCopy { get; private set; }

    public virtual DateTime? IssueDate { get; private set; }

    // Navigation properties (only the most used for now)
    public virtual Claim? Claim { get; set; }
    public virtual ResDocument? Document { get; set; }
    public virtual ResDocumentType? DocumentType { get; set; }
    public virtual ClaimFolder? ClaimFolder { get; set; }
    public virtual ClaimAdjustAtLocation? AdjustAtLocation { get; set; }

    protected ClaimDocument()
    {
        // For ORM
    }

    public ClaimDocument(
        Guid id,
        Guid? claimId = null,
        Guid? documentId = null,
        Guid? documentTypeId = null,
        Guid? claimFolderId = null,
        Guid? claimFolderObjectId = null,
        Guid? adjustAtLocationId = null,
        Guid? claimFolderItemId = null,
        Guid? quotationId = null,
        string? note = null,
        string? complete = null,
        string? isCopy = null,
        DateTime? issueDate = null)
        : base(id)
    {
        ClaimId = claimId;
        DocumentId = documentId;
        DocumentTypeId = documentTypeId;
        ClaimFolderId = claimFolderId;
        ClaimFolderObjectId = claimFolderObjectId;
        AdjustAtLocationId = adjustAtLocationId;
        ClaimFolderItemId = claimFolderItemId;
        QuotationId = quotationId;
        SetNote(note);
        SetComplete(complete);
        SetIsCopy(isCopy);
        IssueDate = issueDate;
    }

    private void SetNote(string? note)
    {
        if (!string.IsNullOrWhiteSpace(note) && note.Length > 250)
        {
            throw new ArgumentException("Note cannot exceed 250 characters.", nameof(note));
        }
        Note = note;
    }

    private void SetComplete(string? complete)
    {
        if (string.IsNullOrWhiteSpace(complete))
        {
            Complete = null;
            return;
        }
        if (complete.Length != 1 || (complete != "Y" && complete != "N"))
        {
            throw new ArgumentException("Complete must be 'Y' or 'N'.", nameof(complete));
        }
        Complete = complete;
    }

    private void SetIsCopy(string? isCopy)
    {
        if (string.IsNullOrWhiteSpace(isCopy))
        {
            IsCopy = null;
            return;
        }
        if (isCopy.Length != 1 || (isCopy != "Y" && isCopy != "N"))
        {
            throw new ArgumentException("IsCopy must be 'Y' or 'N'.", nameof(isCopy));
        }
        IsCopy = isCopy;
    }

    public virtual void UpdateMetadata(string? note, string? complete, string? isCopy, DateTime? issueDate)
    {
        SetNote(note);
        SetComplete(complete);
        SetIsCopy(isCopy);
        IssueDate = issueDate;
    }

    public virtual void UpdateClaimFolderId(Guid? claimFolderId)
    {
        ClaimFolderId = claimFolderId;
    }

    public virtual void UpdateDocumentTypeId(Guid? documentTypeId)
    {
        DocumentTypeId = documentTypeId;
    }
}
