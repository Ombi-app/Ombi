using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses.Negotiation;
using NLog;
using Ombi.Api;
using Ombi.Core;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Permissions;
using Ombi.Services.Interfaces;
using Ombi.Services.Notification;
using Ombi.Store;
using Ombi.UI.Models;
using ISecurityExtensions = Ombi.Core.ISecurityExtensions;

namespace Ombi.UI.Modules
{
    public class IssuesModule : BaseAuthModule
    {
        public IssuesModule(ISettingsService<PlexRequestSettings> pr, IIssueService issueService, IRequestService request, INotificationService n, ISecurityExtensions security) : base("issues", pr, security)
        {
            IssuesService = issueService;
            RequestService = request;
            NotificationService = n;

            Get["/"] = x => Index();

            Get["/{id}", true] = async (x, ct) => await Details(x.id);

            Post["/issue", true] = async (x, ct) => await ReportRequestIssue((int)Request.Form.requestId, (IssueState)(int)Request.Form.issue, null);

            Get["/pending", true] = async (x, ct) => await GetIssues(IssueStatus.PendingIssue);
            Get["/resolved", true] = async (x, ct) => await GetIssues(IssueStatus.ResolvedIssue);

            Post["/remove", true] = async (x, ct) => await RemoveIssue((int)Request.Form.issueId);
            Post["/resolvedUpdate", true] = async (x, ct) => await ChangeStatus((int)Request.Form.issueId, IssueStatus.ResolvedIssue);

            Post["/clear", true] = async (x, ct) => await ClearIssue((int)Request.Form.issueId, (IssueState)(int)Request.Form.issue);

            Get["/issuecount", true] = async (x, ct) => await IssueCount();
            Get["/tabCount", true] = async (x, ct) => await TabCount();

            Post["/issuecomment", true] = async (x, ct) => await ReportRequestIssue((int)Request.Form.providerId, IssueState.Other, (string)Request.Form.commentArea);

            Post["/nonrequestissue", true] = async (x, ct) => await ReportNonRequestIssue((int)Request.Form.providerId, (string)Request.Form.type, (IssueState)(int)Request.Form.issue, null);

            Post["/nonrequestissuecomment", true] = async (x, ct) => await ReportNonRequestIssue((int)Request.Form.providerId, (string)Request.Form.type, IssueState.Other, (string)Request.Form.commentArea);


            Post["/addnote", true] = async (x, ct) => await AddNote((int)Request.Form.requestId, (string)Request.Form.noteArea, (IssueState)(int)Request.Form.issue);
        }

        private IIssueService IssuesService { get; }
        private IRequestService RequestService { get; }
        private INotificationService NotificationService { get; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public Negotiator Index()
        {
            return View["Index"];
        }

        private async Task<Response> GetIssues(IssueStatus status)
        {
            var issues = await IssuesService.GetAllAsync();
            issues = await FilterIssuesAsync(issues, status == IssueStatus.ResolvedIssue);

            var issuesModels = issues as IssuesModel[] ?? issues.Where(x => x.IssueStatus == status).ToArray();
            var viewModel = new List<IssuesViewModel>();

            foreach (var i in issuesModels)
            {
                var model = new IssuesViewModel { Id = i.Id, RequestId = i.RequestId, Title = i.Title, Type = i.Type.ToString().ToCamelCaseWords(), Admin = Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests)
            };

                // Create a string with all of the current issue states with a "," delimiter in e.g. Wrong Content, Playback Issues
                var state = i.Issues.Select(x => x.Issue).ToArray();
                var issueState = string.Empty;
                for (var j = 0; j < state.Length; j++)
                {
                    var word = state[j].ToString().ToCamelCaseWords();
                    if (j != state.Length - 1)
                    {
                        issueState += $"{word}, ";
                    }
                    else
                    {
                        issueState += word;
                    }
                }
                model.Issues = issueState;

                viewModel.Add(model);
            }

            return Response.AsJson(viewModel);
        }

        public async Task<Response> IssueCount()
        {
            var issues = await IssuesService.GetAllAsync();

            var myIssues = await FilterIssuesAsync(issues);

            var count = myIssues.Count();

            return Response.AsJson(count);
        }

        public async Task<Response> TabCount()
        {
            var issues = await IssuesService.GetAllAsync();

            var myIssues = await FilterIssuesAsync(issues);

            var count = new List<object>();

            var issuesModels = myIssues as IssuesModel[] ?? myIssues.ToArray();
            var pending = issuesModels.Where(x => x.IssueStatus == IssueStatus.PendingIssue);
            var resolved = issuesModels.Where(x => x.IssueStatus == IssueStatus.ResolvedIssue);

            count.Add(new { Name = IssueStatus.PendingIssue, Count = pending.Count() });
            count.Add(new { Name = IssueStatus.ResolvedIssue, Count = resolved.Count() });

            return Response.AsJson(count);
        }

