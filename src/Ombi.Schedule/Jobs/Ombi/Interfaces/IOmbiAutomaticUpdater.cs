using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Ombi
{
    public interface IOmbiAutomaticUpdater : IBaseJob
    {
        string[] GetVersion();
        Task<bool> UpdateAvailable(string currentVersion);
    }
}