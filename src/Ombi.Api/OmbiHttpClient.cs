#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: OmbiHttpClient.cs
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;

namespace Ombi.Api
{
    /// <summary>
    /// The purpose of this class is simple, keep one instance of the HttpClient in play.
    /// There are many articles related to when using multiple HttpClient's keeping the socket in a WAIT state
    /// https://blogs.msdn.microsoft.com/alazarev/2017/12/29/disposable-finalizers-and-httpclient/
    /// https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
    /// </summary>
    public class OmbiHttpClient : IOmbiHttpClient
    {
        public OmbiHttpClient(ICacheService cache, ISettingsService<OmbiSettings> s)
        {
            _cache = cache;
            _settings = s;
            _runtimeVersion = AssemblyHelper.GetRuntimeVersion();
        }

        private static HttpClient _client;
        private static HttpMessageHandler _handler;

        private readonly ICacheService _cache;
        private readonly ISettingsService<OmbiSettings> _settings;
        private readonly string _runtimeVersion;


        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            await Setup();
            return await _client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Setup();
            return await _client.SendAsync(request, cancellationToken);
        }

        public async Task<string> GetStringAsync(Uri requestUri)
        {
            await Setup();
            return await _client.GetStringAsync(requestUri);
        }

        private async Task Setup()
        {
            if (_client == null)
            {
                if (_handler == null)
                {
                    // Get the handler
                    _handler = await GetHandler();
                }
                _client = new HttpClient(_handler);
                _client.DefaultRequestHeaders.Add("User-Agent",$"Ombi/{_runtimeVersion} (https://ombi.io/)");
            }
        }

        private async Task<HttpMessageHandler> GetHandler()
        {
            var settings = await _cache.GetOrAdd(CacheKeys.OmbiSettings, async () => await _settings.GetSettingsAsync(), DateTime.Now.AddHours(1));
            if (settings.IgnoreCertificateErrors)
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                };
            }
            return new HttpClientHandler();
        }
    }
}