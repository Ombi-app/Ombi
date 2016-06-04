using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;
using Nancy.Security;

using NLog;

using PlexRequests.Core;
using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Store;
using PlexRequests.UI.Helpers;
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

            Get["/inprogress", true] = async (x, ct) => await GetIssues(IssueStatus.InProgressIssue);
            Get["/pending", true] = async (x, ct) => await GetIssues(IssueStatus.PendingIssue);
            Get["/resolved", true] = async (x, ct) => await GetIssues(IssueStatus.ResolvedIssue);

            Post["/remove", true] = async (x, ct) => await RemoveIssue((int)Request.Form.issueId);
            Post["/inprogressUpdate", true] = async (x, ct) => await ChangeStatus((int)Request.Form.issueId, IssueStatus.InProgressIssue);
            Post["/resolvedUpdate", true] = async (x, ct) => await ChangeStatus((int)Request.Form.issueId, IssueStatus.ResolvedIssue);

            Post["/clear", true] = async (x, ct) => await ClearIssue((int) Request.Form.issueId, (IssueState) (int) Request.Form.issue);

            Get["/issuecount", true] = async (x, ct) => await IssueCount();
            Get["/tabCount", true] = async (x, ct) => await TabCount();

            Post["/issuecomment", true] = async (x, ct) => await ReportIssue((int)Request.Form.requestId, IssueState.Other, (string)Request.Form.commentArea);


            Post["/addnote", true] = async (x, ct) => await AddNote((int)Request.Form.requestId, (string)Request.Form.noteArea, (IssueState)(int)Request.Form.issue);
        }

        private IIssueService IssuesService { get; }
        private IRequestService RequestService { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public Negotiator Index()
        {
            return View["Index"];
        }

        private async Task<Response> GetIssues(IssueStatus status)
        {
            var issues = await IssuesService.GetAllAsync();
            issues = await FilterIssues(issues);

            var issuesModels = issues as IssuesModel[] ?? issues.Where(x => x.IssueStatus == status).ToArray();
            var model = issuesModels.Select(i => new IssuesViewModel
            {
                Title = i.Title, Type = i.Type.ToString().CamelCaseToWords(), Count = i.Issues.Count, Id = i.Id, RequestId = i.RequestId
            }).ToList();

            return Response.AsJson(model);
        }

        public async Task<Response> IssueCount()
        {
            var issues = await IssuesService.GetAllAsync();

            var myIssues = await FilterIssues(issues);

            var count = myIssues.Count();

            return Response.AsJson(count);
        }

        public async Task<Response> TabCount()
        {
            var issues = await IssuesService.GetAllAsync();

            var myIssues = await FilterIssues(issues);

            var count = new List<object>();

            var issuesModels = myIssues as IssuesModel[] ?? myIssues.ToArray();
            var pending = issuesModels.Where(x => x.IssueStatus == IssueStatus.PendingIssue);
            var progress = issuesModels.Where(x => x.IssueStatus == IssueStatus.InProgressIssue);
            var resolved = issuesModels.Where(x => x.IssueStatus == IssueStatus.ResolvedIssue);
          
            count.Add(new  { Name = IssueStatus.PendingIssue, Count = pending.Count()});
            count.Add(new  { Name = IssueStatus.InProgressIssue, Count = progress.Count()});
            count.Add(new  { Name = IssueStatus.ResolvedIssue, Count = resolved.Count()});
            
            return Response.AsJson(count);
        }

        public async Task<Negotiator> Details(int id)
        {
            var issue = await IssuesService.GetAsync(id);
            issue = Order(issue);
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
                if (existingIssue.Issues.Any(x => x.Issue == issue))
                {
                    return
                        Response.AsJson(new JsonResponseModel()
                        {
                            Result = false,
                            Message = "This issue has already been reported!"
                        });

                }
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

        private async Task<IEnumerable<IssuesModel>> FilterIssues(IEnumerable<IssuesModel> issues)
        {
            var settings = await PlexRequestSettings.GetSettingsAsync();
            IEnumerable<IssuesModel> myIssues;
            if (IsAdmin)
            {
                myIssues = issues.Where(x => x.Deleted == false);
            }
            else if (settings.UsersCanViewOnlyOwnIssues)
            {
                myIssues =
                    issues.Where(
                        x =>
                            x.Issues.Any(i => i.UserReported.Equals(Username, StringComparison.CurrentCultureIgnoreCase)) && x.Deleted == false);
            }
            else
            {
                myIssues = issues.Where(x => x.Deleted == false);
            }

            return myIssues;
        }
        private async Task<Negotiator> RemoveIssue(int issueId)
        {
            try
            {
                this.RequiresClaims(UserClaims.Admin);

                await IssuesService.DeleteIssueAsync(issueId);

                return View["Index"];
            }
            catch (Exception e)
            {
                Log.Error(e);
                return View["Index"];
            }

        }

        private async Task<Negotiator> ChangeStatus(int issueId, IssueStatus status)
        {
            try
            {
                this.RequiresClaims(UserClaims.Admin);

                var issue = await IssuesService.GetAsync(issueId);
                issue.IssueStatus = status;
                var result = await IssuesService.UpdateIssueAsync(issue);
                return result ? await Details(issueId) : View["Index"];
            }
            catch (Exception e)
            {
                Log.Error(e);
                return View["Index"];
            }

        }


        private async Task<Negotiator> ClearIssue(int issueId, IssueState state)
        {
            this.RequiresClaims(UserClaims.Admin);
            var issue = await IssuesService.GetAsync(issueId);

            var toRemove = issue.Issues.FirstOrDefault(x => x.Issue == state);
            issue.Issues.Remove(toRemove);

            var result = await IssuesService.UpdateIssueAsync(issue);

            return result ? await Details(issueId) : View["Index"];
        }

        private async Task<Response> AddNote(int requestId, string noteArea, IssueState state)
        {
            this.RequiresClaims(UserClaims.Admin);
            var issue = await IssuesService.GetAsync(requestId);
            if (issue == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Issue does not exist to add a note!" });
            }
            var toAddNote = issue.Issues.FirstOrDefault(x => x.Issue == state);

            issue.Issues.Remove(toAddNote);
            toAddNote.AdminNote = noteArea;
            issue.Issues.Add(toAddNote);
            

            var result = await IssuesService.UpdateIssueAsync(issue);
            return Response.AsJson(result
                                       ? new JsonResponseModel { Result = true }
                                       : new JsonResponseModel { Result = false, Message = "Could not update the notes, please try again or check the logs" });
        }

        private IssuesModel Order(IssuesModel issues)
        {
            issues.Issues = issues.Issues.OrderByDescending(x => x.Issue).ToList();
            return issues;
        }
    }
}
