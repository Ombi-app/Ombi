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
using Ombi.Api.External.ExternalApis.Service;
using Ombi.Core.Processor;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Branch = Ombi.Settings.Settings.Models.Branch;

namespace Ombi.Schedule.Processor
{
    public class ChangeLogProcessor : IChangeLogProcessor
    {
        private const string RepositoryOwner = "ExtremeFiretop";
        private const string RepositoryName = "Reqestra";
        private const string ProductHeader = "Reqestra";

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
            await GetGithubRelease(release, settings);

            return TransformUpdate(release);
        }

        private UpdateModel TransformUpdate(Release release)
        {
            Version updateVersion = Version.Parse(release.Version.TrimStart('v'));
            Version currentVersion = Version.Parse(AssemblyHelper.GetRuntimeVersion());
            var newUpdate = new UpdateModel
            {
                UpdateVersionString = release.Version,
                UpdateVersion = int.Parse(release.Version.Substring(1, 5).Replace(".", "")),
                UpdateDate = DateTime.Now,
                ChangeLogs = release.Description,
                Downloads = new List<Downloads>(),
                UpdateAvailable = updateVersion > currentVersion
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

        private async Task GetGithubRelease(Release release, Task<OmbiSettings> settingsTask)
        {
            var client = new GitHubClient(Octokit.ProductHeaderValue.Parse(ProductHeader));

            // Reqestra is now maintained and released independently from upstream Ombi.
            // Both update channels are sourced exclusively from the Reqestra GitHub releases:
            // Stable = normal releases, Develop = prereleases.
            var releases = await client.Repository.Release.GetAll(RepositoryOwner, RepositoryName);

            var settings = await settingsTask;

            var latest = settings.Branch switch
            {
                Branch.Develop => releases.Where(x => x.Prerelease).OrderByDescending(x => x.CreatedAt).FirstOrDefault(),
                Branch.Stable => releases.Where(x => !x.Prerelease).OrderByDescending(x => x.CreatedAt).FirstOrDefault(),
                _ => throw new NotImplementedException(),
            };

            if (latest == null)
            {
                throw new InvalidOperationException(
                    $"No {settings.Branch} release was found in {RepositoryOwner}/{RepositoryName}.");
            }

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