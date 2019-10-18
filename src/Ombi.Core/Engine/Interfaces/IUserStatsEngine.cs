using System.Threading.Tasks;

namespace Ombi.Core.Engine
{
    public interface IUserStatsEngine
    {
        Task<UserStatsSummary> GetSummary(SummaryRequest request);
    }
}