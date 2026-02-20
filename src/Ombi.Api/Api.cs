using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Ombi.Helpers;
using Polly;

namespace Ombi.Api
{
    public class Api : IApi
    {
        public Api(ILogger<Api> log, HttpClient client, ICacheService cacheService, IHostEnvironment hostEnvironment)
        {
            Logger = log;
            _client = client;
            _cacheService = cacheService;
            _hostEnvironment = hostEnvironment;
        }

        private ILogger<Api> Logger { get; }
        private readonly HttpClient _client;
        private readonly ICacheService _cacheService;
        private readonly IHostEnvironment _hostEnvironment;

        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new PluralPropertyContractResolver()
        };

        public async Task<T> Request<T>(Request request, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check if caching should be used
            var shouldCache = ShouldCacheRequest(request);

            if (shouldCache)
            {
                var cacheKey = GenerateCacheKey(request);
                Logger.LogDebug($"ApiCache: Checking cache for {request.HttpMethod.Method} {request.FullUri}");

                try
                {
                    return await _cacheService.GetOrAddAsync(cacheKey, async () =>
                    {
                        Logger.LogDebug($"ApiCache: MISS for {request.HttpMethod.Method} {request.FullUri}");
                        return await ExecuteRequest<T>(request, cancellationToken);
                    }, DateTimeOffset.UtcNow.Add(request.CacheDuration.Value));
                }
                catch (JsonException ex)
                {
                    // Deserialization failed - evict cache and retry
                    Logger.LogWarning(ex, $"ApiCache: Deserialization failed for {request.FullUri}, evicting and retrying");
                    _cacheService.Remove(cacheKey);
                    return await ExecuteRequest<T>(request, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Cache service failed - log and proceed without cache
                    Logger.LogWarning(ex, $"ApiCache: Cache read failed for {request.FullUri}, proceeding without cache");
                    return await ExecuteRequest<T>(request, cancellationToken);
                }
            }

            // No caching - execute request directly
            return await ExecuteRequest<T>(request, cancellationToken);
        }

        private async Task<T> ExecuteRequest<T>(Request request, CancellationToken cancellationToken)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                AddHeadersBody(request, httpRequestMessage);

                var httpResponseMessage = await _client.SendAsync(httpRequestMessage, cancellationToken);

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
                                return await _client.SendAsync(req, cancellationToken);
                            }
                        });
                    }
                }

                // Only cache successful responses
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    // For failed responses, don't cache - just deserialize and return
                    var errorString = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                    LogDebugContent(errorString);
                    if (request.ContentType == ContentType.Json)
                    {
                        request.OnBeforeDeserialization?.Invoke(errorString);
                        return JsonConvert.DeserializeObject<T>(errorString, Settings);
                    }
                    else
                    {
                        return DeserializeXml<T>(errorString);
                    }
                }

                // do something with the response
                var receivedString = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                LogDebugContent(receivedString);
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

        private bool ShouldCacheRequest(Request request)
        {
            // Only cache GET requests
            if (request.HttpMethod != HttpMethod.Get)
            {
                return false;
            }

            // Don't cache in Development environment
            if (_hostEnvironment.IsDevelopment())
            {
                return false;
            }

            // Don't cache if CacheDuration is not set
            if (!request.CacheDuration.HasValue || request.CacheDuration.Value <= TimeSpan.Zero)
            {
                return false;
            }

            // Don't cache if explicitly bypassed
            if (request.BypassCache)
            {
                return false;
            }

            return true;
        }

        private string GenerateCacheKey(Request request)
        {
            return $"ApiCache:{request.HttpMethod.Method}:{request.FullUri}";
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
                await LogDebugContent(httpResponseMessage);
                return await data.ReadAsStringAsync();
            }

        }

        public async Task<HttpResponseMessage> Request(Request request, CancellationToken token = default(CancellationToken))
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                AddHeadersBody(request, httpRequestMessage);
                var httpResponseMessage = await _client.SendAsync(httpRequestMessage, token);
                await LogDebugContent(httpResponseMessage);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (!request.IgnoreErrors)
                    {
                        await LogError(request, httpResponseMessage);
                    }
                }

                return httpResponseMessage;
            }
        }

        private void AddHeadersBody(Request request, HttpRequestMessage httpRequestMessage)
        {
            // Add the Json Body
            if (request.JsonBody != null)
            {
                LogDebugContent("REQUEST: " + request.JsonBody);
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
            await LogDebugContent(httpResponseMessage);
        }

        private async Task LogDebugContent(HttpResponseMessage message)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                var content = await message.Content.ReadAsStringAsync();
                Logger.LogDebug(content);
            }
        }

        private void LogDebugContent(string message)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug(message);
            }
        }
    }
}
