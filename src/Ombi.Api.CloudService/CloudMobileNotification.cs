using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ombi.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api.CloudService
{
    public interface ICloudMobileNotification
    {
        Task<bool> SendMessage(MobileNotificationRequest notification);
    }
    public class CloudMobileNotification : ICloudMobileNotification
    {
        private readonly IApi _api;
        private readonly ILogger _logger;
        private readonly string _baseUrl;

        public CloudMobileNotification(IApi api, ILogger<CloudMobileNotification> logger, IOptions<ApplicationSettings> settings)
        {
            _api = api;
            _baseUrl = settings.Value.NotificationService;
            _logger = logger;
        }

        public async Task<bool> SendMessage(MobileNotificationRequest notification)
        {
            var request = new Request("MobileNotification", _baseUrl, HttpMethod.Post);
            request.AddJsonBody(notification);
            var response = await _api.Request(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error when sending mobile notification message, status code: {response.StatusCode}. Please raise an issue on Github, might be a problem with" +
                    $" the notification service!");
                return false;
            }
            return true;
        }
    }

    public class MobileNotificationRequest
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string To { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }
}
