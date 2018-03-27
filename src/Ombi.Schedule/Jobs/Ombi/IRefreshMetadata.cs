using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Ombi
{
    public interface IRefreshMetadata : IBaseJob
    {
        Task Start();
    }
}