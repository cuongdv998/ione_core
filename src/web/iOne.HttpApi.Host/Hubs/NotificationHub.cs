using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.AspNetCore.SignalR;

namespace iOne.Hubs;

[Authorize]
public class NotificationHub : AbpHub
{
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task JoinPolicyGroup(string policyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"policy_{policyId}");
    }
}
