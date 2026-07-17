using System;
using System.ComponentModel.DataAnnotations;
using iOne.AdminConfigs;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.AdminConfigs;

public class AdminConfigDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Master::AdminConfig:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Master::AdminConfig:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Master::AdminConfig:SubCode")]
    public string SubCode { get; set; } = null!;

    [Display(Name = "Master::AdminConfig:Value")]
    public string Value { get; set; } = null!;

    [Display(Name = "Master::AdminConfig:Description")]
    public string? Description { get; set; }

    [Display(Name = "Master::AdminConfig:Status")]
    public AdminConfigStatus Status { get; set; }
}