        public async Task<Negotiator> Details(int id)
        {
            var issue = await IssuesService.GetAsync(id);
            if (issue == null)
                return Index();

            issue = Order(issue);
            var m = new IssuesDetailsViewModel
            {
                Issues = issue.Issues,
                RequestId = issue.RequestId,
                Title = issue.Title,
                IssueStatus = issue.IssueStatus,
                Deleted = issue.Deleted,
                Type = issue.Type,
                ProviderId = issue.ProviderId,
                PosterUrl = issue.PosterUrl,
                Id = issue.Id
            };
            return View["Details", m];
        }

        private async Task<Response> ReportRequestIssue(int requestId, IssueState issue, string comment)
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

            var notifyModel = new NotificationModel
            {
                User = Username,
                NotificationType = NotificationType.Issue,
                Title = request.Title,
                DateTime = DateTime.Now,
                Body = issue == IssueState.Other ? comment : issue.ToString().ToCamelCaseWords()
            };

            // An issue already exists
            if (existingIssue != null)
            {
                if (existingIssue.Issues.Any(x => x.Issue == issue))
                {
                    return
                        Response.AsJson(new JsonResponseModel
                        {
                            Result = false,
                            Message = "This issue has already been reported!"
                        });

                }
                existingIssue.Issues.Add(model);
                var result = await IssuesService.UpdateIssueAsync(existingIssue);


                await NotificationService.Publish(notifyModel);

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

            await NotificationService.Publish(notifyModel);
            return Response.AsJson(new JsonResponseModel { Result = true });
        }

        private async Task<Response> ReportNonRequestIssue(int providerId, string type, IssueState issue, string comment)
        {
            var currentIssues = await IssuesService.GetAllAsync();
            var notifyModel = new NotificationModel
            {
                User = Username,
                NotificationType = NotificationType.Issue,
                DateTime = DateTime.Now,
                Body = issue == IssueState.Other ? comment : issue.ToString().ToCamelCaseWords()
            };
            var model = new IssueModel
            {
                Issue = issue,
                UserReported = Username,
                UserNote = !string.IsNullOrEmpty(comment)
                ? $"{Username} - {comment}"
                : string.Empty,
            };

            var existing = currentIssues.FirstOrDefault(x => x.ProviderId == providerId && !x.Deleted && x.IssueStatus == IssueStatus.PendingIssue);
            if (existing != null)
            {
                existing.Issues.Add(model);
                await IssuesService.UpdateIssueAsync(existing);
                return Response.AsJson(new JsonResponseModel { Result = true });
            }

            if (type == "movie")
            {
                var movieApi = new TheMovieDbApi();

                var result = await movieApi.GetMovieInformation(providerId);
                if (result != null)
                {
                    notifyModel.Title = result.Title;
                    // New issue
                    var issues = new IssuesModel
                    {
                        Title = result.Title,
                        PosterUrl = "https://image.tmdb.org/t/p/w150/" + result.PosterPath,
                        ProviderId = providerId,
                        Type = RequestType.Movie,
                        IssueStatus = IssueStatus.PendingIssue
                    };
                    issues.Issues.Add(model);

                    var issueId = await IssuesService.AddIssueAsync(issues);

                    await NotificationService.Publish(notifyModel);
                    return Response.AsJson(new JsonResponseModel { Result = true });
                }
            }

            if (type == "tv")
            {
                var tv = new TvMazeApi();
                var result = tv.ShowLookupByTheTvDbId(providerId);
                if (result != null)
                {
                    var banner = result.image?.medium;
                    if (!string.IsNullOrEmpty(banner))
                    {
                        banner = banner.Replace("http", "https");
                    }

                    notifyModel.Title = result.name;
                    // New issue
                    var issues = new IssuesModel
                    {
                        Title = result.name,
                        PosterUrl = banner,
                        ProviderId = providerId,
                        Type = RequestType.TvShow,
                        IssueStatus = IssueStatus.PendingIssue
                    };
                    issues.Issues.Add(model);

                    var issueId = await IssuesService.AddIssueAsync(issues);

                    await NotificationService.Publish(notifyModel);
                    return Response.AsJson(new JsonResponseModel { Result = true });
                }
            }



            return Response.AsJson(new JsonResponseModel { Result = false, Message = "Album Reports are not supported yet!"});
        }

