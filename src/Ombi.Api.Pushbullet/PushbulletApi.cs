using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api.Pushbullet
{
    public class PushbulletApi : IPushbulletApi
    {
        public PushbulletApi(IApi api)
        {
            Api = api;
        }

        private IApi Api { get; }
        private const string BaseUrl = "https://api.pushbullet.com/v2";
        public async Task Push(string accessToken, string subject, string body, string channelTag)
        {
            var request = new Request("/pushes", BaseUrl, HttpMethod.Post);

            request.AddHeader("Access-Token", accessToken);
            request.ApplicationJsonContentType();


            var jsonBody = new
            {
                type = "note",
                body = body,
                title = subject,
                channel_tag = channelTag
            };

            request.AddJsonBody(jsonBody);

            await Api.Request(request);
        }
    }
}
