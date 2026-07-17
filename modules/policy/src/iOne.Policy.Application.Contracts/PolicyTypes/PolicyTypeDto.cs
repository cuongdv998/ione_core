using System;
using System.ComponentModel.DataAnnotations;
using iOne.PolicyTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Policy.PolicyTypes;

public class PolicyTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "PolicyType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "PolicyType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "PolicyType:Description")]
    public string? Description { get; set; }

    [Display(Name = "PolicyType:Status")]
    public PolicyTypeStatus Status { get; set; }
}
