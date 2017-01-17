#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: NetflixRouletteApi.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Netflix;
using Ombi.Api.Models.Notifications;
using RestSharp;

namespace Ombi.Api
{
    public class DiscordApi : IDiscordApi
    {
        public DiscordApi(IApiRequest req)
        {
            Api = req;
        }

        private IApiRequest Api { get; }
        private Uri Endpoint => new Uri("https://discordapp.com/api/"); //webhooks/270828242636636161/lLysOMhJ96AFO1kvev0bSqP-WCZxKUh1UwfubhIcLkpS0DtM3cg4Pgeraw3waoTXbZii


        public void SendMessage(string message, string webhookId, string webhookToken, string username = null)
        {
            var request = new RestRequest
            {
                Resource = "webhooks/{webhookId}/{webhookToken}"
            };

            request.AddUrlSegment("webhookId", webhookId);
            request.AddUrlSegment("webhookToken", webhookToken);

            var body = new DiscordWebhookRequest
            {
                content = message,
                username = username
            };
            request.AddJsonBody(body);

            request.AddHeader("Content-Type", "application/json");

            Api.Execute(request, Endpoint);
        }

        public async Task SendMessageAsync(string message, string webhookId, string webhookToken, string username = null)
        {
            var request = new RestRequest
            {
                Resource = "webhooks/{webhookId}/{webhookToken}"
            };

            request.AddUrlSegment("webhookId", webhookId);
            request.AddUrlSegment("webhookToken", webhookToken);

            var body = new DiscordWebhookRequest
            {
                content = message,
                username = username
            };
            request.AddJsonBody(body);

            request.AddHeader("Content-Type", "application/json");

            await Task.Run(
                () =>
                {
                    Api.Execute(request, Endpoint);

                });
        }



        public NetflixMovieResult CheckNetflix(string title, string year = null)
        {
            var request = new RestRequest();
            request.AddQueryParameter("title", title);
            if (!string.IsNullOrEmpty(year))
            {
                request.AddQueryParameter("year", year);
            }
            var result = Api.Execute(request, Endpoint);

            return JsonConvert.DeserializeObject<NetflixMovieResult>(result.Content);
        }
    }
}