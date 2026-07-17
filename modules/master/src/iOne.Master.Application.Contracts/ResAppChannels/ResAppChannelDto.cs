using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResAppChannels;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResAppChannels;

public class ResAppChannelDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResAppChannel:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResAppChannel:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResAppChannel:Description")]
    public string? Description { get; set; }

    [Display(Name = "ResAppChannel:Status")]
    public ResAppChannelStatus Status { get; set; }

    [Display(Name = "ResAppChannel:Type")]
    public ResAppChannelType? Type { get; set; }
}
