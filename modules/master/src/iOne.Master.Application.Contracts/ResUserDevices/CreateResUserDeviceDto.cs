using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResUserDevices;

namespace iOne.Master.ResUserDevices;

public class CreateResUserDeviceDto
{
    [Required(ErrorMessage = "ResUserDevice:UserNameRequired")]
    [MaxLength(50, ErrorMessage = "ResUserDevice:UserNameMaxLength")]
    [Display(Name = "ResUserDevice:UserName")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "ResUserDevice:DeviceUidRequired")]
    [MaxLength(250, ErrorMessage = "ResUserDevice:DeviceUidMaxLength")]
    [Display(Name = "ResUserDevice:DeviceUid")]
    public string DeviceUid { get; set; } = null!;

    [Required(ErrorMessage = "ResUserDevice:DeviceTokenRequired")]
    [MaxLength(500, ErrorMessage = "ResUserDevice:DeviceTokenMaxLength")]
    [Display(Name = "ResUserDevice:DeviceToken")]
    public string DeviceToken { get; set; } = null!;

    [Required(ErrorMessage = "ResUserDevice:AppChannelCodeRequired")]
    [MaxLength(50, ErrorMessage = "ResUserDevice:AppChannelCodeMaxLength")]
    [Display(Name = "ResUserDevice:AppChannelCode")]
    public string AppChannelCode { get; set; } = null!;

    [Required(ErrorMessage = "ResUserDevice:EffectDateRequired")]
    [Display(Name = "ResUserDevice:EffectDate")]
    public DateTime EffectDate { get; set; }

    [Display(Name = "ResUserDevice:ExpirDate")]
    public DateTime? ExpirDate { get; set; }

    [Required(ErrorMessage = "ResUserDevice:StatusRequired")]
    [Display(Name = "ResUserDevice:Status")]
    public ResUserDeviceStatus Status { get; set; }

    [MaxLength(50, ErrorMessage = "ResUserDevice:OsMaxLength")]
    [Display(Name = "ResUserDevice:Os")]
    public string? Os { get; set; }

    [MaxLength(250, ErrorMessage = "ResUserDevice:DeviceNameMaxLength")]
    [Display(Name = "ResUserDevice:DeviceName")]
    public string? DeviceName { get; set; }
}
