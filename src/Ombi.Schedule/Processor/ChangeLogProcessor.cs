using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Markdig;
using Octokit;
using Ombi.Api;
using Ombi.Api.Service;
using Ombi.Core.Processor;
using Ombi.Helpers;

namespace Ombi.Schedule.Processor
{
    public class ChangeLogProcessor : IChangeLogProcessor
    {
        public ChangeLogProcessor(IApi api, IHttpClientFactory client)
        {
            _api = api;
            _client = client.CreateClient("OmbiClient");
        }

        private readonly IApi _api;
        private readonly HttpClient _client;
        private const string _changeLogUrl = "https://raw.githubusercontent.com/tidusjar/Ombi/{0}/CHANGELOG.md";
        private const string AppveyorApiUrl = "https://ci.appveyor.com/api";
        private string ChangeLogUrl(string branch) => string.Format(_changeLogUrl, branch);

        public async Task<UpdateModel> Process()
        {
            var release = new Release
            {
                Downloads = new List<Downloads>()
            };

            await GetGitubRelease(release);
            
            return TransformUpdate(release);
        }

        private UpdateModel TransformUpdate(Release release)
        {
            var newUpdate = new UpdateModel
            {
                UpdateVersionString = release.Version,
                UpdateVersion = int.Parse(release.Version.Substring(1, 5).Replace(".", "")),
                UpdateDate = DateTime.Now,
                ChangeLogs = release.Description,
                Downloads = new List<Downloads>(),
                UpdateAvailable = release.Version != "v" + AssemblyHelper.GetRuntimeVersion()
        };

            foreach (var dl in release.Downloads)
            {
                newUpdate.Downloads.Add(new Downloads
                {
                    Name = dl.Name,
                    Url = dl.Url
                });
            }

            return newUpdate;
        }

        private async Task GetGitubRelease(Release release)
        {
            var client = new GitHubClient(Octokit.ProductHeaderValue.Parse("OmbiV4"));

            var releases = await client.Repository.Release.GetAll("ombi-app", "ombi");
            var latest = releases.OrderByDescending(x => x.CreatedAt).FirstOrDefault();

            foreach (var item in latest.Assets)
            {
                var d = new Downloads
                {
                    Name = item.Name,
                    Url = item.BrowserDownloadUrl
                };
                release.Downloads.Add(d);
            }
            release.Description = Markdown.ToHtml(latest.Body);
            release.Version = latest.TagName;
        }
    }
    public class Release
    {
        public string Version { get; set; }
        public string CheckinVersion { get; set; }
        public List<Downloads> Downloads { get; set; }
        public string Description { get; set; }
    }

    public class Downloads
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}