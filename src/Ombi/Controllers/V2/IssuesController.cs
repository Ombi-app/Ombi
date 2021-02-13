using Microsoft.AspNetCore.Mvc;
using Ombi.Core.Engine.V2;
using Ombi.Store.Entities.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Controllers.V2
{
    public class IssuesController : V2Controller
    {
        private readonly IIssuesEngine _engine;

        public IssuesController(IIssuesEngine engine)
        {
            _engine = engine;
        }

        [HttpGet("{position}/{take}/{status}")]
        public Task<IEnumerable<IssuesSummaryModel>> GetIssuesSummary(int position, int take, IssueStatus status)
        {
            return _engine.GetIssues(position, take, status, HttpContext.RequestAborted);
        }
    }
}
