using System;
using System.Threading.Tasks;
using Nancy.Session;
using Octokit;
using Ombi.Core.Models;

namespace Ombi.Core
{
    public interface IStatusChecker
    {
        Task<StatusModel> GetStatus();
        Task<Issue> ReportBug(string title, string body, string oauthToken);
        Task<Uri> OAuth(string url, ISession session);
        Task<OauthToken> OAuthAccessToken(string code);
    }
}