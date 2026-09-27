using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
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

        // DNS failures are often very short-lived (for example while a local resolver
        // restarts or an upstream resolver briefly fails to answer). Retrying only
        // name-resolution failures is safe even for POST requests because the request
        // has not reached the remote server when DNS resolution fails.
        private static readonly TimeSpan[] DnsRetryDelays =
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(10),
        };

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
                    var wasSuccessful = true;
                    var cachedResult = await _cacheService.GetOrAddAsync(cacheKey, async () =>
                    {
                        Logger.LogDebug($"ApiCache: MISS for {request.HttpMethod.Method} {request.FullUri}");
                        var (value, requestSucceeded) = await ExecuteRequest<T>(request, cancellationToken);
                        wasSuccessful = requestSucceeded;
                        return value;
                    }, DateTimeOffset.UtcNow.Add(request.CacheDuration.Value));

                    // Don't keep unsuccessful responses in the cache, otherwise a transient
                    // failure (e.g. an expired token returning a 401) would be served from the
                    // cache for the entire cache duration.
                    if (!wasSuccessful)
                    {
                        _cacheService.Remove(cacheKey);
                    }

                    return cachedResult;
                }
                catch (JsonException ex)
                {
                    // Deserialization failed - evict cache and retry
                    Logger.LogWarning(ex, $"ApiCache: Deserialization failed for {request.FullUri}, evicting and retrying");
                    _cacheService.Remove(cacheKey);
                    return (await ExecuteRequest<T>(request, cancellationToken)).value;
                }
                catch (Exception ex)
                {
                    // Cache service failed - log and proceed without cache
                    Logger.LogWarning(ex, $"ApiCache: Cache read failed for {request.FullUri}, proceeding without cache");
                    return (await ExecuteRequest<T>(request, cancellationToken)).value;
                }
            }

            // No caching - execute request directly
            return (await ExecuteRequest<T>(request, cancellationToken)).value;
        }

        private async Task<(T value, bool wasSuccessful)> ExecuteRequest<T>(Request request, CancellationToken cancellationToken)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                AddHeadersBody(request, httpRequestMessage);

                var httpResponseMessage = await SendRequestAsync(request, httpRequestMessage, cancellationToken);

                if (!httpResponseMessage.IsSuccessStatusCode && !request.IgnoreErrors)
                {
                    await LogError(request, httpResponseMessage);
                }

                // Only cache successful responses
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (request.ThrowOnErrorStatus)
                    {
                        throw new HttpRequestException(
                            $"External API request failed with HTTP {(int)httpResponseMessage.StatusCode} ({httpResponseMessage.StatusCode}).",
                            null,
                            httpResponseMessage.StatusCode);
                    }

                    // For failed responses, don't cache. The body is usually an error payload
                    // (e.g. Plex returns an <errors> document on a 401) that does not match the
                    // expected success type, so attempt to deserialize it but fall back to the
                    // default value rather than throwing a misleading deserialization exception.
                    var errorString = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                    LogDebugContent(errorString);
                    try
                    {
                        if (request.ContentType == ContentType.Json)
                        {
                            request.OnBeforeDeserialization?.Invoke(errorString);
                            return (JsonConvert.DeserializeObject<T>(errorString, Settings), false);
                        }

                        return (DeserializeXml<T>(errorString), false);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex,
                            $"Could not deserialize the error response from {request.FullUri} (Status Code: {httpResponseMessage.StatusCode}) into {typeof(T).Name}, returning default");
                        return (default, false);
                    }
                }

                // do something with the response
                var receivedString = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                LogDebugContent(receivedString);
                if (request.ContentType == ContentType.Json)
                {
                    request.OnBeforeDeserialization?.Invoke(receivedString);
                    return (JsonConvert.DeserializeObject<T>(receivedString, Settings), true);
                }

                // XML
                return (DeserializeXml<T>(receivedString), true);
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

                var httpResponseMessage = await SendRequestAsync(request, httpRequestMessage, CancellationToken.None);
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
                var httpResponseMessage = await SendRequestAsync(request, httpRequestMessage, token);
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

        private async Task<HttpResponseMessage> SendRequestAsync(Request request, HttpRequestMessage template, CancellationToken cancellationToken)
        {
            if (!request.Retry)
            {
                return await SendWithDnsRetryAsync(template, cancellationToken);
            }

            // Preserve the existing opt-in retry behavior for callers that explicitly
            // request retries (for example TMDB 429 handling), while DNS failures are
            // handled separately below for every request.
            var retryPolicy = Policy
                .Handle<HttpRequestException>(ex => !IsDnsResolutionFailure(ex))
                .OrResult<HttpResponseMessage>(r => request.StatusCodeToRetry.Contains(r.StatusCode))
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(10),
                }, (outcome, delay, context) =>
                {
                    if (outcome.Exception != null)
                    {
                        Logger.LogWarning(LoggingEvents.Api,
                            outcome.Exception,
                            "Retrying RequestUri: {RequestUri} in {DelaySeconds} seconds because of a transient HTTP error",
                            SafeUriForLogging(request.FullUri), delay.TotalSeconds);
                        return;
                    }

                    Logger.LogWarning(LoggingEvents.Api,
                        "Retrying RequestUri: {RequestUri} in {DelaySeconds} seconds because we got Status Code: {StatusCode}",
                        SafeUriForLogging(request.FullUri), delay.TotalSeconds, outcome.Result?.StatusCode);
                });

            return await retryPolicy.ExecuteAsync(() => SendWithDnsRetryAsync(template, cancellationToken));
        }

        private async Task<HttpResponseMessage> SendWithDnsRetryAsync(HttpRequestMessage template, CancellationToken cancellationToken)
        {
            for (var attempt = 0; ; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var request = await template.Clone())
                {
                    try
                    {
                        return await _client.SendAsync(request, cancellationToken);
                    }
                    catch (HttpRequestException ex) when (IsDnsResolutionFailure(ex) && attempt < DnsRetryDelays.Length)
                    {
                        var delay = DnsRetryDelays[attempt];
                        Logger.LogWarning(LoggingEvents.Api,
                            "DNS resolution failed for {Host}: {Message}. Retrying in {DelaySeconds} seconds (attempt {NextAttempt}/{TotalAttempts})",
                            template.RequestUri?.Host, ex.Message, delay.TotalSeconds, attempt + 2, DnsRetryDelays.Length + 1);
                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }
        }

        private static string SafeUriForLogging(Uri uri)
        {
            return uri?.GetLeftPart(UriPartial.Path) ?? string.Empty;
        }

        private static bool IsDnsResolutionFailure(HttpRequestException exception)
        {
            if (exception.HttpRequestError == HttpRequestError.NameResolutionError)
            {
                return true;
            }

            return IsDnsResolutionFailure((Exception)exception);
        }

        private static bool IsDnsResolutionFailure(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                if (current is SocketException socketException && IsDnsSocketError(socketException.SocketErrorCode))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDnsSocketError(SocketError error)
        {
            return error == SocketError.HostNotFound ||
                   error == SocketError.TryAgain ||
                   error == SocketError.NoData ||
                   error == SocketError.NoRecovery;
        }

        private void AddHeadersBody(Request request, HttpRequestMessage httpRequestMessage)
        {
            // Add the request body. Plex PIN polling requires application/x-www-form-urlencoded
            // values even though the request method is GET, while most Ombi APIs use JSON bodies.
            if (request.JsonBody != null)
            {
                LogDebugContent("REQUEST: " + request.JsonBody);
                httpRequestMessage.Content = new JsonContent(request.JsonBody);
                httpRequestMessage.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json"); // Emby connect fails if we have the charset in the header
            }
            else if (request.FormBody.Count > 0)
            {
                httpRequestMessage.Content = new FormUrlEncodedContent(request.FormBody);
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
