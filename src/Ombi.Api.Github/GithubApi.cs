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
            var request = new Request("repos/leram84/layer.Cake/contents/ombi/themes", BaseUrl, HttpMethod.Get);
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddHeader("User-Agent", "Ombi");
            return await _api.Request<List<CakeThemes>>(request);
        }
    }
}
