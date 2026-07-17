using System;
using System.ComponentModel.DataAnnotations;
using iOne.SystemEventNotifies;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.SystemEventNotifies;

public class SystemEventNotifyDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "SystemEventNotify:EventCode")]
    public string EventCode { get; set; } = null!;

    [Display(Name = "SystemEventNotify:AppChannelId")]
    public Guid AppChannelId { get; set; }

    [Display(Name = "SystemEventNotify:Title")]
    public string Title { get; set; } = null!;

    [Display(Name = "SystemEventNotify:Body")]
    public string Body { get; set; } = null!;

    [Display(Name = "SystemEventNotify:Payload")]
    public string? Payload { get; set; }

    [Display(Name = "SystemEventNotify:RecipientType")]
    public string RecipientType { get; set; } = null!;

    [Display(Name = "SystemEventNotify:RecipientId")]
    public Guid RecipientId { get; set; }

    [Display(Name = "SystemEventNotify:Recipient")]
    public string? Recipient { get; set; }

    [Display(Name = "SystemEventNotify:Status")]
    public SystemEventNotifyStatus Status { get; set; }

    [Display(Name = "SystemEventNotify:ScheduleAt")]
    public DateTime ScheduleAt { get; set; }

    [Display(Name = "SystemEventNotify:SentAt")]
    public DateTime? SentAt { get; set; }

    [Display(Name = "SystemEventNotify:ReadAt")]
    public DateTime? ReadAt { get; set; }

    [Display(Name = "SystemEventNotify:ErrorMessage")]
    public string? ErrorMessage { get; set; }

    [Display(Name = "SystemEventNotify:RetryNumber")]
    public int RetryNumber { get; set; }
}
