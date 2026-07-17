using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResWards;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResWards;

public class ResWardDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Master::ResWard:Province")]
    public Guid ProvinceId { get; set; }

    [Display(Name = "Master::ResWard:ProvinceName")]
    public string ProvinceName { get; set; } = null!; // For display purposes

    [Display(Name = "Master::ResWard:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Master::ResWard:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Master::ResWard:Status")]
    public ResWardStatus Status { get; set; }

    [Display(Name = "Master::ResWard:Description")]
    public string? Description { get; set; }
}

