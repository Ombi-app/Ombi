﻿using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api
{
    public interface IOmbiHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}