using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using iOne.AppMobile.Firebases;
using iOne.AppMobile.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.AppMobile.Controllers;

[RemoteService(Name = AppMobileRemoteServiceConsts.RemoteServiceName)]
[Area(AppMobileRemoteServiceConsts.ModuleName)]
[Route("api/appmobile/firebase")]
//[Authorize(FirebasePermissions.Default)]
public class FirebaseController : AbpControllerBase
{
    protected IFireBaseAppService AppService { get; }

    public FirebaseController(IFireBaseAppService appService)
    {
        AppService = appService;
    }

    /// <summary>
    /// Send push notification to a single device
    /// </summary>
    [HttpPost("send")]
    //[Authorize(FirebasePermissions.SendNotification)]
    public virtual Task<string> SendNotificationAsync([FromBody] SendNotificationInput input)
    {
        return AppService.SendNotificationAsync(input.DeviceToken, input.Title, input.Body, input.Data);
    }

    /// <summary>
    /// Send push notification to multiple devices
    /// </summary>
    [HttpPost("send-multiple")]
    //[Authorize(FirebasePermissions.SendToMultiple)]
    public virtual Task<BatchResponse> SendNotificationToMultipleDevicesAsync([FromBody] SendMultipleNotificationInput input)
    {
        return AppService.SendNotificationToMultipleDevicesAsync(input.DeviceTokens, input.Title, input.Body, input.Data);
    }

    /// <summary>
    /// Send push notification to a topic
    /// </summary>
    [HttpPost("send-topic")]
    //[Authorize(FirebasePermissions.SendToTopic)]
    public virtual Task<string> SendNotificationToTopicAsync([FromBody] SendTopicNotificationInput input)
    {
        return AppService.SendNotificationToTopicAsync(input.Topic, input.Title, input.Body, input.Data);
    }

    /// <summary>
    /// Subscribe devices to a topic
    /// </summary>
    [HttpPost("subscribe")]
    //[Authorize(FirebasePermissions.SubscribeToTopic)]
    public virtual Task<TopicManagementResponse> SubscribeToTopicAsync([FromBody] TopicSubscriptionInput input)
    {
        return AppService.SubscribeToTopicAsync(input.DeviceTokens, input.Topic);
    }

    /// <summary>
    /// Unsubscribe devices from a topic
    /// </summary>
    [HttpPost("unsubscribe")]
    //[Authorize(FirebasePermissions.UnsubscribeFromTopic)]
    public virtual Task<TopicManagementResponse> UnsubscribeFromTopicAsync([FromBody] TopicSubscriptionInput input)
    {
        return AppService.UnsubscribeFromTopicAsync(input.DeviceTokens, input.Topic);
    }

    [HttpGet]
    [Route("check-auth")]
    public async Task<object> CheckAuth()
    {
        return await AppService.CheckFirebaseAuthAsync();
    }
}

/// <summary>
/// Input model for sending notification to a single device
/// </summary>
public class SendNotificationInput
{
    public string DeviceToken { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public Dictionary<string, string>? Data { get; set; }
}

/// <summary>
/// Input model for sending notification to multiple devices
/// </summary>
public class SendMultipleNotificationInput
{
    public List<string> DeviceTokens { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public Dictionary<string, string>? Data { get; set; }
}

/// <summary>
/// Input model for sending notification to a topic
/// </summary>
public class SendTopicNotificationInput
{
    public string Topic { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public Dictionary<string, string>? Data { get; set; }
}

/// <summary>
/// Input model for topic subscription/unsubscription
/// </summary>
public class TopicSubscriptionInput
{
    public List<string> DeviceTokens { get; set; } = null!;
    public string Topic { get; set; } = null!;
}
