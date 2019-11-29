using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Api.Webhook
{
    public interface IWebhookApi
    {
        Task PushAsync(string endpoint, string accessToken, IReadOnlyDictionary<string, string> parameters);
    }
}