using System.Threading.Tasks;
using Octokit;
using Ombi.Core.Models;

namespace Ombi.Core
{
    public interface IStatusChecker
    {
        Task<StatusModel> GetStatus();
        Task<Issue> ReportBug(string title, string body);
    }
}