        /// <summary>
        /// Filters the issues. Checks to see if we have set <c>UsersCanViewOnlyOwnIssues</c> in the database and filters upon the user logged in and that setting.
        /// </summary>
        /// <param name="issues">The issues.</param>
        private async Task<IEnumerable<IssuesModel>> FilterIssuesAsync(IEnumerable<IssuesModel> issues, bool showResolved = false)
        {
            var settings = await PlexRequestSettings.GetSettingsAsync();
            IEnumerable<IssuesModel> myIssues;

            // Is the user an Admin? If so show everything
            if (IsAdmin)
            {
                var issuesModels = issues as IssuesModel[] ?? issues.ToArray();
                myIssues = issuesModels.Where(x => x.Deleted == false);
                if (!showResolved)
                {
                    myIssues = issuesModels.Where(x => x.IssueStatus != IssueStatus.ResolvedIssue);
                }
            }
            else if (Security.HasPermissions(User, Permissions.UsersCanViewOnlyOwnIssues)) // The user is not an Admin, do we have the settings to hide them?
            {
                if (!showResolved)
                {
                    myIssues =
                        issues.Where(
                            x =>
                            x.Issues.Any(i => i.UserReported.Equals(Username, StringComparison.CurrentCultureIgnoreCase)) && x.Deleted == false
                            && x.IssueStatus != IssueStatus.ResolvedIssue);
                }
                else
                {
                    myIssues =
                        issues.Where(
                            x =>
                                x.Issues.Any(i => i.UserReported.Equals(Username, StringComparison.CurrentCultureIgnoreCase)) && x.Deleted == false);
                }
            }
            else // Looks like the user is not an admin and there is no settings set.
            {
                var issuesModels = issues as IssuesModel[] ?? issues.ToArray();
                myIssues = issuesModels.Where(x => x.Deleted == false);
                if (!showResolved)
                {
                    myIssues = issuesModels.Where(x => x.IssueStatus != IssueStatus.ResolvedIssue);
                }
            }

            return myIssues;
        }
        private async Task<Response> RemoveIssue(int issueId)
        {
            try
            {
                if (!Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests))
                {
                    return Response.AsJson(new JsonResponseModel { Result = false, Message = "Sorry, you do not have the correct permissions to remove an issue." });
                }

                var issue = await IssuesService.GetAsync(issueId);
                var request = await RequestService.GetAsync(issue.RequestId);
                if (request.Id > 0)
                {
                    request.IssueId = 0; // No issue;

                    var result = await RequestService.UpdateRequestAsync(request);
                    if (result)
                    {
                        await IssuesService.DeleteIssueAsync(issueId);
                    }
                }
                else
                {
                    await IssuesService.DeleteIssueAsync(issueId);
                }

                return Response.AsJson(new JsonResponseModel { Result = true });

            }
            catch (Exception e)
            {
                Log.Error(e);
                return Response.AsJson(new JsonResponseModel() { Result = false, Message = "Could not delete issue! Check the logs."});
            }

        }

        private async Task<Negotiator> ChangeStatus(int issueId, IssueStatus status)
        {
            try
            {
                if (!Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests))
                {
                    return View["Index"];
                }


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
            if (!Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests))
            {
                return View["Index"];
            }

            var issue = await IssuesService.GetAsync(issueId);

            var toRemove = issue.Issues.FirstOrDefault(x => x.Issue == state);
            issue.Issues.Remove(toRemove);

            var result = await IssuesService.UpdateIssueAsync(issue);

            return result ? await Details(issueId) : View["Index"];
        }

        private async Task<Response> AddNote(int requestId, string noteArea, IssueState state)
        {
            if (!Security.HasAnyPermissions(User, Permissions.Administrator, Permissions.ManageRequests))
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Sorry, you do not have the correct permissions to add a note." });
            }

            var issue = await IssuesService.GetAsync(requestId);
            if (issue == null)
            {
                return Response.AsJson(new JsonResponseModel { Result = false, Message = "Issue does not exist to add a note!" });
            }
            var toAddNote = issue.Issues.FirstOrDefault(x => x.Issue == state);

            if (toAddNote != null)
            {
                issue.Issues.Remove(toAddNote);
                toAddNote.AdminNote = noteArea;
                issue.Issues.Add(toAddNote);
            }

            var result = await IssuesService.UpdateIssueAsync(issue);
            return Response.AsJson(result
                                       ? new JsonResponseModel { Result = true }
                                       : new JsonResponseModel { Result = false, Message = "Could not update the notes, please try again or check the logs" });
        }

        /// <summary>
        /// Orders the issues descending by the <see cref="IssueState"/>.
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <returns></returns>
        private IssuesModel Order(IssuesModel issues)
        {
            issues.Issues = issues.Issues.OrderByDescending(x => x.Issue).ToList();
            return issues;
        }
    }
}
