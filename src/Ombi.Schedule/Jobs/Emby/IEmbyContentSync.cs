using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Emby
{
    public interface IEmbyContentSync : IBaseJob
    {
        Task Start();
    }
}