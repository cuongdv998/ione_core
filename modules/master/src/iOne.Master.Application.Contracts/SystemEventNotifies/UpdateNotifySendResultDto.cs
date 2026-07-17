using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.SystemEventNotifies;

/// <summary>
/// DTO ?? c?p nh?t k?t qu? g?i notification
/// </summary>
public class UpdateNotifySendResultDto
{
    /// <summary>
    /// true = g?i thành công, false = g?i th?t b?i
    /// </summary>
    [Required]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Thông tin l?i (ch? dùng khi IsSuccess = false)
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
