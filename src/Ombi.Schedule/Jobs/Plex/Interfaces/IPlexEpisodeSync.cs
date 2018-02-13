using System;
using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Plex.Interfaces
{
    public interface IPlexEpisodeSync : IBaseJob
    {
        Task Start();
    }
}