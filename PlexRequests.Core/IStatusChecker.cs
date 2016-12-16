using System.Threading.Tasks;
using Octokit;
using PlexRequests.Core.Models;

namespace PlexRequests.Core
{
    public interface IStatusChecker
    {
        Task<StatusModel> GetStatus();
        Task<Issue> ReportBug(string title, string body);
    }
}