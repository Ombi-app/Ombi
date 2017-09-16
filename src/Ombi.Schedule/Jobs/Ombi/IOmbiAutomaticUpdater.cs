using System.Threading.Tasks;
using Hangfire.Server;

namespace Ombi.Schedule.Ombi
{
    public interface IOmbiAutomaticUpdater
    {
        Task Update(PerformContext context);
    }
}