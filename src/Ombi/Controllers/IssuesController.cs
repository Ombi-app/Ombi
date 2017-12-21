﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ombi.Attributes;
using Ombi.Core;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Notifications.Models;
using Ombi.Store.Entities;

namespace Ombi.Controllers
{
    [ApiV1]
    [Authorize]
    [Produces("application/json")]
    public class IssuesController : Controller
    {
        public IssuesController(IRepository<IssueCategory> categories, IRepository<Issues> issues, IRepository<IssueComments> comments,
            UserManager<OmbiUser> userManager, INotificationService notify)
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
        private readonly INotificationService _notification;

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
            i.UserReportedId = (await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name)).Id;
            await _issues.Add(i);

            var notificationModel = new NotificationOptions
            {
                RequestId = 0,
                DateTime = DateTime.Now,
                NotificationType = NotificationType.Issue,
                RequestType = i.RequestType,
                Recipient = string.Empty,
                AdditionalInformation = $"{i.Subject} | {i.Description}"
            };

            BackgroundJob.Enqueue(() => _notification.Publish(notificationModel));

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
                    Comment = c.Comment,
                    Date = c.Date,
                    Username = c.User.UserAlias,
                    AdminComment = roles.Contains(OmbiRoles.PowerUser) || roles.Contains(OmbiRoles.Admin)
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
            var userId = await _userManager.Users.Where(x => User.Identity.Name == x.UserName).Select(x => x.Id)
                .FirstOrDefaultAsync();
            var newComment = new IssueComments
            {
                Comment = comment.Comment,
                Date = DateTime.UtcNow,
                UserId = userId,
                IssuesId = comment.IssueId,
            };
            return await _issueComments.Add(newComment);
        }

        [HttpPost("status")]
        public async Task<bool> UpdateStatus([FromBody] IssueStateViewModel model)
        {
            var issue = await _issues.Find(model.IssueId);
            if (issue == null)
            {
                return false;
            }

            issue.Status = model.Status;
            await _issues.SaveChangesAsync();

            return true;
        }
    }
}