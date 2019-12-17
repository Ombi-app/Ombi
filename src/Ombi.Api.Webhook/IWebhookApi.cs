using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Api.Webhook
{
    public interface IWebhookApi
    {
        Task PushAsync(string endpoint, string accessToken, IDictionary<string, string> parameters);
    }
}