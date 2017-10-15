using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Couchpotato
{
    public interface ICouchPotatoCacher
    {
        Task Start();
    }
}