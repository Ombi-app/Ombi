using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Core.Engine;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [Admin]
    [Authorize]
    [Produces("application/json")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        public StatsController(IUserStatsEngine eng)
        {
            _statsEngine = eng;
        }

        private readonly IUserStatsEngine _statsEngine;

        [HttpGet]
        public async Task<UserStatsSummary> GetUserStats(SummaryRequest req)
        {
            return await _statsEngine.GetSummary(req);
        }
    }
}