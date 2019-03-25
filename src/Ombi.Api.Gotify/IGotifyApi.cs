using System.Threading.Tasks;

namespace Ombi.Api.Gotify
{
    public interface IGotifyApi
    {
        Task PushAsync(string endpoint, string accessToken, string subject, string body, sbyte priority);
    }
}