using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Ombi.Helpers;
using Polly;

namespace Ombi.Api
{
    public class Api : IApi
    {
        public Api(ILogger<Api> log, IOmbiHttpClient client)
        {
            Logger = log;
            _client = client;
        }

        private ILogger<Api> Logger { get; }
        private readonly IOmbiHttpClient _client;

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public async Task<T> Request<T>(Request request)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                AddHeadersBody(request, httpRequestMessage);

                var httpResponseMessage = await _client.SendAsync(httpRequestMessage);

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (!request.IgnoreErrors)
                    {
                        await LogError(request, httpResponseMessage);
                    }

                    if (request.Retry)
                    {

                        var result = Policy
                            .Handle<HttpRequestException>()
                            .OrResult<HttpResponseMessage>(r => request.StatusCodeToRetry.Contains(r.StatusCode))
                            .WaitAndRetryAsync(new[]
                            {
                                TimeSpan.FromSeconds(10),
                            }, (exception, timeSpan, context) =>
                            {

                                Logger.LogError(LoggingEvents.Api,
                                    $"Retrying RequestUri: {request.FullUri} Because we got Status Code: {exception?.Result?.StatusCode}");
                            });

                        httpResponseMessage = await result.ExecuteAsync(async () =>
                        {
                            using (var req = await httpRequestMessage.Clone())
                            {
                                return await _client.SendAsync(req);
                            }
                        });
                    }
                }

                // do something with the response
                var receivedString = await httpResponseMessage.Content.ReadAsStringAsync();
                if (request.ContentType == ContentType.Json)
                {
                    request.OnBeforeDeserialization?.Invoke(receivedString);
                    return JsonConvert.DeserializeObject<T>(receivedString, Settings);
                }
                else
                {
                    // XML
                    return DeserializeXml<T>(receivedString);
                }
            }

        }

        public T DeserializeXml<T>(string receivedString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(receivedString);
            var value = (T) serializer.Deserialize(reader);
            return value;
        }

        public async Task<string> RequestContent(Request request)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                AddHeadersBody(request, httpRequestMessage);

                var httpResponseMessage = await _client.SendAsync(httpRequestMessage);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (!request.IgnoreErrors)
                    {
                        await LogError(request, httpResponseMessage);
                    }
                }
                // do something with the response
                var data = httpResponseMessage.Content;

                return await data.ReadAsStringAsync();
            }

        }

        public async Task Request(Request request)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                AddHeadersBody(request, httpRequestMessage);
                var httpResponseMessage = await _client.SendAsync(httpRequestMessage);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (!request.IgnoreErrors)
                    {
                        await LogError(request, httpResponseMessage);
                    }
                }
            }
        }

        private static void AddHeadersBody(Request request, HttpRequestMessage httpRequestMessage)
        {
            // Add the Json Body
            if (request.JsonBody != null)
            {
                httpRequestMessage.Content = new JsonContent(request.JsonBody);
                httpRequestMessage.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json"); // Emby connect fails if we have the charset in the header
            }

            // Add headers
            foreach (var header in request.Headers)
            {
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }
        }

        private async Task LogError(Request request, HttpResponseMessage httpResponseMessage)
        {
            Logger.LogError(LoggingEvents.Api,
                $"StatusCode: {httpResponseMessage.StatusCode}, Reason: {httpResponseMessage.ReasonPhrase}, RequestUri: {request.FullUri}");
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                Logger.LogDebug(content);
            }
        }
    }
}
