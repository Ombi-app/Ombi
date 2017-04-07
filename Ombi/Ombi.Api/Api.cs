using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ombi.Api
{
    public class Api
    {
        public static JsonSerializerSettings Settings = new JsonSerializerSettings
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
    }
}
