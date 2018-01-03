using System;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api.Slack.Models;

namespace Ombi.Api.Slack
{
    public class SlackApi : ISlackApi
    {
        public SlackApi(IApi api)
        {
            Api = api;
        }

        private IApi Api { get; }
        private const string BaseUrl = "https://hooks.slack.com/";
        public async Task<string> PushAsync(string team, string token, string service, SlackNotificationBody message)
        {

            var request = new Request($"/services/{team}/{service}/{token}", BaseUrl, HttpMethod.Post);
            dynamic body = new ExpandoObject();
            body.channel = message.channel;
            body.text = message.text;
            body.username = message.username;
            body.link_names = 1;

            if (!string.IsNullOrEmpty(message.icon_url))
            {
                body.icon_url = message.icon_url;
            }

            if (!string.IsNullOrEmpty(message.icon_emoji))
            {
                body.icon_emoji = message.icon_emoji;
            }
            request.AddJsonBody(body);
            request.ApplicationJsonContentType();

            return await Api.RequestContent(request);
        }
    }
}
