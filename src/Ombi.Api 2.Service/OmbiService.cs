using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ombi.Api.Service.Models;
using Ombi.Helpers;

namespace Ombi.Api.Service
{
    public class OmbiService : IOmbiService
    {
        public OmbiService(IOptions<ApplicationSettings> settings, IApi api, ILogger<OmbiService> log)
        {
            Settings = settings.Value;
            Api = api;
            Logger = log;
        }

        private ApplicationSettings Settings { get; }
        private ILogger<OmbiService> Logger { get; }
        private IApi Api { get; }

        public async Task<Updates> GetUpdates(string branch)
        {
            var request = new Request($"api/update/{branch}", Settings.OmbiService, HttpMethod.Get);
            
            request.ContentHeaders.Add(new KeyValuePair<string, string>("Content-Type", "application/json"));

            return await Api.Request<Updates>(request);
        }
    }
}
