using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResBanks;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResBanks;

public class ResBankDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResBank:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResBank:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResBank:Status")]
    public ResBankStatus Status { get; set; }
}

