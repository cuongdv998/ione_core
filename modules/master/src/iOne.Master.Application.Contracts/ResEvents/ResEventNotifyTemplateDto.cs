using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResEvents;

public class ResEventNotifyTemplateDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResEventNotifyTemplate:EventId")]
    public Guid EventId { get; set; }

    [Display(Name = "ResEventNotifyTemplate:AppChannelId")]
    public Guid AppChannelId { get; set; }

    [Display(Name = "ResEventNotifyTemplate:AppChannelCode")]
    public string? AppChannelCode { get; set; }

    [Display(Name = "ResEventNotifyTemplate:AppChannelName")]
    public string? AppChannelName { get; set; }

    [Display(Name = "ResEventNotifyTemplate:RetryNumber")]
    public int RetryNumber { get; set; }

    [Display(Name = "ResEventNotifyTemplate:Title")]
    public string Title { get; set; } = null!;

    [Display(Name = "ResEventNotifyTemplate:Body")]
    public string Body { get; set; } = null!;

    [Display(Name = "ResEventNotifyTemplate:Data")]
    public string? Data { get; set; }
}

