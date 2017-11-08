using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api.Github.Models;

namespace Ombi.Api.Github
{
    public class GithubApi : IGithubApi
    {
        public GithubApi(IApi api)
        {
            _api = api;
        }

        private readonly IApi _api;
        private const string BaseUrl = "https://api.github.com/";

        public async Task<List<CakeThemes>> GetCakeThemes()
        {
            var request = new Request("repos/tidusjar/layer.Cake/contents/ombi/themes", BaseUrl, HttpMethod.Get);
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddHeader("User-Agent", "Ombi");
            return await _api.Request<List<CakeThemes>>(request);
        }

        public async Task<string> GetThemesRawContent(string url)
        {
            var sections = url.Split('/');
            var lastPart = sections.Last();
            url = url.Replace(lastPart, string.Empty);
            var request = new Request(lastPart, url, HttpMethod.Get);
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddHeader("User-Agent", "Ombi");
            return await _api.RequestContent(request);
        }
    }
}
