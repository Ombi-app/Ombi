using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Plex
{
    public interface IPlexEpisodeSync
    {
        Task Start();
    }
}