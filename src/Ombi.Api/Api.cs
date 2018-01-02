using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;

namespace Ombi.Api
{
    public class Api : IApi
    {
        public Api(ILogger<Api> log, ISettingsService<OmbiSettings> s, ICacheService cache, IOmbiHttpClient client)
        {
            Logger = log;
            _settings = s;
            _cache = cache;
            _client = client;
        }

        private ILogger<Api> Logger { get; }
        private readonly ISettingsService<OmbiSettings> _settings;
        private readonly ICacheService _cache;
        private readonly IOmbiHttpClient _client;

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public async Task<T> Request<T>(Request request)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                // Add the Json Body
                if (request.JsonBody != null)
                {
                    httpRequestMessage.Content = new JsonContent(request.JsonBody);
                    httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); // Emby connect fails if we have the charset in the header
                }

                // Add headers
                foreach (var header in request.Headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);

                }
                using (var httpResponseMessage = await _client.SendAsync(httpRequestMessage))
                {
                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        Logger.LogError(LoggingEvents.Api, $"StatusCode: {httpResponseMessage.StatusCode}, Reason: {httpResponseMessage.ReasonPhrase}");
                    }
                    // do something with the response
                    var data = httpResponseMessage.Content;
                    var receivedString = await data.ReadAsStringAsync();
                    if (request.ContentType == ContentType.Json)
                    {
                        request.OnBeforeDeserialization?.Invoke(receivedString);
                        return JsonConvert.DeserializeObject<T>(receivedString, Settings);
                    }
                    else
                    {
                        // XML
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        StringReader reader = new StringReader(receivedString);
                        var value = (T)serializer.Deserialize(reader);
                        return value;
                    }
                }
            }

        }

        public async Task<string> RequestContent(Request request)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                // Add the Json Body
                if (request.JsonBody != null)
                {
                    httpRequestMessage.Content = new JsonContent(request.JsonBody);
                }

                // Add headers
                foreach (var header in request.Headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);

                }
                using (var httpResponseMessage = await _client.SendAsync(httpRequestMessage))
                {
                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        Logger.LogError(LoggingEvents.Api, $"StatusCode: {httpResponseMessage.StatusCode}, Reason: {httpResponseMessage.ReasonPhrase}");
                    }
                    // do something with the response
                    var data = httpResponseMessage.Content;


                    return await data.ReadAsStringAsync();
                }
            }

        }

        public async Task Request(Request request)
        {
                using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
                {
                    // Add the Json Body
                    if (request.JsonBody != null)
                    {
                        httpRequestMessage.Content = new JsonContent(request.JsonBody);
                    }

                    // Add headers
                    foreach (var header in request.Headers)
                    {
                        httpRequestMessage.Headers.Add(header.Key, header.Value);

                    }
                    using (var httpResponseMessage = await _client.SendAsync(httpRequestMessage))
                    {
                        if (!httpResponseMessage.IsSuccessStatusCode)
                        {
                            Logger.LogError(LoggingEvents.Api, $"StatusCode: {httpResponseMessage.StatusCode}, Reason: {httpResponseMessage.ReasonPhrase}");
                        }
                    }
                }
        }
    }
}
