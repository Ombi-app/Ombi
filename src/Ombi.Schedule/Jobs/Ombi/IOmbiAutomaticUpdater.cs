using System.Threading.Tasks;
using Hangfire.Server;

namespace Ombi.Schedule.Ombi
{
    public interface IOmbiAutomaticUpdater
    {
        Task Update(PerformContext context);
        string[] GetVersion();
        Task<bool> UpdateAvailable(string branch, string currentVersion);
    }
}