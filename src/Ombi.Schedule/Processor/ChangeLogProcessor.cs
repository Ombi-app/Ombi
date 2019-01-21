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
        public ChangeLogProcessor(IApi api, IOmbiHttpClient client)
        {
            _api = api;
            _client = client;
        }

        private readonly IApi _api;
        private readonly IOmbiHttpClient _client;
        private const string _changeLogUrl = "https://raw.githubusercontent.com/tidusjar/Ombi/{0}/CHANGELOG.md";
        private const string AppveyorApiUrl = "https://ci.appveyor.com/api";
        private string ChangeLogUrl(string branch) => string.Format(_changeLogUrl, branch);

        public async Task<UpdateModel> Process(string branch)
        {
            var masterBranch = branch.Equals("master", StringComparison.CurrentCultureIgnoreCase);
            string githubChangeLog;

            githubChangeLog = await _client.GetStringAsync(new Uri(ChangeLogUrl(branch)));


            var html = Markdown.ToHtml(githubChangeLog);


            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode latestRelease;
            if (masterBranch)
            {
                latestRelease = doc.DocumentNode.Descendants("h2")
                    .FirstOrDefault(x => x.InnerText != "(unreleased)");
            }
            else
            {
                latestRelease = doc.DocumentNode.Descendants("h2")
                    .FirstOrDefault(x => x.InnerText == "(unreleased)");

                if (latestRelease == null)
                {
                    latestRelease = doc.DocumentNode.Descendants("h2")
                        .FirstOrDefault(x => x.InnerText != "(unreleased)");
                }
            }

            var newFeatureList = latestRelease.NextSibling.NextSibling.NextSibling.NextSibling;
            var featuresString = newFeatureList.ChildNodes.Where(x => x.Name != "#text").Select(x => x.InnerText.Replace("\\n", "")).ToList();
            var fixes = newFeatureList.NextSibling.NextSibling.NextSibling.NextSibling;
            var fixesString = fixes.ChildNodes.Where(x => x.Name != "#text").Select(x => x.InnerText.Replace("\\n", "")).ToList();

            // Cleanup
            var featuresList = featuresString.Distinct().ToList();
            var fixesList = fixesString.Distinct().ToList();

            // Get release
            var release = new Release
            {
                Version = latestRelease.InnerText,
                Features = featuresList,
                Fixes = fixesList,
                Downloads = new List<Downloads>()
            };

            if (masterBranch)
            {
                var releaseTag = latestRelease.InnerText.Substring(0, 9);
                await GetGitubRelease(release, releaseTag);
            }
            else
            {
                // Get AppVeyor
                await GetAppVeyorRelease(release, branch);
            }


            return TransformUpdate(release,!masterBranch);

        }

        private UpdateModel TransformUpdate(Release release, bool develop)
        {
            var newUpdate = new UpdateModel
            {
                UpdateVersionString = develop ? release.Version : release.Version.Substring(1,8),
                UpdateVersion = release.Version == "(unreleased)" ? 0 : int.Parse(release.Version.Substring(1, 5).Replace(".", "")),
                UpdateDate = DateTime.Now,
                ChangeLogs = new List<ChangeLog>(),
                Downloads = new List<Downloads>()
            };

            foreach (var dl in release.Downloads)
            {
                newUpdate.Downloads.Add(new Downloads
                {
                    Name = dl.Name,
                    Url = dl.Url
                });
            }

            foreach (var f in release.Features)
            {
                var change = new ChangeLog
                {
                    Descripion = f,
                    Type = "New",
                };

                newUpdate.ChangeLogs.Add(change);
            }

            foreach (var f in release.Fixes)
            {
                var change = new ChangeLog
                {
                    Descripion = f,
                    Type = "Fixed",
                };

                newUpdate.ChangeLogs.Add(change);
            }

            return newUpdate;
        }

        private async Task GetAppVeyorRelease(Release release, string branch)
        {
            var request = new Request($"/projects/tidusjar/requestplex/branch/{branch}", AppVeyorApi.AppveyorApiUrl, HttpMethod.Get);
            request.ApplicationJsonContentType();

            var builds = await _api.Request<AppveyorBranchResult>(request);
            var jobId = builds.build.jobs.FirstOrDefault()?.jobId ?? string.Empty;

            if (builds.build.finished == DateTime.MinValue || builds.build.status.Equals("failed"))
            {
                return;
            }
            release.Version = builds.build.version;
            // get the artifacts
            request = new Request($"/buildjobs/{jobId}/artifacts", AppVeyorApi.AppveyorApiUrl, HttpMethod.Get);
            request.ApplicationJsonContentType();

            var artifacts = await _api.Request<List<BuildArtifacts>>(request);

            foreach (var item in artifacts)
            {
                var d = new Downloads
                {
                    Name = item.fileName,
                    Url = $"{AppveyorApiUrl}/buildjobs/{jobId}/artifacts/{item.fileName}"
                };
                release.Downloads.Add(d);
            }
        }

        private async Task GetGitubRelease(Release release, string releaseTag)
        {
            var client = new GitHubClient(Octokit.ProductHeaderValue.Parse("OmbiV3"));

            var releases = await client.Repository.Release.GetAll("tidusjar", "ombi");
            var latest = releases.FirstOrDefault(x => x.TagName.Equals(releaseTag, StringComparison.InvariantCultureIgnoreCase));
            if (latest.Name.Contains("V2", CompareOptions.IgnoreCase))
            {
                latest = null;
            }
            if (latest == null)
            {
                latest = releases.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
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
        }
    }
    public class Release
    {
        public string Version { get; set; }
        public string CheckinVersion { get; set; }
        public List<Downloads> Downloads { get; set; }
        public List<string> Features { get; set; }
        public List<string> Fixes { get; set; }
    }

    public class Downloads
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}