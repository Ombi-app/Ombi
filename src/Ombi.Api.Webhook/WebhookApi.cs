using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api.Webhook
{
    public class WebhookApi : IWebhookApi
    {
        private static readonly CamelCasePropertyNamesContractResolver _nameResolver = new CamelCasePropertyNamesContractResolver();

        public WebhookApi(IApi api)
        {
            _api = api;
        }

        private readonly IApi _api;

        public async Task PushAsync(string baseUrl, string accessToken, IDictionary<string, string> parameters)
        {
            var request = new Request("/", baseUrl, HttpMethod.Post);

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.AddQueryString("token", accessToken);
                request.AddHeader("Access-Token", accessToken);
            }

            var body = parameters.ToDictionary(
                x => _nameResolver.GetResolvedPropertyName(x.Key),
                x => x.Value
            );

            request.ApplicationJsonContentType();
            request.AddJsonBody(body);

            await _api.Request(request);
        }
    }
}
