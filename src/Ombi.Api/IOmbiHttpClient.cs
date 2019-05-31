using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Api
{
    public interface IOmbiHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
        Task<string> GetStringAsync(Uri requestUri);
    }
}