using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.SystemEventNotifies;

public interface ISystemEventNotifyAppService : ICrudAppService<
    SystemEventNotifyDto,
    Guid,
    GetSystemEventNotifiesInput,
    CreateSystemEventNotifyDto,
    UpdateSystemEventNotifyDto>
{
    Task SendNotify(Guid eventId, EventNotifyInfoDto infor);

    /// <summary>
    /// Get list of SystemEventNotify by IDs with AppChannel Type information
    /// </summary>
    /// <param name="ids">List of SystemEventNotify IDs</param>
    /// <returns>List of SystemEventNotify with ChannelType</returns>
    Task<List<SystemEventNotifyWithChannelTypeDto>> GetListByIdsWithChannelTypeAsync(List<Guid> ids);

    /// <summary>
    /// C?p nh?t k?t qu? g?i notification
    /// - N?u g?i thành công: status = Sent, retry_number + 1, sent_at = now
    /// - N?u g?i th?t b?i: status = Fail, retry_number + 1, error_message = errorMessage
    /// </summary>
    /// <param name="id">ID c?a notification</param>
    /// <param name="input">K?t qu? g?i</param>
    /// <returns>Notification ?ã ???c c?p nh?t</returns>
    Task<SystemEventNotifyDto> UpdateSendResultAsync(Guid id, UpdateNotifySendResultDto input);

    /// <summary>
    /// L?y danh sách các b?n tin g?i l?i trong ngày có retry_number nh? h?n s? c?u hình c?a b?ng res_event_notify_template
    /// </summary>
    /// <returns>Danh sách các b?n tin c?n retry v?i thông tin channel type</returns>
    Task<List<SystemEventNotifyWithChannelTypeDto>> GetFailedNotifiesToRetryAsync();

    /// <summary>
    /// L?y danh sách thông báo web c?a ng??i dùng hi?n t?i có phân trang, s?p x?p theo CreationTime gi?m d?n
    /// </summary>
    /// <param name="input">Input phân trang</param>
    /// <returns>Danh sách thông báo web có phân trang</returns>
    Task<PagedResultDto<SystemEventNotifyDto>> GetListWebNotifyAsync(GetWebNotifiesInput input);

    /// <summary>
    /// G?i thông báo web real-time qua SignalR cho danh sách notification IDs
    /// </summary>
    /// <param name="input">Danh sách notification IDs</param>
    Task SendWebNotificationAsync(SendWebNotificationDto input);
}
