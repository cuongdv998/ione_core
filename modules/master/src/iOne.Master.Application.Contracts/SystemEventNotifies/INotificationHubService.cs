using System;
using System.Threading.Tasks;

namespace iOne.Master.SystemEventNotifies;

public interface INotificationHubService
{
    Task SendNotificationToUserAsync(Guid userId, string title, string body, Guid notificationId);
    Task SendCustomEventToPolicyAsync(string policyId, string eventName, object data);
}
