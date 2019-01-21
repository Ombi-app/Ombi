using System.Threading.Tasks;

namespace Ombi.Schedule.Jobs.Ombi
{
    public interface IIssuesPurge : IBaseJob
    {
        Task Start();
    }
}