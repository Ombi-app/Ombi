using System.Threading.Tasks;

namespace Ombi.Api.External.NotificationServices.Gotify
{
    public interface IGotifyApi
    {
        Task PushAsync(string endpoint, string accessToken, string subject, string body, sbyte priority);
    }
}