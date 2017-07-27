using System.Threading.Tasks;

namespace Ombi.Schedule.Ombi
{
    public interface IOmbiAutomaticUpdater
    {
        Task Update();
    }
}