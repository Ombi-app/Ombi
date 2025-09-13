using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Api.External.NotificationServices.Webhook
{
    public interface IWebhookApi
    {
        Task PushAsync(string endpoint, string accessToken, IDictionary<string, string> parameters);
    }
}