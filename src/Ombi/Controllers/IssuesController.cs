using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ombi.Attributes;

namespace Ombi.Controllers
{
    [ApiV1]
    [Authorize]
    [Produces("application/json")]
    public class IssuesController : Controller
    {
        public IssuesController(IRepository<IssueCategory> categories, IRepository<MovieIssues> movieIssues, IRepository<TvIssues> tvIssues, IRepository<IssueComments> comments)
        {
            _categories = categories;
            _movieIssues = movieIssues;
            _tvIssues = tvIssues;
            _issueComments = comments;
        }

        private readonly IRepository<IssueCategory> _categories;
        private readonly IRepository<MovieIssues> _movieIssues;
        private readonly IRepository<TvIssues> _tvIssues;
        private readonly IRepository<IssueComments> _issueComments;

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
            return await _movieIssues.GetAll().ToListAsync();
        }

        /// <summary>
        /// Retuns all the tv issues
        /// </summary>
        /// <returns></returns>
        [HttpGet("tv")]
        public async Task<IEnumerable<TvIssues>> GetTvIssues()
        {
            return await _tvIssues.GetAll().ToListAsync();
        }

        /// <summary>
        /// Create Movie Issue
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        [HttpPost("movie")]
        public async Task<int> CreateMovieIssue(MovieIssues movie)
        {
            await _movieIssues.Add(movie);
            return movie.Id;
        }

        /// <summary>
        /// Create TV Issue
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        [HttpPost("tv")]
        public async Task<int> CreateTvIssue(TvIssues tv)
        {
            await _tvIssues.Add(tv);
            return tv.Id;
        }

        [HttpGet("movie/comments/{movieId}")]
        public async Task<IEnumerable<IssueComments>> GetMovieComments(int movieId)
        {
            return await _issueComments.GetAll().Where(x => x.MovieIssueId == movieId).ToListAsync();
        }

        [HttpGet("tv/comments/{tvId}")]
        public async Task<IEnumerable<IssueComments>> GetTvComments(int tvId)
        {
            return await _issueComments.GetAll().Where(x => x.TvIssueId == tvId).ToListAsync();
        }
    }
}