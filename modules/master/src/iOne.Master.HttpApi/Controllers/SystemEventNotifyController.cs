using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.SystemEventNotifies;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/system-event-notifies")]
[Authorize]
public class SystemEventNotifyController : AbpControllerBase
{
    protected ISystemEventNotifyAppService AppService { get; }

    public SystemEventNotifyController(ISystemEventNotifyAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(SystemEventNotifyPermissions.View)]
    public virtual Task<PagedResultDto<SystemEventNotifyDto>> GetListAsync(GetSystemEventNotifiesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(SystemEventNotifyPermissions.View)]
    public virtual Task<SystemEventNotifyDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(SystemEventNotifyPermissions.Create)]
    public virtual Task<SystemEventNotifyDto> CreateAsync(CreateSystemEventNotifyDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(SystemEventNotifyPermissions.Edit)]
    public virtual Task<SystemEventNotifyDto> UpdateAsync(Guid id, UpdateSystemEventNotifyDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(SystemEventNotifyPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }

    [HttpPost("send/{eventId}")]
    public virtual Task SendNotify(Guid eventId, [FromBody] EventNotifyInfoDto infor)
    {
        return AppService.SendNotify(eventId, infor);
    }

    /// <summary>
    /// Get list of SystemEventNotify by IDs with AppChannel Type information
    /// </summary>
    [HttpPost("by-ids-with-channel-type")]
    public virtual Task<List<SystemEventNotifyWithChannelTypeDto>> GetListByIdsWithChannelTypeAsync([FromBody] List<Guid> ids)
    {
        return AppService.GetListByIdsWithChannelTypeAsync(ids);
    }

    /// <summary>
    /// C?p nh?t k?t qu? g?i notification
    /// - N?u g?i thành công: status = Sent, retry_number + 1, sent_at = now
    /// - N?u g?i th?t b?i: status = Fail, retry_number + 1, error_message = errorMessage
    /// </summary>
    [HttpPut("{id}/send-result")]
    [Authorize(SystemEventNotifyPermissions.Edit)]
    public virtual Task<SystemEventNotifyDto> UpdateSendResultAsync(Guid id, [FromBody] UpdateNotifySendResultDto input)
    {
        return AppService.UpdateSendResultAsync(id, input);
    }

    /// <summary>
    /// L?y danh sách các b?n tin g?i l?i trong ngày có retry_number nh? h?n s? c?u hình c?a b?ng res_event_notify_template
    /// </summary>
    [HttpGet("failed-to-retry")]
    [Authorize(SystemEventNotifyPermissions.View)]
    public virtual Task<List<SystemEventNotifyWithChannelTypeDto>> GetFailedNotifiesToRetryAsync()
    {
        return AppService.GetFailedNotifiesToRetryAsync();
    }

    /// <summary>
    /// L?y danh sách thông báo web c?a ng??i dùng hi?n t?i có phân trang, s?p x?p theo CreationTime gi?m d?n
    /// </summary>
    [HttpGet("web")]
    [Authorize]
    public virtual Task<PagedResultDto<SystemEventNotifyDto>> GetListWebNotifyAsync([FromQuery] GetWebNotifiesInput input)
    {
        return AppService.GetListWebNotifyAsync(input);
    }

    /// <summary>
    /// G?i thông báo web real-time qua SignalR cho danh sách notification IDs
    /// </summary>
    [HttpPost("send-web-notification")]
    [Authorize]
    public virtual Task SendWebNotificationAsync([FromBody] SendWebNotificationDto input)
    {
        return AppService.SendWebNotificationAsync(input);
    }
}
