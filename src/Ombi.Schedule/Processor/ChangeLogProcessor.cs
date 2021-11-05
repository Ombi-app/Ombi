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
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Branch = Ombi.Settings.Settings.Models.Branch;

namespace Ombi.Schedule.Processor
{
    public class ChangeLogProcessor : IChangeLogProcessor
    {
        private readonly ISettingsService<OmbiSettings> _ombiSettingsService;

        public ChangeLogProcessor(ISettingsService<OmbiSettings> ombiSettings)
        {
            _ombiSettingsService = ombiSettings;
        }

        public async Task<UpdateModel> Process()
        {
            var release = new Release
            {
                Downloads = new List<Downloads>()
            };
            var settings = _ombiSettingsService.GetSettingsAsync();
            await GetGitubRelease(release, settings);

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

        private async Task GetGitubRelease(Release release, Task<OmbiSettings> settingsTask)
        {
            var client = new GitHubClient(Octokit.ProductHeaderValue.Parse("OmbiV4"));

            var releases = await client.Repository.Release.GetAll("ombi-app", "ombi");

            var settings = await settingsTask;

            var latest = settings.Branch switch
            {
                Branch.Develop => releases.Where(x => x.Prerelease).OrderByDescending(x => x.CreatedAt).FirstOrDefault(),
                Branch.Stable => releases.Where(x => !x.Prerelease).OrderByDescending(x => x.CreatedAt).FirstOrDefault(),
                _ => throw new NotImplementedException(),
            };

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