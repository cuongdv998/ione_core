using System;
using System.ComponentModel.DataAnnotations;
using iOne.SystemEventNotifies;

namespace iOne.Master.SystemEventNotifies;

public class CreateSystemEventNotifyDto
{
    [Required(ErrorMessage = "SystemEventNotify:EventCodeRequired")]
    [MaxLength(50, ErrorMessage = "SystemEventNotify:EventCodeMaxLength")]
    [Display(Name = "SystemEventNotify:EventCode")]
    public string EventCode { get; set; } = null!;

    [Required(ErrorMessage = "SystemEventNotify:AppChannelIdRequired")]
    [Display(Name = "SystemEventNotify:AppChannelId")]
    public Guid AppChannelId { get; set; }

    [Required(ErrorMessage = "SystemEventNotify:TitleRequired")]
    [MaxLength(250, ErrorMessage = "SystemEventNotify:TitleMaxLength")]
    [Display(Name = "SystemEventNotify:Title")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "SystemEventNotify:BodyRequired")]
    [MaxLength(500, ErrorMessage = "SystemEventNotify:BodyMaxLength")]
    [Display(Name = "SystemEventNotify:Body")]
    public string Body { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "SystemEventNotify:PayloadMaxLength")]
    [Display(Name = "SystemEventNotify:Payload")]
    public string? Payload { get; set; }

    [Required(ErrorMessage = "SystemEventNotify:RecipientTypeRequired")]
    [MaxLength(10, ErrorMessage = "SystemEventNotify:RecipientTypeMaxLength")]
    [Display(Name = "SystemEventNotify:RecipientType")]
    public string RecipientType { get; set; } = null!;

    [Required(ErrorMessage = "SystemEventNotify:RecipientIdRequired")]
    [Display(Name = "SystemEventNotify:RecipientId")]
    public Guid RecipientId { get; set; }

    [MaxLength(500, ErrorMessage = "SystemEventNotify:RecipientMaxLength")]
    [Display(Name = "SystemEventNotify:Recipient")]
    public string? Recipient { get; set; }

    [Required(ErrorMessage = "SystemEventNotify:StatusRequired")]
    [Display(Name = "SystemEventNotify:Status")]
    public SystemEventNotifyStatus Status { get; set; }

    [Required(ErrorMessage = "SystemEventNotify:ScheduleAtRequired")]
    [Display(Name = "SystemEventNotify:ScheduleAt")]
    public DateTime ScheduleAt { get; set; }

    [Display(Name = "SystemEventNotify:SentAt")]
    public DateTime? SentAt { get; set; }

    [Display(Name = "SystemEventNotify:ReadAt")]
    public DateTime? ReadAt { get; set; }

    [MaxLength(1000, ErrorMessage = "SystemEventNotify:ErrorMessageMaxLength")]
    [Display(Name = "SystemEventNotify:ErrorMessage")]
    public string? ErrorMessage { get; set; }

    [Display(Name = "SystemEventNotify:RetryNumber")]
    public int RetryNumber { get; set; } = 0;
}
