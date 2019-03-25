using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api.Gotify
{
    public class GotifyApi : IGotifyApi
    {
        public GotifyApi(IApi api)
        {
            _api = api;
        }

        private readonly IApi _api;

        public async Task PushAsync(string baseUrl, string accessToken, string subject, string body, sbyte priority)
        {
            var request = new Request("/message", baseUrl, HttpMethod.Post);
            request.AddQueryString("token", accessToken);

            request.AddHeader("Access-Token", accessToken);
            request.ApplicationJsonContentType();


            var jsonBody = new
            {
                message = body,
                title = subject,
                priority = priority
            };

            request.AddJsonBody(jsonBody);

            await _api.Request(request);
        }
    }
}
