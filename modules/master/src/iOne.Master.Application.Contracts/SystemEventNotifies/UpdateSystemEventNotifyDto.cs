using iOne.SystemEventNotifies;
using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.SystemEventNotifies;

/// <summary>
/// Chỉ cho phép sửa trạng thái (Status).
/// </summary>
public class UpdateSystemEventNotifyDto
{
    [Required(ErrorMessage = "SystemEventNotify:StatusRequired")]
    [Display(Name = "SystemEventNotify:Status")]
    public SystemEventNotifyStatus Status { get; set; }

    [Display(Name = "SystemEventNotify:ReadAt")]
    public DateTime? ReadAt { get; set; }
}
