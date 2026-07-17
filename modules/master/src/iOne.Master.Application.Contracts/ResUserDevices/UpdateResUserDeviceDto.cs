using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResUserDevices;

namespace iOne.Master.ResUserDevices;

public class UpdateResUserDeviceDto
{
    // ⚠️ QUAN TRỌNG: KHÔNG có UserName, DeviceUid, AppChannelCode, EffectDate
    // Các trường này không được phép sửa sau khi tạo

    [Required(ErrorMessage = "ResUserDevice:DeviceTokenRequired")]
    [MaxLength(500, ErrorMessage = "ResUserDevice:DeviceTokenMaxLength")]
    [Display(Name = "ResUserDevice:DeviceToken")]
    public string DeviceToken { get; set; } = null!;

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
