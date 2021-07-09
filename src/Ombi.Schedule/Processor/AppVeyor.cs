using System;
using System.Collections.Generic;
using Ombi.Api.Service;
using Ombi.Schedule.Processor;

namespace Ombi.Core.Processor
{
    public class AppveyorBranchResult
    {
        public Project1 project { get; set; }
        public Build1 build { get; set; }
    }

    public class Project1
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
        public string repositoryBranch { get; set; }
        public bool isPrivate { get; set; }
        public bool skipBranchesWithoutAppveyorYml { get; set; }
        public bool enableSecureVariablesInPullRequests { get; set; }
        public bool enableSecureVariablesInPullRequestsFromSameRepo { get; set; }
        public bool enableDeploymentInPullRequests { get; set; }
        public bool saveBuildCacheInPullRequests { get; set; }
        public bool rollingBuilds { get; set; }
        public bool rollingBuildsDoNotCancelRunningBuilds { get; set; }
        public bool alwaysBuildClosedPullRequests { get; set; }
        public string tags { get; set; }
        public Nugetfeed nuGetFeed { get; set; }
        public AppVeyorApi.Securitydescriptor securityDescriptor { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
    }

    public class Nugetfeed
    {
        public string id { get; set; }
        public string name { get; set; }
        public int accountId { get; set; }
        public int projectId { get; set; }
        public bool publishingEnabled { get; set; }
        public DateTime created { get; set; }
    }


    public class Build1
    {
        public int buildId { get; set; }
        public Job[] jobs { get; set; }
        public int buildNumber { get; set; }
        public string version { get; set; }
        public string message { get; set; }
        public string branch { get; set; }
        public bool isTag { get; set; }
        public string commitId { get; set; }
        public string authorName { get; set; }
        public string committerName { get; set; }
        public DateTime committed { get; set; }
        public object[] messages { get; set; }
        public string status { get; set; }
        public DateTime started { get; set; }
        public DateTime finished { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
    }

    public class Job
    {
        public string jobId { get; set; }
        public string name { get; set; }
        public string osType { get; set; }
        public bool allowFailure { get; set; }
        public int messagesCount { get; set; }
        public int compilationMessagesCount { get; set; }
        public int compilationErrorsCount { get; set; }
        public int compilationWarningsCount { get; set; }
        public int testsCount { get; set; }
        public int passedTestsCount { get; set; }
        public int failedTestsCount { get; set; }
        public int artifactsCount { get; set; }
        public string status { get; set; }
        public DateTime started { get; set; }
        public DateTime finished { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
    }

    public class BuildArtifactsContainer
    {
        public BuildArtifacts[] artifacts { get; set; }
    }

    public class BuildArtifacts
    {
        public string fileName { get; set; }
        public string type { get; set; }
        public int size { get; set; }
    }

    public class UpdateModel
    {
        public string UpdateVersionString { get; set; }
        public int UpdateVersion { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool UpdateAvailable { get; set; }
        public string ChangeLogs { get; set; }
        public List<Downloads> Downloads { get; set; }
    }

    public class ChangeLog
    {
        public string Type { get; set; } // New, Fixed, Updated
        public string Descripion { get; set; }
    }

}