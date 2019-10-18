using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api.Notifications.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Api.Notifications
{
    public class OneSignalApi : IOneSignalApi
    {
        public OneSignalApi(IApi api, IApplicationConfigRepository repo)
        {
            _api = api;
            _appConfig = repo;
        }

        private readonly IApi _api;
        private readonly IApplicationConfigRepository _appConfig;
        private const string ApiUrl = "https://onesignal.com/api/v1/notifications";

        public async Task<OneSignalNotificationResponse> PushNotification(List<string> playerIds, string message, bool isAdminNotification, int requestId, int requestType)
        {
            if (!playerIds.Any())
            {
                return null;
            }
            var id = await _appConfig.GetAsync(ConfigurationTypes.Notification);
            var request = new Request(string.Empty, ApiUrl, HttpMethod.Post);

            var body = new OneSignalNotificationBody
            {
                app_id = id.Value,
                contents = new Contents
                {
                    en = message
                },
                include_player_ids = playerIds.ToArray()
            };

            if (isAdminNotification)
            {
                // Add the action buttons
                body.data = new { requestid = requestId, requestType =  requestType};
                body.buttons = new[]
                {
                    new Button {id = "approve", text = "Approve Request"},
                    new Button {id = "deny", text = "Deny Request"},
                };
            }

            request.AddJsonBody(body);

            var result = await _api.Request<OneSignalNotificationResponse>(request);
            return result;
        }
    }
}