using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ombi.Attributes;
using Ombi.Core;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Notifications.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [Authorize]
    [Produces("application/json")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        public IssuesController(IRepository<IssueCategory> categories, IRepository<Issues> issues, IRepository<IssueComments> comments,
            UserManager<OmbiUser> userManager, INotificationHelper notify)
        {
            _categories = categories;
            _issues = issues;
            _issueComments = comments;
            _userManager = userManager;
            _notification = notify;
        }

        private readonly IRepository<IssueCategory> _categories;
        private readonly IRepository<Issues> _issues;
        private readonly IRepository<IssueComments> _issueComments;
        private readonly UserManager<OmbiUser> _userManager;
        private readonly INotificationHelper _notification;

        /// <summary>
        /// Get's all categories
        /// </summary>
        /// <returns></returns>
        [HttpGet("categories")]
        public async Task<IEnumerable<IssueCategory>> Categories()
        {
            return await _categories.GetAll().ToListAsync();
        }

        /// <summary>
        /// Creates a new category
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        [PowerUser]
        [HttpPost("categories")]
        public async Task<bool> CreateCategory([FromBody]IssueCategory cat)
        {
            var result = await _categories.Add(cat);
            if (result.Id > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes a Category
        /// </summary>
        /// <param name="catId"></param>
        /// <returns></returns>
        [PowerUser]
        [HttpDelete("categories/{catId}")]
        public async Task<bool> DeleteCategory([FromRoute]int catId)
        {
            var cat = await _categories.GetAll().FirstOrDefaultAsync(x => x.Id == catId);
            await _categories.Delete(cat);
            return true;
        }

        /// <summary>
        /// Returns all the issues
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<Issues>> GetIssues()
        {
            return await _issues.GetAll().Include(x => x.IssueCategory).ToListAsync();
        }

        /// <summary>
        /// Returns all the issues
        /// </summary>
        /// <returns></returns>
        [HttpGet("{take}/{skip}/{status}")]
        public async Task<IEnumerable<Issues>> GetIssues(int take, int skip, IssueStatus status)
        {
            return await _issues
                .GetAll()
                .Where(x => x.Status == status)
                .Include(x => x.IssueCategory)
                .Include(x => x.UserReported)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Returns all the issues count
        /// </summary>
        /// <returns></returns>
        [HttpGet("count")]
        public async Task<IssueCountModel> GetIssueCount()
        {
            return new IssueCountModel
            {
                Pending = await _issues.GetAll().Where(x => x.Status == IssueStatus.Pending).CountAsync(),
                InProgress = await _issues.GetAll().Where(x => x.Status == IssueStatus.InProgress).CountAsync(),
                Resolved = await _issues.GetAll().Where(x => x.Status == IssueStatus.Resolved).CountAsync()
            };
        }

        /// <summary>
        /// Create Movie Issue
        /// </summary>
        [HttpPost]
        public async Task<int> CreateIssue([FromBody]Issues i)
        {
            i.IssueCategory = null;
            i.CreatedDate = DateTime.UtcNow;
            var username = User.Identity.Name.ToUpper();
            i.UserReportedId = (await _userManager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username)).Id;
            await _issues.Add(i);
            var category = await _categories.GetAll().FirstOrDefaultAsync(x => i.IssueCategoryId == x.Id);
            if (category != null)
            {
                i.IssueCategory = category;
            }
            var notificationModel = new NotificationOptions
            {
                RequestId = i.RequestId ?? 0,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.Issue,
                RequestType = i.RequestType,
                Recipient = string.Empty,
                AdditionalInformation = $"{i.Subject} | {i.Description}",
                UserId =  i.UserReportedId,
                
            };

            AddIssueNotificationSubstitutes(notificationModel, i, User.Identity.Name);

            await _notification.Notify(notificationModel);

            return i.Id;
        }

        /// <summary>
        /// Returns the issue by Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<Issues> GetIssue([FromRoute] int id)
        {
            return await _issues.GetAll().Where(x => x.Id == id)
                .Include(x => x.IssueCategory)
                .Include(x => x.UserReported)
                .FirstOrDefaultAsync();
        }
        
        [HttpGet("request/{id}")]
        public async Task<IActionResult> GetIssueByRequestId([FromRoute] int id)
        {
            return new OkObjectResult(await _issues.GetAll().Where(x => x.RequestId == id)
                .Include(x => x.IssueCategory)
                .Include(x => x.UserReported).ToListAsync());
        }

                
        [HttpGet("provider/{id}")]
        public async Task<IActionResult> GetIssueByProviderId([FromRoute] string id)
        {
            return new OkObjectResult(await _issues.GetAll().Where(x => x.ProviderId == id)
                .Include(x => x.IssueCategory)
                .Include(x => x.UserReported).ToListAsync());
        }

        /// <summary>
        /// Get's all the issue comments by id
        /// </summary>
        [HttpGet("{id}/comments")]
        public async Task<IEnumerable<IssueCommentChatViewModel>> GetComments([FromRoute]int id)
        {
            var comment = await _issueComments.GetAll().Where(x => x.IssuesId == id).Include(x => x.User).ToListAsync();
            var vm = new List<IssueCommentChatViewModel>();

            foreach (var c in comment)
            {
                var roles = await _userManager.GetRolesAsync(c.User);
                vm.Add(new IssueCommentChatViewModel
                {
                    Id = c.Id,
                    Comment = c.Comment,
                    Date = c.Date,
                    Username = c.User.UserAlias,
                    AdminComment = roles.Contains(OmbiRoles.PowerUser) || roles.Contains(OmbiRoles.Admin) || c.User.IsSystemUser
                });
            }
            return vm;
        }

        /// <summary>
        /// Adds a comment on an issue
        /// </summary>
        [HttpPost("comments")]
        public async Task<IssueComments> AddComment([FromBody] NewIssueCommentViewModel comment)
        {
            var user = await _userManager.Users.Where(x => User.Identity.Name == x.UserName)
                .FirstOrDefaultAsync();
            var issue = await _issues.GetAll().Include(x => x.UserReported).Include(x => x.IssueCategory).FirstOrDefaultAsync(x => x.Id == comment.IssueId);
            if (issue == null)
            {
                return null;
            }
            var newComment = new IssueComments
            {
                Comment = comment.Comment,
                Date = DateTime.UtcNow,
                UserId = user.Id,
                IssuesId = comment.IssueId,
            };

            var notificationModel = new NotificationOptions
            {
                RequestId = issue.RequestId ?? 0,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.IssueComment,
                RequestType = issue.RequestType,
                UserId = user.Id
            };

            var isAdmin = await _userManager.IsInRoleAsync(user, OmbiRoles.Admin) || user.IsSystemUser;
            AddIssueNotificationSubstitutes(notificationModel, issue, issue.UserReported.UserAlias);
            notificationModel.Substitutes.Add("NewIssueComment", comment.Comment);
            notificationModel.Substitutes.Add("IssueId", comment.IssueId.ToString());
            notificationModel.Substitutes.Add("AdminComment", isAdmin.ToString());

            if (isAdmin)
            {
                // Send to user
                notificationModel.Recipient = issue.UserReported.Email;
            }
            else
            {
                notificationModel.Recipient = user.Email;
            }

            await _notification.Notify(notificationModel);

            return await _issueComments.Add(newComment);
        }
        /// <summary>
        /// Deletes a comment on a issue
        /// </summary>
        [HttpDelete("comments/{id}")]
        [PowerUser]
        public async Task<bool> DeleteComment(int id)
        {
            var comment = await _issueComments.GetAll().FirstOrDefaultAsync(x => x.Id == id);

            await _issueComments.Delete(comment);
            return true;
        }

        [HttpDelete("{id}")]
        [PowerUser]
        public async Task<bool> DeleteIssue(int id)
        {
            var issue = await _issues.GetAll().FirstOrDefaultAsync(x => x.Id == id);

            await _issues.Delete(issue);
            return true;
        }

        [HttpPost("status")]
        public async Task<bool> UpdateStatus([FromBody] IssueStateViewModel model)
        {
            var username = User.Identity.Name.ToUpper();
            var user = await _userManager.Users.Where(x => username == x.NormalizedUserName)
                .FirstOrDefaultAsync();
            var issue = await _issues.GetAll().Include(x => x.UserReported).Include(x => x.IssueCategory).FirstOrDefaultAsync(x => x.Id == model.IssueId);
            if (issue == null)
            {
                return false;
            }

            issue.Status = model.Status;

            if (model.Status == IssueStatus.Resolved)
            {
                issue.ResovledDate = DateTime.UtcNow;
            }
            await _issues.SaveChangesAsync();

            if (issue.Status == IssueStatus.Resolved)
            {
                var notificationModel = new NotificationOptions
                {
                    RequestId = 0,
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.IssueResolved,
                    RequestType = issue.RequestType,
                    Recipient = !string.IsNullOrEmpty(issue.UserReported?.Email)
                        ? issue.UserReported.Email
                        : string.Empty,
                    AdditionalInformation = $"{issue.Subject} | {issue.Description}",
                    UserId = user.Id,
                    
                };
                AddIssueNotificationSubstitutes(notificationModel, issue, issue.UserReported?.UserAlias ?? string.Empty);

                await _notification.Notify(notificationModel);
            }


            return true;
        }

        private static void AddIssueNotificationSubstitutes(NotificationOptions notificationModel, Issues issue, string issueReportedUsername)
        {
            notificationModel.Substitutes.Add("Title", issue.Title);
            notificationModel.Substitutes.Add("IssueDescription", issue.Description);
            notificationModel.Substitutes.Add("IssueCategory", issue.IssueCategory?.Value);
            notificationModel.Substitutes.Add("IssueStatus", issue.Status.ToString());
            notificationModel.Substitutes.Add("IssueSubject", issue.Subject);
            notificationModel.Substitutes.Add("IssueUser", issueReportedUsername);
            notificationModel.Substitutes.Add("RequestType", notificationModel.RequestType.ToString());
        }
    }
}