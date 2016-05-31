using System.Linq;
using System.Threading.Tasks;

using Nancy;
using Nancy.Responses.Negotiation;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.Modules
{
    public class IssuesModule : BaseAuthModule
    {
        public IssuesModule(ISettingsService<PlexRequestSettings> pr, IIssueService issueService) : base("issues", pr)
        {
            IssuesService = issueService;

            Get["/"] = x => Index();

            Get["/issuecount", true] = async (x, ct) => await IssueCount();

            Get["/details/{id}", true] = async (x, ct) => await Details(x.id);
        }

        private IIssueService IssuesService { get; }

        public Negotiator Index()
        {
            return View["Index"];
        }

        public async Task<Response> IssueCount()
        {
            var issues = await IssuesService.GetAllAsync();
            var count = issues.Count(x => x.Deleted == false);

            return Response.AsJson(count);
        }

        public async Task<Negotiator> Details(int id)
        {
            var issue = await IssuesService.GetAsync(id);

            return issue == null 
                ? Index() 
                : View["Details", issue];
        }


    }
}
