using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResUserDevices;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResUserDevices;

public class ResUserDeviceDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResUserDevice:UserName")]
    public string UserName { get; set; } = null!;

    [Display(Name = "ResUserDevice:DeviceUid")]
    public string DeviceUid { get; set; } = null!;

    [Display(Name = "ResUserDevice:DeviceToken")]
    public string DeviceToken { get; set; } = null!;

    [Display(Name = "ResUserDevice:AppChannelCode")]
    public string AppChannelCode { get; set; } = null!;

    [Display(Name = "ResUserDevice:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ResUserDevice:ExpirDate")]
    public DateTime? ExpirDate { get; set; }

    [Display(Name = "ResUserDevice:Status")]
    public ResUserDeviceStatus Status { get; set; }

    [Display(Name = "ResUserDevice:Os")]
    public string? Os { get; set; }

    [Display(Name = "ResUserDevice:DeviceName")]
    public string? DeviceName { get; set; }
}
