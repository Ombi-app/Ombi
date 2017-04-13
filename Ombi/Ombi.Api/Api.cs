using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ombi.Api
{
    public class Api
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        public async Task<T> Get<T>(Uri uri)
        {
            var h = new HttpClient();
            //h.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            var response = await h.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                // Logging
            }
            var receiveString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(receiveString, Settings);
        }

        public async Task<T> Request<T>(Request request)
        {
            using (var httpClient = new HttpClient())
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
                            // Logging
                        }
                        // do something with the response
                        var data = httpResponseMessage.Content;


                        var receivedString = await data.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T>(receivedString, Settings);
                    }
                }
            }
        }
    }
}
