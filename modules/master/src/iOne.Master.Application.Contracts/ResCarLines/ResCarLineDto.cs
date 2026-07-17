using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCarLines;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCarLines;

public class ResCarLineDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResCarLine:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResCarLine:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResCarLine:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResCarLine:Status")]
    public ResCarLineStatus Status { get; set; }
}


