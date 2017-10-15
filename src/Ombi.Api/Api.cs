using System;
using System.IO;
using System.Net.Http;
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
        public Api(ILogger<Api> log, ISettingsService<OmbiSettings> s, IMemoryCache cache)
        {
            Logger = log;
            _settings = s;
            _cache = cache;
        }

        private ILogger<Api> Logger { get; }
        private readonly ISettingsService<OmbiSettings> _settings;
        private readonly IMemoryCache _cache;

        private async Task<HttpMessageHandler> GetHandler()
        {
            var settings = await _cache.GetOrCreateAsync(CacheKeys.OmbiSettings, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
                return await _settings.GetSettingsAsync();
            });
            if (settings.IgnoreCertificateErrors)
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
            }
            return new HttpClientHandler();
        }

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public async Task<T> Request<T>(Request request)
        {
            using (var httpClient = new HttpClient(await GetHandler()))
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
                    using (var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage))
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
        }

        public async Task<string> RequestContent(Request request)
        {
            using (var httpClient = new HttpClient(await GetHandler()))
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
                    using (var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage))
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
        }

        public async Task Request(Request request)
        {
            using (var httpClient = new HttpClient(await GetHandler()))
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
                    using (var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage))
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
}
