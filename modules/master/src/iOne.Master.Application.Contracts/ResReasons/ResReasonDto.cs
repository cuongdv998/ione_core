using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResReasonGroups;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResReasons;

public class ResReasonDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResReason:GroupId")]
    public Guid GroupId { get; set; }

    [Display(Name = "ResReason:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResReason:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResReason:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResReason:Status")]
    public ResReasonGroupStatus Status { get; set; }
}
