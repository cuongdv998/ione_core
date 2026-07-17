using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using FirebaseAdmin.Messaging;

namespace iOne.AppMobile.Firebases
{
    public interface IFireBaseAppService : IApplicationService
    {
        Task<string> SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);
        Task<BatchResponse> SendNotificationToMultipleDevicesAsync(List<string> deviceTokens, string title, string body, Dictionary<string, string>? data = null);
        Task<string> SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null);
        Task<TopicManagementResponse> SubscribeToTopicAsync(List<string> deviceTokens, string topic);
        Task<TopicManagementResponse> UnsubscribeFromTopicAsync(List<string> deviceTokens, string topic);
        Task<object> CheckFirebaseAuthAsync();
    }
}
