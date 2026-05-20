using System.Threading.Tasks;

namespace Ombi.Api.External.NotificationServices.Ntfy
{
    public interface INtfyApi
    {
        Task PushAsync(string baseUrl, string topic, string authorizationHeader, string subject, string message, int priority);
    }
}
