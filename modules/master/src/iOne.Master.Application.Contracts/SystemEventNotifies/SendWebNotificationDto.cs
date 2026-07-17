using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.SystemEventNotifies;

/// <summary>
/// DTO ?? g?i thông báo web real-time qua SignalR cho danh sách notification IDs
/// </summary>
public class SendWebNotificationDto
{
    /// <summary>
    /// Danh sách notification IDs c?n g?i real-time
    /// </summary>
    [Required]
    public List<Guid> NotificationIds { get; set; } = new();
}
