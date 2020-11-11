using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api
{
    public static class HttpRequestExtensions
    {
        public static async Task<HttpRequestMessage> Clone(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = await request.Content.Clone(),
                Version = request.Version
            };
            foreach (KeyValuePair<string, object> prop in request.Options)
            {
                clone.Options.TryAdd(prop.Key, prop.Value);
            }
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        public static async Task<HttpContent> Clone(this HttpContent content)
        {
            if (content == null) return null;

            var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            ms.Position = 0;

            var clone = new StreamContent(ms);
            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
            {
                clone.Headers.Add(header.Key, header.Value);
            }
            return clone;
        }
    }
}