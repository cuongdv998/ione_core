using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResReasonGroups;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResReasonGroups;

public class ResReasonGroupDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResReasonGroup:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResReasonGroup:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResReasonGroup:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResReasonGroup:Status")]
    public ResReasonGroupStatus Status { get; set; }
}
