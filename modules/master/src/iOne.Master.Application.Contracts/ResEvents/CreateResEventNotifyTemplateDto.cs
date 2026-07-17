using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.ResEvents;

public class CreateResEventNotifyTemplateDto
{
    [Required(ErrorMessage = "ResEventNotifyTemplate:EventIdRequired")]
    [Display(Name = "ResEventNotifyTemplate:EventId")]
    public Guid EventId { get; set; }

    [Required(ErrorMessage = "ResEventNotifyTemplate:AppChannelIdRequired")]
    [Display(Name = "ResEventNotifyTemplate:AppChannelId")]
    public Guid AppChannelId { get; set; }

    [Required(ErrorMessage = "ResEventNotifyTemplate:RetryNumberRequired")]
    [Range(0, 99, ErrorMessage = "ResEventNotifyTemplate:RetryNumberRange")]
    [Display(Name = "ResEventNotifyTemplate:RetryNumber")]
    public int RetryNumber { get; set; } = 0;

    [Required(ErrorMessage = "ResEventNotifyTemplate:TitleRequired")]
    [StringLength(250, ErrorMessage = "ResEventNotifyTemplate:TitleMaxLength")]
    [Display(Name = "ResEventNotifyTemplate:Title")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "ResEventNotifyTemplate:BodyRequired")]
    [StringLength(500, ErrorMessage = "ResEventNotifyTemplate:BodyMaxLength")]
    [Display(Name = "ResEventNotifyTemplate:Body")]
    public string Body { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "ResEventNotifyTemplate:DataMaxLength")]
    [Display(Name = "ResEventNotifyTemplate:Data")]
    public string? Data { get; set; }
}

