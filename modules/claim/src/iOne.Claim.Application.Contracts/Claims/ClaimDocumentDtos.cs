using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Claim.Claims;

public class ClaimDocumentDto : EntityDto<Guid>
{
    [Display(Name = "ClaimDocument:ClaimId")]
    public Guid? ClaimId { get; set; }

    public Guid? ClaimFolderId { get; set; }
    public Guid? AdjustAtLocationId { get; set; }

    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = null!;
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string MimeType { get; set; } = null!;

    public string? Note { get; set; }
    public string? Complete { get; set; }
    public string? IsCopy { get; set; }
    public DateTime? IssueDate { get; set; }

    /// <summary>Mã loại file (ảnh toàn cảnh, ảnh trước, ...).</summary>
    public string? DocumentGroupCode { get; set; }
}

public class GetClaimDocumentsInput : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid ClaimId { get; set; }

    public Guid? AdjustAtLocationId { get; set; }
}

public class GetClaimOnsiteImagesInput
{
    [Required]
    public Guid ClaimId { get; set; }

    public Guid? WorkTaskId { get; set; }
}
