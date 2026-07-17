using System;
using System.ComponentModel.DataAnnotations;
using iOne.ClaimStages;
using iOne.WorkTasks;
using Volo.Abp.Application.Dtos;

namespace iOne.Claim.Claims;

public class ClaimStageProgressDto : EntityDto<Guid>
{
    /// <summary>Id claim stage.</summary>
    public Guid ClaimStageId { get; set; }

    public Guid ClaimId { get; set; }
    public Guid? ClaimFolderId { get; set; }

    [Display(Name = "ClaimStage:FolderNo")]
    public string? FolderNo { get; set; }

    [Display(Name = "ClaimStage:StageName")]
    public string StageName { get; set; } = null!;

    [Display(Name = "ClaimStage:TaskName")]
    public string? TaskName { get; set; }

    [Display(Name = "ClaimStage:PerformerName")]
    public string? PerformerName { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>SLA (phút) theo cấu hình.</summary>
    public int? SlaTime { get; set; }

    /// <summary>Thực tế (phút) từ StartDate tới EndDate / Now.</summary>
    public double? ActualMinutes { get; set; }

    public string? SlaText { get; set; }

    public ClaimStageStatus Status { get; set; }

    public WorkTaskStatus? WorkTaskStatus { get; set; }
}

public class GetClaimStageProgressInput : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid ClaimId { get; set; }

    public Guid? ClaimFolderId { get; set; }
}

