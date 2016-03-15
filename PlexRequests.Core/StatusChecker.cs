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
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using Octokit;

using PlexRequests.Core.Models;
using PlexRequests.Helpers;

namespace PlexRequests.Core
{
    public class StatusChecker
    {
        public StatusChecker()
        {
            Git = new GitHubClient(new ProductHeaderValue("PlexRequests-StatusChecker"));
        }
        private IGitHubClient Git { get; }
        private const string Owner = "tidusjar";
        private const string RepoName = "PlexRequests.Net";

        public async Task<Release> GetLatestRelease()
        {
            var releases = await Git.Repository.Release.GetAll(Owner, RepoName);
            return releases.FirstOrDefault();
        }

        public StatusModel GetStatus()
        {
            var assemblyVersion = AssemblyHelper.GetAssemblyVersion();
            var model = new StatusModel
            {
                Version = assemblyVersion,
            };

            var latestRelease = GetLatestRelease();

            var latestVersionArray = latestRelease.Result.Name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var latestVersion = latestVersionArray.Length > 1 ? latestVersionArray[1] : string.Empty;

            if (!latestVersion.Equals(AssemblyHelper.GetReleaseVersion(), StringComparison.InvariantCultureIgnoreCase))
            {
                model.UpdateAvailable = true;
                model.UpdateUri = latestRelease.Result.HtmlUrl;
            } 

            return model;
        }
    }
}