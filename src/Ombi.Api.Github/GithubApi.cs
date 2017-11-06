using System;
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

        public async Task<CakeThemesContainer> GetCakeThemes()
        {
            var request = new Request("repos/leram84/layer.Cake/contents/Themes", BaseUrl, HttpMethod.Get);

            return await _api.Request<CakeThemesContainer>(request);
        }
    }
}
