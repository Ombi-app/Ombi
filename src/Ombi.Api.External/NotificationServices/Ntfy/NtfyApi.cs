using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api.External.NotificationServices.Ntfy
{
    public class NtfyApi : INtfyApi
    {
        public NtfyApi(IApi api)
        {
            _api = api;
        }

        private readonly IApi _api;

        public async Task PushAsync(string baseUrl, string topic, string authorizationHeader, string subject, string message, int priority)
        {
            var request = new Request(string.Empty, baseUrl, HttpMethod.Post);

            // Add authorization header if provided (optional for public Ntfy servers)
            if (!string.IsNullOrWhiteSpace(authorizationHeader))
            {
                request.AddHeader("Authorization", authorizationHeader);
            }

            request.ApplicationJsonContentType();

            var jsonBody = new
            {
                topic = topic,
                title = subject,
                message = message,
                priority = priority
            };

            request.AddJsonBody(jsonBody);

            await _api.Request(request);
        }
    }
}
