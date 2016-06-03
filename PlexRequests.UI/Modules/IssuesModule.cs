using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Security;

using NLog;

using PlexRequests.Core;
using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
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

            Get["/{id}", true] = async (x, ct) => await Details(x.id);

            Post["/issue", true] = async (x, ct) => await ReportIssue((int)Request.Form.requestId, (IssueState)(int)Request.Form.issue, null);

            Get["/inprogress", true] = async (x, ct) => await GetInProgressIssues(IssueStatus.InProgressIssue);
            Get["/pending", true] = async (x, ct) => await GetInProgressIssues(IssueStatus.PendingIssue);
            Get["/resolved", true] = async (x, ct) => await GetInProgressIssues(IssueStatus.ResolvedIssue);

            Post["/remove", true] = async (x, ct) => await RemoveIssue((int)Request.Form.issueId);

            Get["/issuecount", true] = async (x, ct) => await IssueCount();

            Post["/issuecomment", true] = async (x, ct) => await ReportIssue((int)Request.Form.requestId, IssueState.Other, (string)Request.Form.commentArea);
        }

        private IIssueService IssuesService { get; }
        private IRequestService RequestService { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public Negotiator Index()
        {
            return View["Index"];
        }

        private async Task<Response> GetInProgressIssues(IssueStatus status)
        {
            var issues = await IssuesService.GetAllAsync();

            return Response.AsJson(issues.Where(x => x.IssueStatus == status));
        }

        public async Task<Response> IssueCount()
        {
            var issues = await IssuesService.GetAllAsync();

            var myIssues = await FilterIssues(issues);

            var count = myIssues.Count();

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
                Type = request.Type,
                IssueStatus = IssueStatus.PendingIssue
            };
            issues.Issues.Add(model);

            var issueId = await IssuesService.AddIssueAsync(issues);

            request.IssueId = issueId;
            await RequestService.UpdateRequestAsync(request);


            return Response.AsJson(new JsonResponseModel { Result = true });
        }

        private async Task<IEnumerable<IssueModel>> FilterIssues(IEnumerable<IssuesModel> issues)
        {
            var settings = await PlexRequestSettings.GetSettingsAsync();
            IEnumerable<IssueModel> myIssues;
            if (IsAdmin)
            {
                myIssues = issues.Where(x => x.Deleted == false).SelectMany(i => i.Issues);
            }
            else if (settings.UsersCanViewOnlyOwnRequests)
            {
                myIssues = (from issuesModel in issues
                            from i in issuesModel.Issues
                            where i.UserReported.Equals(Username, StringComparison.CurrentCultureIgnoreCase)
                            select i).ToList();
            }
            else
            {
                myIssues = issues.Where(x => x.Deleted == false).SelectMany(i => i.Issues);
            }

            return myIssues;
        }
        private async Task<Response> RemoveIssue(int issueId)
        {
            try
            {
                this.RequiresClaims(UserClaims.PowerUser);

                await IssuesService.DeleteIssueAsync(issueId);

                return Response.AsJson(new JsonResponseModel {Result = true, Message = "Issue Removed"});
            }
            catch (Exception e)
            {
                Log.Error(e);
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Looks like we couldn't remove the issue. Check the logs!" });
            }

        }


    }
}
