#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: StatusChecker.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using PlexRequests.Api;
using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using RestSharp;

namespace PlexRequests.Core.StatusChecker
{
    public class StatusChecker : IStatusChecker
    {
        public StatusChecker(ISettingsService<SystemSettings> ss)
        {
            SystemSettings = ss;
            Git = new GitHubClient(new ProductHeaderValue("PlexRequests-StatusChecker"));
        }

        private ISettingsService<SystemSettings> SystemSettings { get; }

        private IGitHubClient Git { get; }
        private const string Owner = "tidusjar";
        private const string RepoName = "PlexRequests.Net";
        private const string AppveyorApiUrl = "https://ci.appveyor.com/api";

        private const string Api =
            "48Ku58C0794nBrXra8IxWav+dc6NqgkRw+PZB3/bQwbt/D0IrnJQkgtjzo0bd6nkooLMKsC8M+Ab7jyBO+ROjY14VRuxffpDopX9r0iG/fjBl6mZVvqkm+VTDNstDtzp";

        public async Task<StatusModel> GetStatus()
        {
            var settings = await SystemSettings.GetSettingsAsync();
            var stable = settings.Branch == Branches.Stable;

            if (!stable)
            {
                // Early Access Preview Releases
                return GetAppveyorRelease(settings.Branch);
            }

            // Stable releases
            return await GetLatestGithubRelease();
        }

        private async Task<StatusModel> GetLatestGithubRelease()
        {
            var assemblyVersion = AssemblyHelper.GetProductVersion();
            var model = new StatusModel
            {
                CurrentVersion = assemblyVersion,
            };

            var releases = await Git.Repository.Release.GetAll(Owner, RepoName);
            var latestRelease = releases.FirstOrDefault();

            if (latestRelease == null)
            {
                return new StatusModel { NewVersion = "Unknown" };
            }
            var latestVersionArray = latestRelease.Name.Split(new[] { 'v' }, StringSplitOptions.RemoveEmptyEntries);
            var latestVersion = latestVersionArray.Length > 1 ? latestVersionArray[1] : string.Empty;

            if (!latestVersion.Equals(assemblyVersion, StringComparison.InvariantCultureIgnoreCase))
            {
                model.UpdateAvailable = true;
                model.UpdateUri = latestRelease.HtmlUrl;
                model.NewVersion = latestVersion;
            }

            model.ReleaseNotes = latestRelease.Body;
            model.DownloadUri = latestRelease.Assets[0].BrowserDownloadUrl;
            model.ReleaseTitle = latestRelease.Name;

            return model;
        }

        private StatusModel GetAppveyorRelease(Branches branch)
        {
            var request = new ApiRequest();

            // Get latest EAP Build
            var eapBranchRequest = new RestRequest
            {
                Method = Method.GET
            };


            switch (branch)
            {
                case Branches.Dev: 
                    eapBranchRequest.Resource = "/projects/tidusjar/requestplex/branch/dev";
                    break;
                case Branches.EarlyAccessPreview:
                    eapBranchRequest.Resource = "/projects/tidusjar/requestplex/branch/EAP";
                    break;
            }
            
            var api = StringCipher.Decrypt(Api,"Appveyor");
            eapBranchRequest.AddHeader("Authorization", $"Bearer {api}");
            eapBranchRequest.AddHeader("Content-Type", "application/json");
            
            var branchResult = request.ExecuteJson<AppveyorBranchResult>(eapBranchRequest, new Uri(AppveyorApiUrl));

            var jobId = branchResult.build.jobs.FirstOrDefault()?.jobId ?? string.Empty;

            if (string.IsNullOrEmpty(jobId))
            {
                return new StatusModel {UpdateAvailable = false};
            }

            // Get artifacts from the EAP Build
            var eapAtrifactRequest = new RestRequest
            {
                Resource = $"/buildjobs/{jobId}/artifacts",
                Method = Method.GET
            };
            eapAtrifactRequest.AddHeader("Authorization", $"Bearer {api}");
            eapAtrifactRequest.AddHeader("Content-Type", "application/json");

            var artifactResult = request.ExecuteJson<List<AppveyorArtifactResult>>(eapAtrifactRequest, new Uri(AppveyorApiUrl)).FirstOrDefault();

            var downloadLink = $"{AppveyorApiUrl}/buildjobs/{jobId}/artifacts/{artifactResult.fileName}";

            var branchDisplay = EnumHelper<Branches>.GetDisplayValue(branch);
            var fileversion = AssemblyHelper.GetFileVersion();
            
            var model = new StatusModel
            {
                DownloadUri = downloadLink,
                ReleaseNotes = $"{branchDisplay} (See recent commits for details)",
                ReleaseTitle = $"Plex Requests {branchDisplay}",
                NewVersion = branchResult.build.version,
                UpdateUri = downloadLink,
                CurrentVersion = fileversion
            };

            if (!fileversion.Equals(branchResult.build.version, StringComparison.CurrentCultureIgnoreCase))
            {
                model.UpdateAvailable = true;
            }

            return model;
        }
    }
}