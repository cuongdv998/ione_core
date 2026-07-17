using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResChannels;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResChannels;

public class ResChannelDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Partner::ResChannel:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Partner::ResChannel:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Partner::ResChannel:Status")]
    public ResChannelStatus Status { get; set; }

    [Display(Name = "Partner::ResChannel:Description")]
    public string? Description { get; set; }
}

