using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Ombi.Api
{
    public class Api
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

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
                        if (request.ContentType == ContentType.Json)
                        {
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

        public async Task<string> Request(Request request)
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


                        return await data.ReadAsStringAsync();
                    }
                }
            }
        }
    }
}
