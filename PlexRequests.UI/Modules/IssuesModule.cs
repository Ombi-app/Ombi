using System.Linq;
using System.Threading.Tasks;

using Nancy;
using Nancy.Responses.Negotiation;

using PlexRequests.Core;
using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
{
    public class IssuesModule : BaseAuthModule
    {
        public IssuesModule(ISettingsService<PlexRequestSettings> pr, IIssueService issueService, IRequestService request) : base("issues", pr)
        {
            IssuesService = issueService;
            RequestService = request;

            Get["/"] = x => Index();

            Get["/issuecount", true] = async (x, ct) => await IssueCount();

            Get["/details/{id}", true] = async (x, ct) => await Details(x.id);

            Post["/issue", true] = async (x, ct) => await ReportIssue((int)Request.Form.requestId, (IssueState)(int)Request.Form.issue, null);
            Post["/issuecomment", true] = async (x, ct) => await ReportIssue((int)Request.Form.requestId, IssueState.Other, (string)Request.Form.commentArea);
        }

        private IIssueService IssuesService { get; }
        private IRequestService RequestService { get; }

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

        private async Task<Response> ReportIssue(int requestId, IssueState issue, string comment)
        {

            var model = new IssueModel
            {
                Issue = issue,
                IssueStatus = IssueStatus.PendingIssue,
                UserReported = Username,
                UserNote = !string.IsNullOrEmpty(comment)
                ? $"{Username} - {comment}"
                : string.Empty,
            };

            var request = await RequestService.GetAsync(requestId);

            var issueEntity = await IssuesService.GetAllAsync();
            var existingIssue = issueEntity.FirstOrDefault(x => x.RequestId == requestId);

            // An issue already exists
            if (existingIssue != null)
            {
                existingIssue.Issues.Add(model);
                var result = await IssuesService.UpdateIssueAsync(existingIssue);

                return Response.AsJson(result
                    ? new JsonResponseModel { Result = true }
                    : new JsonResponseModel { Result = false });
            }

            // New issue
            var issues = new IssuesModel
            {
                Title = request.Title,
                PosterUrl = request.PosterPath,
                RequestId = requestId,
                Type = request.Type
            };
            issues.Issues.Add(model);

            var issueId = await IssuesService.AddIssueAsync(issues);

            request.IssueId = issueId;
            await RequestService.UpdateRequestAsync(request);


            return Response.AsJson(new JsonResponseModel { Result = true });
        }
    }
}
