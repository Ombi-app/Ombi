using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Plex
{
    public interface IPlexRecentlyAddedSync : IBaseJob
    {
        void Start();
    }
}