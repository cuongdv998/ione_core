using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCarGroups;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarGroups;

public class ResCarGroupDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResCarGroup:CarLineId")]
    public Guid? CarLineId { get; set; }

    [Display(Name = "ResCarGroup:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResCarGroup:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResCarGroup:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResCarGroup:Status")]
    public ResCarGroupStatus Status { get; set; }
}
