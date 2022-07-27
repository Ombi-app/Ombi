using Ombi.Api.Ntfy.Models;

namespace Ombi.Api.Ntfy;

public interface INtfyApi
{
    Task PushAsync(string endpoint, string authorizationHeader, NtfyNotificationBody body);
}