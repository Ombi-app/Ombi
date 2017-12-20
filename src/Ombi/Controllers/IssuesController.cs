using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ombi.Attributes;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Store.Entities;

namespace Ombi.Controllers
{
    [ApiV1]
    [Authorize]
    [Produces("application/json")]
    public class IssuesController : Controller
    {
        public IssuesController(IRepository<IssueCategory> categories, IRepository<Issues> issues, IRepository<IssueComments> comments,
            UserManager<OmbiUser> userManager)
        {
            _categories = categories;
            _issues = issues;
            _issueComments = comments;
            _userManager = userManager;
        }

        private readonly IRepository<IssueCategory> _categories;
        private readonly IRepository<Issues> _issues;
        private readonly IRepository<IssueComments> _issueComments;
        private readonly UserManager<OmbiUser> _userManager;

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
        /// Returns all the movie issues
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<Issues>> GetMovieIssues()
        {
            return await _issues.GetAll().Include(x => x.IssueCategory).ToListAsync();
        }

        /// <summary>
        /// Create Movie Issue
        /// </summary>
        [HttpPost]
        public async Task<int> CreateIssue([FromBody]Issues i)
        {
            i.IssueCategory = null;
            await _issues.Add(i);
            return i.Id;
        }

        /// <summary>
        /// Returns the Movie issue by Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<Issues> GetMovieIssue([FromRoute] int id)
        {
            return await _issues.GetAll().Where(x => x.Id == id).Include(x => x.IssueCategory).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get's all the movie issue comments by id
        /// </summary>
        [HttpGet("{id}/comments")]
        public async Task<IEnumerable<IssueCommentChatViewModel>> GetMovieComments([FromRoute]int id)
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
        public async Task<StatusCodeResult> UpdateStatus([FromBody] IssueStateViewModel model)
        {
            var issue = await _issues.Find(model.IssueId);
            if (issue == null)
            {
                return NotFound();
            }

            issue.Status = model.Status;
            await _issues.SaveChangesAsync();

            return Ok();
        }
    }
}