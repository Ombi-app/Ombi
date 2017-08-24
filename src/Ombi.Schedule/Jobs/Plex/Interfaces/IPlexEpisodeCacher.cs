using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Plex
{
    public interface IPlexEpisodeCacher
    {
        Task Start();
    }
}