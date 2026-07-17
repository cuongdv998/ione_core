using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.ClaimFolders;
using Volo.Abp.Application.Dtos;

namespace iOne.Claim.Claims;

public class ClaimFolderDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ClaimFolder:FolderNo")]
    public string FolderNo { get; set; } = null!;

    [Display(Name = "ClaimFolder:FolderName")]
    public string? FolderName { get; set; }

    [Display(Name = "ClaimFolder:ClaimId")]
    public Guid ClaimId { get; set; }

    [Display(Name = "ClaimFolder:IncidentId")]
    public Guid? IncidentId { get; set; }

    [Display(Name = "ClaimFolder:InsurerId")]
    public Guid? InsurerId { get; set; }

    [Display(Name = "ClaimFolder:InsurerName")]
    public string? InsurerName { get; set; }

    [Display(Name = "ClaimFolder:ClaimTypeId")]
    public Guid ClaimTypeId { get; set; }

    [Display(Name = "ClaimFolder:PolicyNo")]
    public string? PolicyNo { get; set; }

    [Display(Name = "ClaimFolder:InsurerPolicyNo")]
    public string? InsurerPolicyNo { get; set; }

    [Display(Name = "ClaimFolder:Status")]
    public ClaimFolderStatus Status { get; set; }

    [Display(Name = "ClaimFolder:Stage")]
    public string Stage { get; set; } = null!;

    [Display(Name = "ClaimFolder:OpenDate")]
    public DateTime OpenDate { get; set; }

    [Display(Name = "ClaimFolder:OpenEmployeeName")]
    public string? OpenEmployeeName { get; set; }

    [Display(Name = "ClaimFolder:Priority")]
    public ClaimFolderPriority? Priority { get; set; }
}

public class ClaimFolderListDto : EntityDto<Guid>
{
    public string FolderNo { get; set; } = null!;
    public string? FolderName { get; set; }
    public string? InsurerName { get; set; }
    public string? CustomerName { get; set; }
    public string? CarPlate { get; set; }
    public string? PolicyNo { get; set; }
    public string? AdjustorName { get; set; }
    public ClaimFolderPriority? Priority { get; set; }
    public decimal? ClaimAmount { get; set; }
    public ClaimFolderStatus Status { get; set; }
    public DateTime OpenDate { get; set; }
}

public class GetClaimFoldersInput : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid ClaimId { get; set; }
}

public class CreateClaimFolderDto
{
    [Required]
    public Guid ClaimId { get; set; }

    /// <summary>
    /// Loại yêu cầu bồi thường. Nếu không truyền từ client sẽ tự lấy từ Claim tương ứng.
    /// </summary>
    public Guid? ClaimTypeId { get; set; }

    public Guid? InsurerId { get; set; }
    public Guid? IncidentId { get; set; }
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Ước bồi thường ban đầu khi mở hồ sơ.
    /// </summary>
    public decimal? EstimateAmount { get; set; }

    [StringLength(250)]
    public string? FolderName { get; set; }

    [StringLength(50)]
    public string? PolicyNo { get; set; }

    [StringLength(50)]
    public string? InsurerPolicyNo { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public ClaimFolderPriority? Priority { get; set; }

    /// <summary>Y/N - có giám định hiện trường hay không.</summary>
    [StringLength(1)]
    public string? HasAdjustLocation { get; set; }
    public string? AssigneeId { get; set; }

    /// <summary>
    /// Danh sách id loại đối tượng bảo hiểm (ResObjectType) user chọn theo sản phẩm/đơn;
    /// JSON thường dùng key camelCase <c>incidentObjectIds</c>.
    /// </summary>
    public List<Guid>? IncidentObjectIds { get; set; }

    public Guid? AssigneeOrganizationId { get; set; }

    public DateTime? AssessmentStartDate { get; set; }
}

public class ClaimObjectTypeDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;
}
