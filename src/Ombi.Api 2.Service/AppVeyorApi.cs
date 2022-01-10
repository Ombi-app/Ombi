using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api.Service
{
    public class AppVeyorApi : IAppVeyorApi
    {
        public const string AppveyorApiUrl = "https://ci.appveyor.com/api";

        public AppVeyorApi(IApi api)
        {
            _api = api;
        }

        private readonly IApi _api;
        public async Task<AppveyorProjects> GetProjectHistory(string branchName, int records = 10)
        {
            var request = new Request($"projects/tidusjar/requestplex/history?recordsNumber={records}&branch={branchName}", AppveyorApiUrl, HttpMethod.Get);

            request.ApplicationJsonContentType();

            return await _api.Request<AppveyorProjects>(request);
        }

        public class AppveyorProjects
        {
            public Project project { get; set; }
            public Build[] builds { get; set; }
        }

        public class Project
        {
            public int projectId { get; set; }
            public int accountId { get; set; }
            public string accountName { get; set; }
            public object[] builds { get; set; }
            public string name { get; set; }
            public string slug { get; set; }
            public string repositoryType { get; set; }
            public string repositoryScm { get; set; }
            public string repositoryName { get; set; }
            public bool isPrivate { get; set; }
            public bool skipBranchesWithoutAppveyorYml { get; set; }
            public bool enableSecureVariablesInPullRequests { get; set; }
            public bool enableSecureVariablesInPullRequestsFromSameRepo { get; set; }
            public bool enableDeploymentInPullRequests { get; set; }
            public bool rollingBuilds { get; set; }
            public bool alwaysBuildClosedPullRequests { get; set; }
            public string tags { get; set; }
            public Securitydescriptor securityDescriptor { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
        }

        public class Securitydescriptor
        {
            public Accessrightdefinition[] accessRightDefinitions { get; set; }
            public Roleace[] roleAces { get; set; }
        }

        public class Accessrightdefinition
        {
            public string name { get; set; }
            public string description { get; set; }
        }

        public class Roleace
        {
            public int roleId { get; set; }
            public string name { get; set; }
            public bool isAdmin { get; set; }
            public Accessright[] accessRights { get; set; }
        }

        public class Accessright
        {
            public string name { get; set; }
            public bool allowed { get; set; }
        }

        public class Build
        {
            public int buildId { get; set; }
            public object[] jobs { get; set; }
            public int buildNumber { get; set; }
            public string version { get; set; }
            public string message { get; set; }
            public string messageExtended { get; set; }
            public string branch { get; set; }
            public bool isTag { get; set; }
            public string commitId { get; set; }
            public string authorName { get; set; }
            public string authorUsername { get; set; }
            public string committerName { get; set; }
            public string committerUsername { get; set; }
            public DateTime committed { get; set; }
            public object[] messages { get; set; }
            public string status { get; set; }
            public DateTime started { get; set; }
            public DateTime finished { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public string pullRequestId { get; set; }
            public string pullRequestName { get; set; }
        }
    }
}