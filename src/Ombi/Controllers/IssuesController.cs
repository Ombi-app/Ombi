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
        public IssuesController(IRepository<IssueCategory> categories, IRepository<MovieIssues> movieIssues, IRepository<TvIssues> tvIssues, IRepository<IssueComments> comments,
            UserManager<OmbiUser> userManager)
        {
            _categories = categories;
            _movieIssues = movieIssues;
            _tvIssues = tvIssues;
            _issueComments = comments;
            _userManager = userManager;
        }

        private readonly IRepository<IssueCategory> _categories;
        private readonly IRepository<MovieIssues> _movieIssues;
        private readonly IRepository<TvIssues> _tvIssues;
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
        [HttpGet("movie")]
        public async Task<IEnumerable<MovieIssues>> GetMovieIssues()
        {
            return await _movieIssues.GetAll().Include(x => x.Movie).Include(x => x.IssueCategory).ToListAsync();
        }

        /// <summary>
        /// Retuns all the tv issues
        /// </summary>
        /// <returns></returns>
        [HttpGet("tv")]
        public async Task<IEnumerable<TvIssues>> GetTvIssues()
        {
            return await _tvIssues.GetAll().Include(x => x.Child).Include(x => x.IssueCategory).ToListAsync();
        }

        /// <summary>
        /// Create Movie Issue
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        [HttpPost("movie")]
        public async Task<int> CreateMovieIssue([FromBody]MovieIssues movie)
        {
            movie.IssueCategory = null;
            await _movieIssues.Add(movie);
            return movie.Id;
        }

        /// <summary>
        /// Create TV Issue
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        [HttpPost("tv")]
        public async Task<int> CreateTvIssue([FromBody]TvIssues tv)
        {
            tv.IssueCategory = null;
            await _tvIssues.Add(tv);
            return tv.Id;
        }

        /// <summary>
        /// Returns the Movie issue by Id
        /// </summary>
        [HttpGet("movie/{id}")]
        public async Task<MovieIssues> GetMovieIssue([FromRoute] int id)
        {
            return await _movieIssues.GetAll().Where(x => x.Id == id).Include(x => x.Movie).Include(x => x.IssueCategory).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns the TV issue by Id
        /// </summary>
        [HttpGet("tv/{id}")]
        public async Task<TvIssues> GetTvIssue([FromRoute] int id)
        {
            return await _tvIssues.GetAll().Where(x => x.Id == id).Include(x => x.Child).Include(x => x.IssueCategory).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get's all the movie issue comments by id
        /// </summary>
        [HttpGet("movie/{movieId}/comments")]
        public async Task<IEnumerable<IssueCommentChatViewModel>> GetMovieComments([FromRoute]int movieId)
        {
            var comment = await _issueComments.GetAll().Where(x => x.MovieIssueId == movieId).Include(x => x.User).ToListAsync();
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
        /// Get's all the tv issue comments by id
        /// </summary>
        [HttpGet("tv/{tvId}/comments")]
        public async Task<IEnumerable<IssueCommentChatViewModel>> GetTvComments([FromRoute]int tvId)
        {
            var comment = await _issueComments.GetAll().Where(x => x.TvIssueId == tvId).Include(x => x.User).ToListAsync();
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
                MovieIssueId = comment.MovieIssueId,
                TvIssueId = comment.TvIssueId,
            };
            return await _issueComments.Add(newComment);
        }
    }
}