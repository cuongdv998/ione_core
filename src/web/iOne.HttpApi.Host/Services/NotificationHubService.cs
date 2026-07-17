using System;
using System.Threading.Tasks;
using iOne.Hubs;
using iOne.Master.SystemEventNotifies;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.DependencyInjection;

namespace iOne.Services;

public class NotificationHubService : INotificationHubService, ITransientDependency
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationToUserAsync(Guid userId, string title, string body, Guid notificationId)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
        {
            id = notificationId,
            title,
            body,
            createdAt = DateTime.UtcNow
        });
    }

    public async Task SendCustomEventToPolicyAsync(string policyId, string eventName, object data)
    {
        await _hubContext.Clients.Group($"policy_{policyId}").SendAsync(eventName, data);
    }
}
