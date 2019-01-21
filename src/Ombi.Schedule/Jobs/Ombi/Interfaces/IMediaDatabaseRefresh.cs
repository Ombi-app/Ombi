using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Plex.Interfaces
{
    public interface IMediaDatabaseRefresh : IBaseJob
    {
        Task Start();
    }
}